using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Log;
using YuanTu.ISO8583.Util;
using YuanTu.ZheJiangHospital.Appoint;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Appoint
{
    internal class AppointModel : ModelBase, IAppointModel
    {
        public 科室信息[] 科室信息List { get; set; }
        public 科室信息 科室信息 { get; set; }
        public 排班信息[] 排班信息List { get; set; }
        public 排班信息 排班信息 { get; set; }
        public 号源信息[] 号源信息List { get; set; }
        public 号源信息 号源信息 { get; set; }
        public Res预约挂号 Res预约挂号 { get; set; }
        public Res取消预约 Res取消预约 { get; set; }
        public string OrderId { get; set; }
        public string Pass { get; set; }
    }

    public interface IAppointModel : IModel
    {
        科室信息[] 科室信息List { get; set; }

        科室信息 科室信息 { get; set; }

        排班信息[] 排班信息List { get; set; }

        排班信息 排班信息 { get; set; }

        号源信息[] 号源信息List { get; set; }

        号源信息 号源信息 { get; set; }

        Res预约挂号 Res预约挂号 { get; set; }
        Res取消预约 Res取消预约 { get; set; }
        string OrderId { get; set; }
        string Pass { get; set; }
    }

    internal class AppointService
    {
        public static string AppointAddress { get; set; } = "http://192.17.2.100:9988/server/open.asmx";

        public string ServiceName { get; } = "AppointService";

        private static readonly string Dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeServer", "160");

        private static readonly openSoap Client = new openSoapClient(
            new CustomBinding
            {
                Elements =
                {
                    new TextMessageEncodingBindingElement
                    {
                        MessageVersion = MessageVersion.Soap12
                    },
                    new HttpTransportBindingElement
                    {
                        MaxReceivedMessageSize = 1024 * 1024
                    }
                }
            },
            new EndpointAddress(AppointAddress));

        //public IAppointModel Appoint { get; set; }


        //public IChaKaModel ChaKa { get; set; }

        //public IPrintService PrintService { get; set; }
        
        //private Queue<IPrintable> GenYuYue()
        //{
        //    throw new NotImplementedException();
        //    //var queue = PrintService.NewQueue("预约挂号单");
        //    //var sb = new StringBuilder();


        //    //sb.Append($"状态：预约成功\n");
        //    //sb.Append($"姓名：{ChaKa.Name}\n");
        //    //sb.Append($"交易类型：预约挂号\n");
        //    //sb.Append($"科室名称：{YuYue.排班信息.deptname}\n");
        //    //sb.Append($"就诊医生：{YuYue.排班信息.docname}\n");
        //    //sb.Append($"就诊时间：{YuYue.排班信息.schdate}\n");
        //    //sb.Append($"就诊场次：{YuYue.排班信息.ampm.Parse门诊时间()}\n");
        //    //sb.Append($"就诊地址：{YuYue.科室信息.deptaddress}\n");
        //    //sb.Append($"挂号序号：{YuYue.号源信息.numno}\n");
        //    //sb.Append($"取号密码：{YuYue.Pass}\n");
        //    //sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
        //    //sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
        //    //sb.Append($"请于当日取号就诊。\n");
        //    //sb.Append($"祝您早日康复！\n");

        //    //queue.Enqueue(new PrintItemText()
        //    //{
        //    //    Text = sb.ToString()
        //    //});

        //    //return queue;
        //}

        //public Result 科室列表查询()
        //{
        //    var req = new Req科室列表查询();
        //    var result = Run<Res科室列表查询, Req科室列表查询>(req);
        //    if (!result.IsSuccess)
        //        return Result.Fail(result.Message);
        //    var res = result.Value;
        //    Appoint.科室信息List = res.list;

        //    return Result.Success();
        //}

        //public Result 科室排班查询()
        //{
        //    var req = new Req科室排班查询
        //    {
        //        deptid = Appoint.科室信息.departID
        //    };
        //    var result = Run<Res科室排班查询, Req科室排班查询>(req);
        //    if (!result.IsSuccess)
        //        return Result.Fail(result.Message);
        //    var res = result.Value;

        //    Appoint.排班信息List = res.list;

        //    return Result.Success();
        //}

        //public Result 排班号源查询()
        //{
        //    var req = new Req排班号源查询
        //    {
        //        schid = Appoint.排班信息.schid
        //    };

        //    var result = Run<Res排班号源查询, Req排班号源查询>(req);
        //    if (!result.IsSuccess)
        //        return Result.Fail(result.Message);
        //    var res = result.Value;

        //    Appoint.号源信息List = res.list;

        //    return Result.Success();
        //}

        //public Result 预约挂号()
        //{
        //    var req = new Req预约挂号
        //    {
        //        numid = Appoint.号源信息.numid,
        //        no = Appoint.号源信息.numno,
        //        //patname = ChaKa.Name,
        //        //patsex = ChaKa.Gender == "男" ? "1" : "2",
        //        //mobileno = ChaKa.Phone,
        //        //idcardtype = "1",
        //        //idcard = ChaKa.IDNo,
        //        pass = Appoint.Pass,
        //        oper = FrameworkConst.OperatorId,
        //        appID = "10009-101",
        //        time = DateTimeCore.Now.ToString("yyyyMMdd HHmmss")
        //    };
        //    var dataBytes = Encoding.UTF8.GetBytes($"{req.appID}2C1C6A5056DC79A5{req.time}{req.funcode}");
        //    req.captcha = Convert.ToBase64String(MD5.ComputeHash(dataBytes));

        //    var result = Run<Res预约挂号, Req预约挂号>(req);
        //    if (!result.IsSuccess)
        //        return Result.Fail(result.Message);
        //    var res = result.Value;

        //    Appoint.Res预约挂号 = res;

        //    return Result.Success();
        //}

        //public Result 取消预约()
        //{
        //    var req = new Req取消预约
        //    {
        //        orderid = Appoint.OrderId,
        //        pass = Appoint.Pass,
        //        appID = "10009-101",
        //        time = DateTimeCore.Now.ToString("yyyyMMdd HHmmss")
        //    };
        //    var data = $"{req.appID}2C1C6A5056DC79A5{req.time}{req.funcode}";
        //    var dataBytes = Encoding.UTF8.GetBytes(data);
        //    req.captcha = Convert.ToBase64String(MD5.ComputeHash(dataBytes));

        //    var result = Run<Res取消预约, Req取消预约>(req);
        //    if (!result.IsSuccess)
        //        return Result.Fail(result.Message);
        //    var res = result.Value;

        //    Appoint.Res取消预约 = res;

        //    return Result.Success();
        //}

        private static string Mock(Req req)
        {
            var fileName = Path.Combine(Dir, $"{req.funcode}.xml");
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            return XmlHelper.Serialize(new Res { state = -1, result = $"模拟数据未实现:{req.funcode}" });
        }

        public static Result<TRes> Run<TRes, TReq>(TReq req)
            where TReq : Req
            where TRes : Res
        {
            try
            {
                var reqString = XmlHelper.Serialize(req);

                Logger.Net.Info($"[{DataHandler.Count}][{req.ServiceName}]Send: {reqString}");
                var sw = Stopwatch.StartNew();
                string resString;
                if (FrameworkConst.FakeServer)
                    resString = Mock(req);
                else
                    resString = Client.funMain(reqString);
                sw.Stop();
                Logger.Net.Info($"[{DataHandler.Count}][{req.ServiceName}]Recv:{sw.ElapsedMilliseconds}ms {resString}");

                var res = XmlHelper.Deserialize<TRes>(resString);

                if (res.state != 0)
                    return Result<TRes>.Fail(res.result);

                return Result<TRes>.Success(res);
            }
            catch (Exception ex)
            {
                return Result<TRes>.Fail(ex.Message, ex);
            }
        }

        private static readonly MD5 Md5 = MD5.Create();
        public static string GetCaptcha(string s)
        {
            return Md5.ComputeHash(Encoding.UTF8.GetBytes(s)).Bytes2Hex();
        }
    }
}
