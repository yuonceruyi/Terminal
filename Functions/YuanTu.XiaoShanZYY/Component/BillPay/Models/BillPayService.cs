using System.Linq;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.XiaoShanZYY.Component.Auth.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.BillPay.Models
{
    public interface IBillPayService : IService
    {
        Result 获取费用明细();
        Result 缴费预结算();
        Result 缴费结算();
    }

    internal class BillPayService : IBillPayService
    {
        public string ServiceName { get; }

        public Result 获取费用明细()
        {
            var Auth = GetAuthModel();
            var BillPay = GetBillPayModel();
            var req = new Req门诊费用明细
            {
                JIUZHENKH = Auth.人员信息.就诊卡号,
                JIUZHENKLX = " ",
                BINGRENLB = Auth.Info.PatientType,
                BINGRENXZ = " ",
                YIBAOKLX = " ",
                YIBAOKMM = " ",
                YIBAOKXX = " ",
                YIBAOBRXX = " ",
                YILIAOLB = " ",
                JIESUANLB = " ",
                HISBRXX = " ",
            };
            var result = DataHandler.门诊费用明细(req);
            if (!result.IsSuccess)
                return result.Convert();
            var res = result.Value;
            if (!res.FEIYONGMX.Any())
                return Result.Fail("列表为空");
            BillPay.FEIYONGMX = res.FEIYONGMX;
            BillPay.JIBINGMX = res.JIBINGMX;
            decimal sum = 0;
            foreach (var tab in res.FEIYONGMX)
                sum += decimal.Parse(tab.JINE);
            BillPay.Sum = sum * 100;
            return Result.Success();
        }

        private IBillPayModel GetBillPayModel()
        {
            return ServiceLocator.Current.GetInstance<IBillPayModel>();
        }

        private IAuthModel GetAuthModel()
        {
            return ServiceLocator.Current.GetInstance<IAuthModel>();
        }

        public Result 缴费预结算()
        {
            var Auth = GetAuthModel();
            var BillPay = GetBillPayModel();
            var req = new ReqDll
            {
                卡号 = Auth.人员信息.就诊卡号,
                结算类型 = "03",
                病人类别 = Auth.人员信息.病人类别,
                结算方式 = "1",
            };
            var result = DataHandler.预结算(req);
            if (!result.IsSuccess)
                return result.Convert();
            var res = result.Value;
            BillPay.Res预结算 = res;
            return Result.Success();
        }

        public Result 缴费结算()
        {
            var Auth = GetAuthModel();
            var BillPay = GetBillPayModel();
            var paymentModel = ServiceLocator.Current.GetInstance<IPaymentModel>();
            var req = new ReqDll
            {
                卡号 = Auth.人员信息.就诊卡号,
                结算类型 = "03",
                病人类别 = Auth.人员信息.病人类别,
                结算方式 = "2",
                应付金额 = paymentModel.Self.ToString("F2"),
                设备编码 = FrameworkConst.OperatorId,
            };
            if (!paymentModel.NoPay)
            {
                var epm = ServiceLocator.Current.GetInstance<IExtraPaymentModel>();
                var payMethod = epm.CurrentPayMethod;

                req.支付金额 = (epm.TotalMoney/100).ToString("F2");

                switch (payMethod)
                {
                    case PayMethod.银联:
                        var posinfo = epm.PaymentResult as TransResDto;
                        if (posinfo != null)
                        {
                            req.支付流水号 = posinfo.Ref;
                            //req.结算流水号 = posinfo.Trace;
                            req.支付时间 = $"{posinfo.TransDate} {posinfo.TransTime}";
                            req.支付方式 = "27";
                        }
                        break;
                    case PayMethod.支付宝:
                    case PayMethod.微信支付:
                        var tpi = epm.PaymentResult as 订单状态;
                        if (tpi != null)
                        {
                            req.支付流水号 = tpi.outPayNo;
                            req.结算流水号 = tpi.outTradeNo;
                            req.支付时间 = tpi.paymentTime;
                            req.支付方式 = payMethod == PayMethod.支付宝 ? "25" : "26";
                        }
                        break;
                }
            }
            var result = DataHandler.结算(req);
            if (!result.IsSuccess)
                return result.Convert();
            var res = result.Value;
            BillPay.Res结算 = res;
            return Result.Success();
        }
    }
}