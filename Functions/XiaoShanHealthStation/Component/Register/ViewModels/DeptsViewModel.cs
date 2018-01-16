using System;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.ConfigService;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.XiaoShanArea.CYHIS.DLL;
using YuanTu.XiaoShanArea.CYHIS.WebService;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;
using YuanTu.XiaoShanHealthStation.Component.Register.Models;
using DataHandler = YuanTu.XiaoShanArea.CYHIS.WebService.DataHandler;
using PAIBANMX = YuanTu.XiaoShanArea.CYHIS.WebService.PAIBANMX;
using DLLDataHandler=YuanTu.XiaoShanArea.CYHIS.DLL.DataHandler;

namespace YuanTu.XiaoShanHealthStation.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }
        [Dependency]
        public IGuaHaoModel GuaHaoModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list2 = GuaHaoModel.排班明细.Select(p => new Info
            {
                Title = p.KESHIMC,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);
            
            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected override void Confirm(Info i)
        {
            GuaHaoModel.所选排班 = i.Tag.As<PAIBANMX>();
            ChangeNavigationContent($"{i.Title}");
            if (RegTypesModel.SelectRegType.RegType != RegType.普通门诊)
            {
                DoCommand(p =>
                {
                    //todo 查询医生
                    p.ChangeText("正在查询医生信息，请稍候...");
                    var req = new GUAHAOYSXX_IN
                    {
                        BASEINFO = Instance.Baseinfo,
                        GUAHAOBC = GuaHaoModel.所选排班.GUAHAOBC,
                        GUAHAOFS = ChoiceModel.Business == Business.挂号 ? "1" : "2",
                        GUAHAOLB = GuaHaoModel.所选排班.GUAHAOLB ?? " ",
                        KESHIDM = GuaHaoModel.所选排班.KESHIDM,
                        RIQI =
                            ChoiceModel.Business == Business.预约
                                ? DateTimeCore.Now.AddDays(2.0).ToString("yyyy-MM-dd")
                                : DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    };
                    GUAHAOYSXX_OUT res;
                    if (!DataHandler.GUAHAOYSXX(req, out res))
                    {
                        ShowAlert(false, "温馨提示", "查询医生信息失败");
                        return;
                    }
                    GuaHaoModel.医生列表 = res?.YISHENGMX;
                    if (res?.OUTMSG.ERRNO != "0" && res?.OUTMSG.ERRNO != "-1")
                    {
                        ShowAlert(false, "温馨提示", $"查询医生信息失败:{res?.OUTMSG.ERRMSG}");
                        return;
                    }
                    if (GuaHaoModel.医生列表 != null && GuaHaoModel.医生列表.Count != 0)
                    {
                        Next();
                    }
                });
            }
            else if (ChoiceModel.Business == Business.预约)
            {
                DoCommand(lp =>
                {

                    lp.ChangeText("正在查询号源信息，请稍候...");
                    var req = new GUAHAOHYXX_IN
                    {
                        BASEINFO = Instance.Baseinfo,
                        GUAHAOFS = ChoiceModel.Business == Business.挂号 ? "1" : "2",
                        RIQI = GuaHaoModel.RegDate.ToString("yyyy-MM-dd"),
                        GUAHAOBC = GuaHaoModel.AmPm == AmPm.Am ? "1" : "2",
                        KESHIDM = GuaHaoModel.所选排班?.KESHIDM,
                        YISHENGDM = GuaHaoModel.所选医生?.YISHENGDM
                    };
                    GUAHAOHYXX_OUT res;
                    if (!DataHandler.GUAHAOHYXX(req, out res))
                    {
                        ShowAlert(false, "温馨提示", "查询号源信息失败");
                        return;
                    }
                    if (res.OUTMSG.ERRNO != "0")
                    {
                        ShowAlert(false, "温馨提示", $"查询号源信息失败:{res.OUTMSG?.ERRMSG}");
                        return;
                    }
                    GuaHaoModel.号源信息 = res.HAOYUANMX;
                    Navigate(A.YY.Time);
                });
            }
            else 
            {
                var schedulInfo = GuaHaoModel.所选排班;
                PaymentModel.Self = decimal.Parse(schedulInfo.ZHENLIAOF);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.ZHENLIAOJSF);
                PaymentModel.NoPay = true; //默认预约或者自费金额为0时不支付
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",schedulInfo.PAIBANRQ),
                new PayInfoItem("时间：",schedulInfo.GUAHAOBC=="1"?"上午":"下午"),
                new PayInfoItem("科室：",schedulInfo.KESHIMC),
                new PayInfoItem("医生：",schedulInfo.YISHENGXM),
            };

                PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("诊疗费：",$"{PaymentModel.Self}元"),
                new PayInfoItem("诊疗费加收：",$"{PaymentModel.Total}元"),
                new PayInfoItem("挂号金额：",$"{PaymentModel.Total+PaymentModel.Self}元",true),
            };
                Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Confirm : A.YY.Confirm);
            }
        }

        protected virtual Result Confirm()
        {
            return DoCommand(p =>
            {
                if (ChoiceModel.Business == Business.挂号)
                {
                    挂号取号_OUT res;
                    var req = new Req挂号取号
                    {
                        卡号 = ChaKaModel.查询建档Out.就诊卡号,
                        结算类型 = "01",
                        调用接口ID = "",
                        调用类型 = "",
                        病人类别 = ChaKaModel.PatientType,
                        结算方式 = "", //1 预结算 2 结算  ?
                        应付金额 = "",
                        就诊序号 = "",
                        操作工号 = FrameworkConst.OperatorId,
                        系统序号 = "",
                        收费类别 = "",
                        科室代码 = GuaHaoModel.所选排班.KESHIDM,
                        医生代码 = GuaHaoModel.所选医生?.YISHENGDM,
                        诊疗费加收 = GuaHaoModel.所选排班.ZHENLIAOJSF,
                        诊疗费 = GuaHaoModel.所选排班.ZHENLIAOF,
                        挂号类别 = GuaHaoModel.所选排班.GUAHAOLB,
                        排班类别 = GuaHaoModel.AmPm == AmPm.Am ? "1" : "2",
                        取号密码 = " ",
                        挂号日期 = GuaHaoModel.RegDate.ToString("yyyy-MM-dd")
                    };
                    if (!DLLDataHandler.挂号取号(req, out res))
                    {
                        return Result.Fail("挂号失败");
                    }
                    GuaHaoModel.挂号结果 = res;
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : AppointPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    return Result.Success();
                }
                else
                {
                    return Result.Success();
                }
                
            }).Result;
        }
        protected virtual Queue<IPrintable> RegisterPrintables()
        {

            var queue = PrintManager.NewQueue("挂号单");
            var register = GuaHaoModel.挂号结果;
            var schedule = GuaHaoModel.所选排班;
            var doctor = GuaHaoModel.所选医生;
            var patientInfo = ChaKaModel.查询建档Out;
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.病人姓名}\n");
            sb.Append($"门诊号：{patientInfo.就诊卡号}\n");
            sb.Append($"交易类型：现场挂号\n");
         
            sb.Append($"挂号科室：{register?.科室名称}\n");
            sb.Append($"挂号医生：{doctor?.YISHENGXM}\n");
            sb.Append($"挂号序号：{register?.挂号序号}\n");
            sb.Append($"科室位置：{register?.科室位置}\n");
            sb.Append($"候诊时间：{register?.候诊时间}\n");
            sb.Append($"就诊时段：{(schedule?.GUAHAOBC=="1"?"上午":"下午")}\n");
            sb.Append($"挂号金额：{(Convert.ToDouble(register?.诊疗费) + Convert.ToDouble(register?.诊疗费加收)).ToString("F2")}\n");
            sb.Append($"医保报销：{register?.医保支付}\n");
            sb.Append($"自费金额：{register?.市民卡账户支付}\n");
            sb.Append($"惠民减免：{register?.惠民减免金额}\n");
            sb.Append($"就诊号：{register?.就诊号码}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected virtual Queue<IPrintable> AppointPrintables()
        {

            var queue = PrintManager.NewQueue("预约挂号单");
            //var register = RegisterModel.Res预约挂号.data;
            //var department = DepartmentModel.所选科室;
            //var schedule = ScheduleModel.所选排班;
            //var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            //sb.Append($"状态：预约成功\n");
            //sb.Append($"姓名：{patientInfo.name}\n");
            //sb.Append($"门诊号：{patientInfo.patientId}\n");
            //sb.Append($"交易类型：预约挂号\n");
            //sb.Append($"科室名称：{department.parentDeptName}\n");
            //sb.Append($"诊疗科室：{department.deptName}\n");
            //sb.Append($"就诊医生：{schedule.doctName}\n");
            ////sb.Append($"挂号费：{paiban.regfee.In元()}\n");
            ////sb.Append($"诊疗费：{paiban.treatfee.In元()}\n");
            ////sb.Append($"挂号金额：{paiban.regAmount.In元()}\n");
            //sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            //sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            //sb.Append($"就诊地址：{register.address}\n");
            //sb.Append($"挂号序号：{register.appoNo}\n");
            //sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            //sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            //sb.Append($"请于当日取号就诊。\n");
            //sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}