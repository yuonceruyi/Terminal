using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.XiaoShanZYY.HIS
{
    public class HISConnection
    {
        public static Result<T> Handle<T>(Req req)
            where T : Res
        {
            try
            {
                var send = XmlHelper.Serilize(req);
                Logger.Net.Info($"[{req.Service}]Send: {send}");
                string recv;
                int ret;
                if (Startup.HISWebService)
                {
                    recv = Mock(req, out ret);
                }
                else
                {
                    string strRecv = string.Empty;
                    ret = Startup.HisService.runservice($"HIS1.Biz.{req.Service}", send, ref strRecv);
                    recv = strRecv;
                }
                Logger.Net.Info($"[{req.Service}]Ret={ret} Recv: {recv}");

                if (string.IsNullOrEmpty(recv))
                    return Result<T>.Fail("HIS返回数据为空");

                if (ret < 0)
                {
                    try
                    {
                        var doc = XDocument.Parse(recv);
                        var msg = doc.Root?.Descendants("ERRMSG").FirstOrDefault()?.Value;
                        return Result<T>.Fail(ret, msg);
                    }
                    catch (Exception ex)
                    {
                        return Result<T>.Fail($"解析返回数据失败:{ex.Message}", ex);
                    }
                }

                var res = new XmlSerializer(typeof(T))
                    .Deserialize(new StringReader(recv)) as T;
                if (res == null)
                    return Result<T>.Fail("解析返回数据失败");

                if (string.IsNullOrEmpty(res.OUTMSG?.ERRNO))
                    return Result<T>.Fail("解析返回数据失败:OUTMSG");
                var errorNo = res.OUTMSG.ERRNO;
                if (!int.TryParse(errorNo, out int errorNoInt))
                    return Result<T>.Fail("解析返回数据失败:OUTMSG.ERRNO");
                if (errorNoInt < 0)
                {
                    return Result<T>.Fail(errorNoInt, res.OUTMSG.ERRMSG);
                }

                return Result<T>.Success(res);
            }
            catch (SocketException ex)
            {
                Logger.Main.Error($"网络通信出错{req.Service}\n{ex}");
                return Result<T>.Fail("网络通信出错", ex);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"出错{req.Service}\n{ex}");
                return Result<T>.Fail("出错", ex);
            }
        }
        
        public static string Mock(Req req, out int ret)
        {
            var path = $"FakeServer\\XiaoShanZYY\\{req.Service}.xml";
            if (File.Exists(path))
            {
                ret = 0;
                return File.ReadAllText(path);
            }
            ret = -1;
            return XmlHelper.Serilize(new Res()
            {
                OUTMSG = new OUTMSG()
                {
                    ERRNO = "-1",
                    ERRMSG = "未找到模拟数据:"+path
                }
            });
        }
    }
}
