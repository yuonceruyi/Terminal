using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.XiaoShanZYY.Component.Auth.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.TakeNum.Models
{
    public interface ITakeNumService : IService
    {
        Result 汇总取号(Func<Result> func);
        Result 取号();
        Result 取号打印();
        Result 取消();
    }

    internal class TakeNumService : ITakeNumService
    {
        public string ServiceName { get; }

        IAuthModel GetAuthModel() => ServiceLocator.Current.GetInstance<IAuthModel>();
        ITakeNumModel GetTakeNumModel() => ServiceLocator.Current.GetInstance<ITakeNumModel>();
        IPaymentModel GetPaymentModel() => ServiceLocator.Current.GetInstance<IPaymentModel>();
        
        public Result 汇总取号(Func<Result> func)
        {
            var Auth = GetAuthModel();
            var Register = GetTakeNumModel();
            var PaymentModel = GetPaymentModel();

            var req = new ReqDll()
            {
                卡号 = Auth.人员信息.就诊卡号,
                结算类型 = "02",
                病人类别 = Auth.人员信息.病人类别,
                结算方式 = "1",
                应付金额 = "",

                就诊序号 = "",
                收费类别 = "",
                //科室代码 = Register.DeptId,
                //医生代码 = Register.DoctorId,
                //诊疗费 = Register.所选排班.ZHENLIAOF,
                //诊疗费_加收 = Register.所选排班.ZHENLIAOJSF,
                //挂号类别 = Register.所选排班.GUAHAOLB,
                //排班类别 = Register.所选排班.GUAHAOBC,
                取号密码 = Register.取号密码,
                //挂号日期 = Register.所选排班.PAIBANRQ,
            };
            var result = DataHandler.挂号取号(req);
            if (!result.IsSuccess)
                return result.Convert();

            var value = result.Value;
            Register.Res挂号预结算 = value;

            //var schedule = Register.所选排班;
            var amountReg = decimal.Parse(value.诊疗费) * 100;
            var amountRegExtra = decimal.Parse(value.诊疗费_加收) * 100;

            var self = decimal.Parse(value.移动支付) * 100;
            var insurance = decimal.Parse(value.医保支付) * 100;
            var card = decimal.Parse(value.市民卡账户支付) * 100;
            var off = decimal.Parse(value.惠民减免金额) * 100;

            PaymentModel.Self = self;
            PaymentModel.Insurance = insurance;
            PaymentModel.Total = amountReg + amountRegExtra;
            PaymentModel.NoPay = self == 0;
            PaymentModel.ConfirmAction = func;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：", value.挂号日期),
                new PayInfoItem("时间：",value.挂号序号),
                new PayInfoItem("科室：",value.科室名称),
                new PayInfoItem("医生：",value.医生名称),

                new PayInfoItem("总金额：",(amountReg+amountRegExtra).In元(), true),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("诊疗费：",amountReg.In元()),
                new PayInfoItem("诊疗费(加收)：",amountRegExtra.In元()),

                new PayInfoItem("医保支付：",insurance.In元()),
                new PayInfoItem("市民卡账户支付：",card.In元()),
                new PayInfoItem("惠民减免金额：",off.In元()),
                new PayInfoItem("应付金额：", self.In元(), true),
            };
            return Result.Success();
        }

        public Result 取号()
        {
            var Auth = GetAuthModel();
            var Register = GetTakeNumModel();
            var req = new ReqDll()
            {
                卡号 = Auth.人员信息.就诊卡号,
                结算类型 = "02",
                病人类别 = Auth.人员信息.病人类别,
                结算方式 = "2",
                应付金额 = Register.Res挂号预结算.移动支付,

                就诊序号 = "",
                收费类别 = "",
                //科室代码 = Register.DeptId,
                //医生代码 = Register.DoctorId,
                //诊疗费 = Register.所选排班.ZHENLIAOF,
                //诊疗费_加收 = Register.所选排班.ZHENLIAOJSF,
                //挂号类别 = Register.所选排班.GUAHAOLB,
                //排班类别 = Register.所选排班.GUAHAOBC,
                取号密码 = Register.取号密码,
                //挂号日期 = Register.所选排班.PAIBANRQ,

                设备编码 = FrameworkConst.OperatorId,
            };
            var paymentModel = ServiceLocator.Current.GetInstance<IPaymentModel>();
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
                            //req.结算流水号 = posinfo.Trace;
                            req.支付流水号 = posinfo.Ref;
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
            var result = DataHandler.挂号取号(req);
            if (!result.IsSuccess)
                return result.Convert();

            var value = result.Value;
            Register.Res挂号结算 = value;
            return Result.Success();
        }

        public Result 取号打印()
        {
            var Register = GetTakeNumModel();
            var res = Register.Res挂号结算;

            var printManager = ServiceLocator.Current.GetInstance<IPrintManager>();
            var queue = printManager.NewQueue("自助取号");
            var builder = new StringBuilder();
            builder.AppendLine("【挂号科室】" + res.科室名称);
            builder.AppendLine("【挂号医生】" + res.医生名称);
            builder.AppendLine("【挂号序号】" + res.挂号序号);
            builder.AppendLine("【科室位置】" + res.科室位置);
            builder.AppendLine("【候诊时间】" + res.候诊时间);
            //builder.AppendLine("【就诊时段】" + (Register.AmPm == "1" ? "上午" : "下午"));
            builder.AppendLine("【 诊疗费 】" + decimal.Parse(res.诊疗费).ToString("F2") + " 元");
            builder.AppendLine("【诊疗费(加收)】" + decimal.Parse(res.诊疗费_加收).ToString("F2") + " 元");
            builder.AppendLine("【医保报销】" + res.医保支付 + " 元");
            builder.AppendLine("【市民卡账户支付】" + res.市民卡账户支付 + " 元");
            builder.AppendLine("【移动支付】" + res.移动支付 + " 元");
            var paymentModel = ServiceLocator.Current.GetInstance<IPaymentModel>();
            if (!paymentModel.NoPay)
            {
                var epm = ServiceLocator.Current.GetInstance<IExtraPaymentModel>();
                var payMethod = epm.CurrentPayMethod;
                builder.AppendLine("【支付方式】" + payMethod);
            }
            builder.AppendLine("【 就诊号 】" + res.就诊号码);
            builder.AppendLine("此取号凭条就诊当日有效！");
            builder.AppendLine(".");
            builder.AppendLine(".");
            builder.AppendLine(".");
            queue.Enqueue(new PrintItemText() { Text = builder.ToString() });

            var printModel = ServiceLocator.Current.GetInstance<IPrintModel>();
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            printModel.SetPrintInfo(true, new PrintInfo()
            {
                TypeMsg = "取号成功",
                TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分取号",
                PrinterName = config.GetValue("Printer:Receipt"),
                Printables = queue,
                TipImage = "提示_凭条"
            });
            return Result.Success();
        }

        public Result 取消()
        {
            var Auth = GetAuthModel();
            var Register = GetTakeNumModel();
            var req = new Req预约退号处理()
            {
                JIUZHENKH = Auth.人员信息.就诊卡号,
                JIUZHENKLX = Auth.人员信息.病人类别,
                XINGMING = Auth.人员信息.病人姓名,
                QUHAOMM = Register.取号密码,
                YUYUELY = " ",
                ZHENGJIANHM = " ",
                ZHENGJIANLX = " "
            };
            var result = DataHandler.预约退号处理(req);
            if (!result.IsSuccess)
                return result.Convert();
            Register.Res预约退号处理 = result.Value;
            return Result.Success();
        }
    }
}