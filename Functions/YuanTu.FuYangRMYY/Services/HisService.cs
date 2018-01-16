using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.FuYangRMYY.Component.SignIn.Models;
using YuanTu.FuYangRMYY.HisNative.Models;
using YuanTu.FuYangRMYY.HisOpBillAutoPayService;
using YuanTu.FuYangRMYY.HisRegisterService;

namespace YuanTu.FuYangRMYY.Services
{
    public class HisService
    {
        private static readonly SelfRegServiceSoapClient RegClient = new SelfRegServiceSoapClient(new BasicHttpBinding(),
           // new EndpointAddress("http://ngrok.yuantutech.com:47004/DTHealth/web/RegInterface.Service.SelfRegService.cls"));
            new EndpointAddress("http://172.100.100.188/DTHealth/web/RegInterface.Service.SelfRegService.cls"));

        private static readonly DHCOPBillAutoPaySOAPSoapClient OpBillAutoPayClient=new DHCOPBillAutoPaySOAPSoapClient(new BasicHttpBinding(),
            // new EndpointAddress("http://ngrok.yuantutech.com:47004/DTHealth/web/web.DHCOPBillAutoPaySOAP.cls"));
            new EndpointAddress("http://172.100.100.188/DTHealth/web/web.DHCOPBillAutoPaySOAP.cls"));



        private const string Prefiex = "HIS接口直连";
        private static int Index=0;
        public static Result<RegisterListResponse> GetRegisterInfo(病人信息 info)
        {
            var curIndex = Interlocked.Increment(ref Index);
            try
            {
                var req = RegClient.QueryAdmOPReg($@"<Request>
    <TradeCode>1104</TradeCode>
    <ExtOrgCode></ExtOrgCode>
    <ClientType></ClientType>
    <HospitalId></HospitalId>
    <ExtUserID></ExtUserID>
    <CardType>02</CardType>
    <PatientCard>{info.cardNo}</PatientCard>
    <PatientID>{info.patientId}</PatientID>
    <StartDate>{DateTimeCore.Now:yyyy-MM-dd}</StartDate>
    <EndDate>{DateTimeCore.Now:yyyy-MM-dd}</EndDate>
</Request>");
                Logger.Net.Info($"[{Prefiex}][{curIndex}][获取挂号信息]入参:{req.ToJsonString()}");
                var watch = Stopwatch.StartNew();
                var val = Deserialize<RegisterListResponse>(req);
                watch.Stop();
                Logger.Net.Info(
                    $"[{Prefiex}][{curIndex}][获取挂号信息]返回:{val.ToJsonString()} 耗时:{watch.ElapsedMilliseconds}ms");
                if (val.ResultCode == 0)
                {
                    return Result<RegisterListResponse>.Success(val);
                }

                return Result<RegisterListResponse>.Fail(val.ErrMsg + val.ResultContent);
            }

            catch (Exception ex)
            {
                return Result<RegisterListResponse>.Fail(ex.Message);
            }
        }

        public static Result<SignInfoResponse> StartSignIn(病人信息 info, ResponseOrder order)
        {
            var curIndex = Interlocked.Increment(ref Index);
            try
            {
                var req = $@"<Request>
    <TradeCode>1103</TradeCode>
    <UserID>{FrameworkConst.OperatorId}</UserID>
    <RegID>{order.RegID}</RegID>
    <QueueID>{order.QueueID}</QueueID>
</Request>";
                Logger.Net.Info($"[{Prefiex}][{curIndex}][开始签到]入参:{req.ToJsonString()}");
                var watch = Stopwatch.StartNew();
                var resp = RegClient.AgainReQueue(req);
                watch.Stop();
                Logger.Net.Info(
                    $"[{Prefiex}][{curIndex}][开始签到]返回:{resp.ToJsonString()} 耗时:{watch.ElapsedMilliseconds}ms");

                var val = Deserialize<SignInfoResponse>(resp);
                if (val.ResultCode == 0)
                {
                    return Result<SignInfoResponse>.Success(val);
                }

                return Result<SignInfoResponse>.Fail(val.ErrMsg + val.ResultContent);

            }
            catch (Exception ex)
            {
                
                return Result<SignInfoResponse>.Fail(ex.Message);
            }
        }

        public static Result<RegisterInsuranceInfoResponse> GetRegisterInsuranceParams(病人信息 info, string scheduleId)
        {
            var curIndex = Interlocked.Increment(ref Index);
            var req = $@"<Request>
    <TabASRowId>{scheduleId}</TabASRowId>
    <PatientID>{info.patientId}</PatientID>
    <AdmReason>26</AdmReason>
    <UserID>{FrameworkConst.OperatorId}</UserID>
    <MedicalBookFlag></MedicalBookFlag>
</Request>";

            Logger.Net.Info($"[{Prefiex}][{curIndex}][获取挂号社保参数]入参:{req}");
            var watch = Stopwatch.StartNew();
            var resp = RegClient.GetRegisterExStr(req);
            watch.Stop();
            Logger.Net.Info(
                $"[{Prefiex}][{curIndex}][获取挂号社保参数]返回:{resp} 耗时:{watch.ElapsedMilliseconds}ms");

           
            var val = Deserialize<RegisterInsuranceInfoResponse>(resp);
            if (val.ResultCode==0)
            {
                return Result<RegisterInsuranceInfoResponse>.Success(val);
            }
            return Result<RegisterInsuranceInfoResponse>.Fail(val.ErrMsg + val.ResultContent);
        }

        //public static void Temp()
        //{
        //    Client.get
        //}

        public static Result<BillpayInsuranceInfo> ConfirmSiBillPay(bool success, 病人信息 info,缴费概要信息 billInfo,结算结果 payResult)
        {
            var curIndex = Interlocked.Increment(ref Index);
           
           
               var request = $@"<Request>
    <CardNo>{info.cardNo}</CardNo>
    <SecrityNo></SecrityNo>
    <Userid>{FrameworkConst.OperatorId}</Userid>
    <StartDate>{billInfo.billDate}</StartDate>
    <EndDate>{billInfo.billDate}</EndDate>
    <Invoices>
        <InvoiceBankInfo>
            <TransactionId></TransactionId>
            <InvoiceNO>{payResult.receiptNo}</InvoiceNO>
            <InvoiceAmt>{payResult.payAccount}</InvoiceAmt>
            <PrescWindow></PrescWindow>
            <InvocieExpStr></InvocieExpStr>
            <InvocieBankInfoStr>{(success?"0000":"1111")}</InvocieBankInfoStr>
        </InvoiceBankInfo>
    </Invoices>
</Request>";
            Logger.Net.Info($"[{Prefiex}][{curIndex}][缴费结果确认]入参:{request}");
            var watch = Stopwatch.StartNew();
            var resp = OpBillAutoPayClient.InsertBankTradeInfoS(request); //RegClient.GetRegisterExStr(request);
            watch.Stop();
            Logger.Net.Info(
                $"[{Prefiex}][{curIndex}][缴费结果确认]返回:{resp} 耗时:{watch.ElapsedMilliseconds}ms");

            var val = Deserialize<BillpayInsuranceInfo>(resp);
            if (val.ResultCode == 0)
            {
                return Result<BillpayInsuranceInfo>.Success(val);
            }
            return Result<BillpayInsuranceInfo>.Fail(val.ErrMsg +val.ErrorMsg+ val.ResultContent);
        }


        private static T Deserialize<T>(string input)
        {
            using (var sr = new StringReader(input))
            {
                System.Xml.Serialization.XmlSerializer xmlSerializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(T));
                var result = (T)xmlSerializer.Deserialize(sr) ;
                return result;
              
            }
        }
    }
}
