using System;
using Newtonsoft.Json;
using YuanTu.ShenZhenArea.Gateway;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ShenZhenArea.Models;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts;
using YuanTu.ShenZhenArea.Insurance;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.UnionPay;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Models.Register;

namespace YuanTu.ShenZhenArea.Services
{
    public partial class AccountingBase
    {
        /// <summary>
        /// 交易记账地址
        /// </summary>
        public static string JiaoYiAccountingUrl { get; set; }
        /// <summary>
        /// 医保记账地址
        /// </summary>
        public static string InsuranceAccountingUrl { get; set; }
    }

    /// <summary>
    /// 记账服务
    /// </summary>
    public interface IAccountingService : IService
    {
        void 充值记账(bool success, bool unKownResult = false);

        void 门诊结算记账(bool success, bool unKownResult = false);


        void 门诊挂号记账(bool success, bool unKownResult = false);



        void 住院押金记账(bool success, bool unKownResult = false);

        void 医保消费记账(bool success);

    }

    /// <summary>
    /// 记账服务
    /// </summary>

    public class AccountingService : ServiceBase, IAccountingService
    {

        [Dependency]
        public IYBModel YBModel { get; set; }

        [Dependency]
        public IBillRecordModel RecordModel { get; set; }

        [Dependency]
        public IOpRechargeModel OpRechargeModel { get; set; }

        [Dependency]
        public IIpRechargeModel IpRechargeModel { get; set; }

        [Dependency]
        public IBillPayModel BillPayModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IMisposUnionService MisposUnionService { get; set; }


        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        public string ServiceName
        {
            get;
        }

        public void 充值记账(bool success, bool unKownResult = false)
        {
            string zhuangtai;
            if (unKownResult)
                zhuangtai = "处理结果未知";
            else zhuangtai = success ? "交易成功" : "交易失败";
            TransResDto posResDo = ExtraPaymentModel.PaymentResult as TransResDto;
            var req = new req交易记账
            {
                jiaoyishijian = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                jiaoyima = "PRE",
                kahao = CardModel.CardNo,
                shoufeiID = "",
                jiaoyijine = OpRechargeModel.Req预缴金充值.cash,
                yinhangkahao = posResDo?.CardNo ?? "",
                yinlianjiaoyiliushuihao = posResDo?.Ref ?? OpRechargeModel.Req预缴金充值.flowId,
                yinlianjiaoyiriqi = posResDo?.TransDate ?? "",
                zhuangtai = zhuangtai,
                yinlianpicihao = posResDo?.Batch ?? "",
                yinlianpingzhenghao = posResDo?.Trace ?? "",
                yinliankaleixingID = "",
                zhongduanhao = posResDo?.TId ?? "",
                caozuoyuanhao = FrameworkConst.OperatorId,
                tradeMode= OpRechargeModel.Req预缴金充值.tradeMode,
                tradeTime= OpRechargeModel.Req预缴金充值.tradeTime
            };
            HttpPost.PostData(JsonConvert.SerializeObject(req), AccountingBase.JiaoYiAccountingUrl);
        }

        public void 门诊结算记账(bool success, bool unKownResult = false)
        {
            string zhuangtai;
            if (unKownResult)
                zhuangtai = "处理结果未知";
            else zhuangtai = success ? "交易成功" : "交易失败";
            TransResDto posResDo = ExtraPaymentModel.PaymentResult as TransResDto;
            var req = new req交易记账
            {
                jiaoyishijian = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                jiaoyima = "INV",
                kahao = CardModel.CardNo,
                shoufeiID = BillPayModel.Res缴费结算.data?.receiptNo ?? "",//.Req缴费结算?.flowId,   INV记账时收费ID为发票号
                jiaoyijine = PaymentModel.Total.ToString(),
                yinhangkahao = posResDo?.CardNo ?? "",
                yinlianjiaoyiliushuihao = posResDo?.Ref ?? "",
                yinlianjiaoyiriqi = posResDo?.TransDate ?? "",
                zhuangtai = zhuangtai,
                yinlianpicihao = posResDo?.Batch ?? "",
                yinlianpingzhenghao = posResDo?.Trace ?? "",
                yinliankaleixingID = "",
                zhongduanhao = posResDo?.TId ?? "",
                caozuoyuanhao = FrameworkConst.OperatorId,
                tradeTime = BillPayModel.Req缴费结算.tradeTime,
                tradeMode = BillPayModel.Req缴费结算.tradeMode
            };
            HttpPost.PostData(JsonConvert.SerializeObject(req), AccountingBase.JiaoYiAccountingUrl);
        }
        public void 门诊挂号记账(bool success, bool unKownResult = false)
        {
            string zhuangtai;
            if (unKownResult)
                zhuangtai = "处理结果未知";
            else zhuangtai = success ? "交易成功" : "交易失败";
            TransResDto posResDo = ExtraPaymentModel.PaymentResult as TransResDto;
            var req = new req交易记账
            {
                jiaoyishijian = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                jiaoyima = "INV",
                kahao = CardModel.CardNo,
                shoufeiID = RegisterModel.Res预约挂号?.data?.receiptNo,  //收费ID  
                jiaoyijine = PaymentModel.Total.ToString(),
                yinhangkahao = posResDo?.CardNo ?? "",
                yinlianjiaoyiliushuihao = posResDo?.Ref ?? "",
                yinlianjiaoyiriqi = posResDo?.TransDate ?? "",
                zhuangtai = zhuangtai,
                yinlianpicihao = posResDo?.Batch ?? "",
                yinlianpingzhenghao = posResDo?.Trace ?? "",
                yinliankaleixingID = "",
                zhongduanhao = posResDo?.TId ?? "",
                caozuoyuanhao = FrameworkConst.OperatorId,
                tradeTime = RegisterModel.Req预约挂号.tradeTime,
                tradeMode = RegisterModel.Req预约挂号.tradeMode
            };
            HttpPost.PostData(JsonConvert.SerializeObject(req), AccountingBase.JiaoYiAccountingUrl);
        }
        public void 住院押金记账(bool success, bool unKownResult = false)
        {
            string zhuangtai;
            if (unKownResult)
                zhuangtai = "处理结果未知";
            else zhuangtai = success ? "缴押金成功" : "缴押金失败";
            TransResDto posResDo = ExtraPaymentModel.PaymentResult as TransResDto;
            var req = new req交易记账
            {
                jiaoyishijian = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                jiaoyima = "DEP",
                kahao = CardModel.CardNo,
                shoufeiID = IpRechargeModel.Res住院预缴金充值?.data?.receiptNo??"",//.Req住院预缴金充值?.flowId,     DEP记账时收费ID为发票号
                jiaoyijine = IpRechargeModel.Req住院预缴金充值.cash,
                yinhangkahao = posResDo?.CardNo ?? "",
                yinlianjiaoyiliushuihao = posResDo?.Ref ?? "",
                yinlianjiaoyiriqi = posResDo?.TransDate ?? "",
                zhuangtai = zhuangtai,
                yinlianpicihao = posResDo?.Batch ?? "",
                yinlianpingzhenghao = posResDo?.Trace,
                yinliankaleixingID = "",
                zhongduanhao = posResDo?.TId ?? "",
                caozuoyuanhao = FrameworkConst.OperatorId,
                tradeTime = IpRechargeModel.Req住院预缴金充值.tradeTime,
                tradeMode = IpRechargeModel.Req住院预缴金充值.tradeMode
            };
            HttpPost.PostData(JsonConvert.SerializeObject(req), AccountingBase.JiaoYiAccountingUrl);
        }

        public void 医保消费记账(bool success)
        {
            //医保记账
            var req = new req医保消费记账
            {
                ylzh = YBModel.医疗证号,
                name = PatientModel.当前病人信息.name,
                danjuhao = YBModel.djh,
                feiyongheji = PaymentModel.Total.ToString(),
                jizhang = PaymentModel.Insurance.ToString(),
                xianjin = PaymentModel.Self.ToString(),
                czybm = YBBase.czybm,
                czyxm = YBBase.czyxm,
                jstime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            HttpPost.PostData(JsonConvert.SerializeObject(req), AccountingBase.InsuranceAccountingUrl);
        }
    }
}