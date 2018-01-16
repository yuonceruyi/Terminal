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

namespace YuanTu.XiaoShanZYY.Component.Register.Models
{
    public interface IRegisterService : IService
    {
        Result 医院排班信息();
        Result 挂号医生信息();
        Result 挂号号源信息();
        Result 汇总挂号(Func<Result> func);
        Result 挂号();
        Result 挂号打印();
        Result 汇总预约(Func<Result> func);
        Result 预约();
        Result 预约打印();
    }

    internal class RegisterService : IRegisterService
    {
        public string ServiceName { get; }

        IAuthModel GetAuthModel() => ServiceLocator.Current.GetInstance<IAuthModel>();
        IRegisterModel GetRegisterModel() => ServiceLocator.Current.GetInstance<IRegisterModel>();
        IPaymentModel GetPaymentModel() => ServiceLocator.Current.GetInstance<IPaymentModel>();
        public Result 医院排班信息()
        {
            var Register = GetRegisterModel();
            var req = new Req医院排班信息
            {
                GUAHAOFS = Register.RegMode,
                PAIBANLX = Register.IsAppoint() ? "2" : "1",
                PAIBANRQ = Register.RegDate,
                GUAHAOBC = Register.IsAppoint() ? " " : Register.AmPm,
                GUAHAOLB = Register.RegType,
                KESHIDM = " ",
                YISHENGDM = "*",
                HUOQUROW = " "
            };

            var result = DataHandler.医院排班信息(req);
            if (!result.IsSuccess)
                return result.Convert();
            var list = result.Value.PAIBANLB;
            if (list == null || !list.Any())
                return Result.Fail("未找到对应的排班");

            Register.PAIBANLB = list;
            Register.PAIBANLBItems = list.GroupBy(ToKey)
                .Select(g =>
                {
                    var p = g.Key.IndexOf("||", StringComparison.Ordinal);
                    var id = g.Key.Substring(0, p);
                    var name = g.Key.Substring(p + 2);
                    return new PAIBANLBItem()
                    {
                        DeptId = id,
                        DeptName = name,
                        PAIBANLB = g.ToList()
                    };
                })
                .ToList();
            return Result.Success();
        }

        private string ToKey(PAIBANMX d)
        {
            string deptId = d.KESHIDM;
            string deptName = d.KESHIMC;
            var p = deptId.IndexOf("|", StringComparison.Ordinal);
            var id = p > 0 ? deptId.Substring(0, p) : deptId;
            var q = deptName.IndexOf("|", StringComparison.Ordinal);
            var name = q > 0 ? deptName.Substring(0, q) : deptName;
            return $"{id}||{name}";
        }

        public Result 挂号医生信息()
        {
            var Register = GetRegisterModel();
            var req = new Req挂号医生信息()
            {
                GUAHAOFS = Register.RegMode,
                GUAHAOLB = Register.RegType,
                GUAHAOBC = Register.所选排班.GUAHAOBC,
                KESHIDM = Register.DeptId,
                RIQI = Register.所选排班.PAIBANRQ,
            };
            var result = DataHandler.挂号医生信息(req);
            if (!result.IsSuccess)
                return result.Convert();
            var list = result.Value.YISHENGMX;
            if (list == null || !list.Any())
                return Result.Fail("未找到对应的排班");

            Register.YISHENGMX = list;
            return Result.Success();
        }

        public Result 挂号号源信息()
        {
            var Register = GetRegisterModel();
            var req = new Req挂号号源信息()
            {
                GUAHAOFS = Register.RegMode,
                GUAHAOBC = Register.所选排班.GUAHAOBC,
                KESHIDM = Register.DeptId,
                YISHENGDM = Register.DoctorId,
                RIQI = Register.所选排班.PAIBANRQ,
            };
            var result = DataHandler.挂号号源信息(req);
            if (!result.IsSuccess)
                return result.Convert();
            var list = result.Value.HAOYUANMX;
            if (list == null || !list.Any())
                return Result.Fail("未找到对应的排班");

            Register.HAOYUANMX = list;
            Register.HAOYUANMXItems = list
                .GroupBy(s => s.RIQI)
                .Select(g => new HAOYUANMXItem()
                {
                    RegDate = g.Key,
                    HAOYUANMX = g.ToList(),
                })
                .ToList();

            return Result.Success();
        }

        //public Result 汇总挂号(Func<Result> func)
        //{
        //    var Register = GetRegisterModel();
        //    var PaymentModel = GetPaymentModel();
        //    var schedule = Register.所选排班;
        //    var amountReg = decimal.Parse(schedule.ZHENLIAOF) * 100;
        //    var amountRegExtra = decimal.Parse(schedule.ZHENLIAOJSF) * 100;

        //    PaymentModel.Self = amountReg + amountRegExtra;
        //    PaymentModel.Insurance = 0;
        //    PaymentModel.Total = amountReg + amountRegExtra;
        //    PaymentModel.NoPay = true;
        //    PaymentModel.ConfirmAction = func;

        //    PaymentModel.LeftList = new List<PayInfoItem>()
        //    {
        //        new PayInfoItem("日期：",schedule.PAIBANRQ),
        //        new PayInfoItem("时间：",schedule.GUAHAOBC.SafeToAmPm()),
        //        new PayInfoItem("科室：",Register.DeptName),
        //        new PayInfoItem("医生：",schedule.YISHENGXM),
        //    };

        //    PaymentModel.RightList = new List<PayInfoItem>()
        //    {
        //        new PayInfoItem("诊疗费：",amountReg.In元()),
        //        new PayInfoItem("诊疗费(加收)：",amountRegExtra.In元()),
        //        new PayInfoItem("总金额：",(amountReg+amountRegExtra).In元(), true),
        //    };
        //    return Result.Success();
        //}

        public Result 汇总挂号(Func<Result> func)
        {
            var Auth = GetAuthModel();
            var Register = GetRegisterModel();
            var PaymentModel = GetPaymentModel();

            var req = new ReqDll()
            {
                卡号 = Auth.人员信息.就诊卡号,
                结算类型 = "01",
                病人类别 = Auth.人员信息.病人类别,
                结算方式 = "1",
                应付金额 = "",

                就诊序号 = "",
                收费类别 = "",
                科室代码 = Register.DeptId,
                医生代码 = Register.DoctorId,
                诊疗费 = Register.所选排班.ZHENLIAOF,
                诊疗费_加收 = Register.所选排班.ZHENLIAOJSF,
                挂号类别 = Register.所选排班.GUAHAOLB,
                排班类别 = Register.所选排班.GUAHAOBC,
                取号密码 = " ",
                挂号日期 = Register.所选排班.PAIBANRQ,
            };
            var result = DataHandler.挂号取号(req);
            if (!result.IsSuccess)
                return result.Convert();

            var value = result.Value;
            Register.Res挂号预结算 = value;
            
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
                new PayInfoItem("日期：",value.挂号日期),
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

        public Result 挂号()
        {
            var Auth = GetAuthModel();
            var Register = GetRegisterModel();
            var req = new ReqDll()
            {
                卡号 = Auth.人员信息.就诊卡号,
                结算类型 = "01",
                病人类别 = Auth.人员信息.病人类别,
                结算方式 = "2",
                应付金额 = Register.Res挂号预结算.移动支付,

                就诊序号 = "",
                收费类别 = "",
                科室代码 = Register.DeptId,
                医生代码 = Register.DoctorId,
                诊疗费 = Register.所选排班.ZHENLIAOF,
                诊疗费_加收 = Register.所选排班.ZHENLIAOJSF,
                挂号类别 = Register.所选排班.GUAHAOLB,
                排班类别 = Register.所选排班.GUAHAOBC,
                取号密码 = " ",
                挂号日期 = Register.所选排班.PAIBANRQ,

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

        public Result 挂号打印()
        {
            var Register = GetRegisterModel();
            var res = Register.Res挂号结算;

            var printManager = ServiceLocator.Current.GetInstance<IPrintManager>();
            var queue = printManager.NewQueue("自助挂号");
            var builder = new StringBuilder();
            builder.AppendLine("【挂号科室】" + res.科室名称);
            builder.AppendLine("【挂号医生】" + res.医生名称);
            builder.AppendLine("【挂号序号】" + res.挂号序号);
            builder.AppendLine("【科室位置】" + res.科室位置);
            builder.AppendLine("【候诊时间】" + res.候诊时间);
            builder.AppendLine("【就诊时段】" + (Register.AmPm == "1" ? "上午" : "下午"));
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
            builder.AppendLine("此挂号凭条就诊当日有效！");
            builder.AppendLine(".");
            builder.AppendLine(".");
            builder.AppendLine(".");
            queue.Enqueue(new PrintItemText() { Text = builder.ToString() });

            var printModel = ServiceLocator.Current.GetInstance<IPrintModel>();
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            printModel.SetPrintInfo(true, new PrintInfo()
            {
                TypeMsg = "挂号成功",
                TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分挂号",
                PrinterName = config.GetValue("Printer:Receipt"),
                Printables = queue,
                TipImage = "提示_凭条"
            });
            return Result.Success();
        }

        public Result 汇总预约(Func<Result> func)
        {
            var Register = GetRegisterModel();
            var PaymentModel = GetPaymentModel();
            var schedule = Register.所选排班;
            var doctor = Register.所选医生;
            var source = Register.所选号源;
            var amountReg = decimal.Parse(schedule.ZHENLIAOF) * 100;
            var amountRegExtra = decimal.Parse(schedule.ZHENLIAOJSF) * 100;

            PaymentModel.Self = amountReg + amountRegExtra;
            PaymentModel.Insurance = 0;
            PaymentModel.Total = amountReg + amountRegExtra;
            PaymentModel.NoPay = true;
            PaymentModel.ConfirmAction = func;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",source.RIQI),
                new PayInfoItem("班次：",source.GUAHAOBC.SafeToAmPm()),
                new PayInfoItem("科室：",Register.DeptName),
                new PayInfoItem("医生：",doctor?.YISHENGXM),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("序号：",source.GUAHAOXH),
                new PayInfoItem("时间：",source.JIUZHENSJ),
                new PayInfoItem("诊疗费：",schedule.ZHENLIAOF),
                new PayInfoItem("诊疗费(加收)：",schedule.ZHENLIAOJSF),
                new PayInfoItem("总金额：",(amountReg+amountRegExtra).In元(), true),
            };
            return Result.Success();
        }

        public Result 预约()
        {
            var Auth = GetAuthModel();
            var Register = GetRegisterModel();
            var doctor = Register.RegType == "4";
            var source = Register.所选号源;
            var req = new Req预约挂号处理()
            {
                PATIENTID = Auth.人员信息.就诊卡号,
                OPERATOR = FrameworkConst.OperatorId,
                ORDERDATE = source.RIQI,
                REGDEPTID = source.KESHIDM,
                DOCTORID = doctor ? source.YISHENGDM : " ",
                CLINICTTYPE = source.GUAHAOLB,
                DUTYTYPE = source.GUAHAOBC,
                SEQUENCENUM = doctor ? source.GUAHAOXH : source.DANGTIANPBID,
                TELNO = " ",
            };
            var result = DataHandler.预约挂号处理(req);
            if (!result.IsSuccess)
                return result.Convert();

            Register.Res预约结算 = result.Value;
            return Result.Success();
        }

        public Result 预约打印()
        {
            var Register = GetRegisterModel();
            var printManager = ServiceLocator.Current.GetInstance<IPrintManager>();
            var res = Register.Res预约结算;

            var queue = printManager.NewQueue("自助预约");
            var builder = new StringBuilder();
            builder.AppendLine("【预约序号】" + res.SEQUENCENUM);
            builder.AppendLine("【取号密码】" + res.ORDERNUM);
            builder.AppendLine("【预约日期】" + Register.RegDate);
            builder.AppendLine("【预约科室】" + Register.DeptName);
            builder.AppendLine("【预约医生】" + Register.DoctorName);
            builder.AppendLine("【取号日期】" + Register.所选号源.RIQI);
            builder.AppendLine("【候诊时间】" + Register.所选号源.JIUZHENSJ);
            builder.AppendLine("【打印日期】" + DateTimeCore.Now.ToShortDateString());
            queue.Enqueue(new PrintItemText { Text = builder.ToString() });

            builder.Clear();
            builder.AppendLine("预约须知：请在候诊时间前半个小时");
            builder.AppendLine("内取号,过期作废,预约后未取号就诊");
            builder.AppendLine("的，累计三次以上将会加入黑名单");
            builder.AppendLine(".");
            builder.AppendLine(".");
            builder.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = builder.ToString(), Font = new Font("宋体", 10, FontStyle.Regular) });

            var printModel = ServiceLocator.Current.GetInstance<IPrintModel>();
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            printModel.SetPrintInfo(true, new PrintInfo()
            {
                TypeMsg = "预约成功",
                TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分预约",
                PrinterName = config.GetValue("Printer:Receipt"),
                Printables = queue,
                TipImage = "提示_凭条"
            });
            return Result.Success();
        }
    }
}