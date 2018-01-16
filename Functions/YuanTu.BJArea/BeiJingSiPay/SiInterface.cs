using MedicareComLib;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.BJArea.BeiJingSiPay
{
    public class SiInterface
    {

        private const string Prefix = "社保接口";
        
        public static Result<TRes> Handle<TReq, TRes>(TReq req)
            where TReq : Req
            where TRes : Res
        {
            try
            {
//#warning mock数据
//                FrameworkConst.FakeServer = true;
//                FrameworkConst.HospitalId = "1";
//#warning mock数据

                var SiModel = ServiceLocator.Current.GetInstance<ISiModel>();
                var reqString = XmlHelper.Serialize(req);
                Logger.Net.Info($"[{Prefix}] [{req.ServiceName}] 发送内容: {reqString}");
                var watch = Stopwatch.StartNew();
                string resString = string.Empty;

                if (FrameworkConst.FakeServer)
                {
                    var fakerdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer");
                    var file = Path.Combine(fakerdir, FrameworkConst.HospitalId, $"{req.ServiceName}.xml");
                    if (!File.Exists(file))
                    {
                        file = Path.Combine(fakerdir, $"{req.ServiceName}.xml");
                        if (!File.Exists(file))
                            return Result<TRes>.Fail($"模拟参数{file}未找到");
                    }
                    using (var sr = new StreamReader(file))
                    {
                        resString = sr.ReadToEnd();
                    }

                }
                else {
                    if (SiModel.Siobj == null) { SiModel.Siobj = new MedicareComLib.OutpatientClass(); }
                    //MedicareComLib.OutpatientClass medicareComLib = new MedicareComLib.OutpatientClass();
                    switch (req.ServiceName)
                    {
                        case "Open":
                            SiModel.Siobj.Open(out resString);
                            break;
                        case "Close":
                            SiModel.Siobj.Close(out resString);
                            break;
                        case "GetPersonInfo":
                            SiModel.Siobj.GetPersonInfo(out resString);
                            break;
                        case "Divide":
                            SiModel.Siobj.Divide(reqString, out resString);
                            break;
                        case "Trade":
                            SiModel.Siobj.Trade(out resString);
                            break;
                        case "GetCardInfo":
                            SiModel.Siobj.GetCardInfo(out resString);
                            break;

                    }

                    watch.Stop();
                    var time = watch.ElapsedMilliseconds;
                    Logger.Net.Info($"[{Prefix}] [{req.ServiceName}] 耗时:{time}毫秒 接收内容:{resString}");
                }
                var res = XmlHelper.Deserialize<TRes>(resString);

                if (res.state.success == "false")
                {
                    if (req.ServiceName == "Open") { SiModel.IsOpened = false; }
                    return Result<TRes>.Fail(FromartErrorWaring(res.state));
                }
                if (req.ServiceName == "Open") { SiModel.IsOpened = true; }
                if (req.ServiceName == "Close") { SiModel.IsOpened = false; }
                return Result<TRes>.Success(res);

            }
            catch (Exception ex)
            {
                return Result<TRes>.Fail(ex.Message, ex);
            }
        }
        private static string FromartErrorWaring(State state)
        {
            try
            {
                return "";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}\r\n{ex.InnerException}";
            }
        }
    }

}
