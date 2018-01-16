using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.Register.Models;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IAuthModel Auth { get; set; }

        [Dependency]
        public IRegisterModel Register { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            if (Register.IsDoctor)
            {
                var list = Register.DoctSchedulesSingleDept
                    .OrderBy(d => d.DEPTCODE)
                    .ThenBy(d => d.MEDAMPM)
                    .Select(p => new InfoMore
                    {
                        Title = p.DOCTNAME,
                        SubTitle = $"{DateTimeCore.Today:MM月dd日} {p.MEDAMPM.SafeToAmPm()}",
                        Type = "挂号费",
                        Amount = p.REGAMOUNT * 100m,
                        Extends = p.RESTNUM > 1000 ? string.Empty : $"剩余号源 {p.RESTNUM:F0}",
                        ConfirmCommand = confirmCommand,
                        Tag = p,
                        IsEnabled = p.RESTNUM > 0,
                        DisableText = "已挂满"
                    });
                Data = new ObservableCollection<InfoMore>(list);
            }
            else
            {
                var list = Register.DeptSchedules
                    .OrderBy(d => d.DEPTCODE)
                    .ThenBy(d => d.MEDAMPM)
                    .Select(p => new InfoMore
                    {
                        Title = p.DEPTNAME,
                        SubTitle = $"{DateTimeCore.Today:MM月dd日} {p.MEDAMPM.SafeToAmPm()}",
                        Type = "挂号费",
                        Amount = p.REGAMOUNT * 100m,
                        //Extends = $"剩余号源 {p.restnum}",
                        ConfirmCommand = confirmCommand,
                        Tag = p,
                        IsEnabled = true
                    });
                Data = new ObservableCollection<InfoMore>(list);
            }

            PlaySound(SoundMapping.选择挂号科室);
        }

        protected override void Confirm(Info i)
        {
            if (Register.IsDoctor)
            {
                if (i.Tag is PAIBAN_YISHENG)
                {
                    var doctSchedule = i.Tag.As<PAIBAN_YISHENG>();
                    Register.SelectedDoctSchedule = doctSchedule;
                    Register.IsDoctor = true;
                    ChangeNavigationContent(i.Title);

                    PaymentModel.Self = doctSchedule.REGAMOUNT * 100m;
                    PaymentModel.Insurance = 0m;
                    PaymentModel.Total = doctSchedule.REGAMOUNT * 100m;
                    PaymentModel.NoPay = true; //托收
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.LeftList = new List<PayInfoItem>
                    {
                        new PayInfoItem("日期：", $"{DateTimeCore.Today:MM月dd日}"),
                        new PayInfoItem("时间：", doctSchedule.MEDAMPM.SafeToAmPm()),
                        new PayInfoItem("科室：", doctSchedule.DEPTNAME),
                        new PayInfoItem("医生：", doctSchedule.DOCTNAME)
                    };

                    PaymentModel.RightList = new List<PayInfoItem>
                    {
                        new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                        new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                        new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                    };
                }
                else
                {
                    return;
                }
            }
            else
            {
                var deptSchedule = i.Tag.As<PAIBAN_KESHI>();
                Register.SelectedDeptSchedule = deptSchedule;
                Register.IsDoctor = false;
                ChangeNavigationContent(i.Title);

                PaymentModel.Self = deptSchedule.REGAMOUNT * 100m;
                PaymentModel.Insurance = 0m;
                PaymentModel.Total = deptSchedule.REGAMOUNT * 100m;
                PaymentModel.NoPay = true; //托收
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", $"{DateTimeCore.Today:MM月dd日}"),
                    new PayInfoItem("时间：", deptSchedule.MEDAMPM.SafeToAmPm()),
                    new PayInfoItem("科室：", deptSchedule.DEPTNAME),
                    new PayInfoItem("医生：", deptSchedule.DOCTNAME)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                };
            }

            Next();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行现场挂号，请稍候...");


                var req = new Req挂号()
                {
                    科室代码 = Register.IsDoctor ? Register.SelectedDoctSchedule.DEPTCODE : Register.SelectedDeptSchedule.DEPTCODE,
                    医生代码 = Register.IsDoctor ? Register.SelectedDoctSchedule.DOCTCODE : null,
                    值班类别 = Register.IsDoctor ? Register.SelectedDoctSchedule.MEDAMPM.ToString() : Register.SelectedDeptSchedule.MEDAMPM.ToString()
                };

                switch (CardModel.CardType)
                {
                    case CardType.身份证:
                        req.医保类别 = "1";
                        req.医保卡号 = Auth.Info.IDNO;
                        break;
                    case CardType.市医保卡:
                        req.医保类别 = "2";
                        req.医保卡号 = Auth.Info.MEDNO;
                        break;
                    case CardType.省医保卡:
                        req.医保类别 = "3";
                        req.医保卡号 = Auth.Info.MEDNO;
                        break;
                }

                var res = DataHandler.RunExe<Res挂号>(req);

                if (!res.Success)
                {
                    ShowAlert(false, "挂号失败", $"挂号失败:{res.Message}");
                    return Result.Fail($"挂号失败:{res.Message}");
                }

                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "挂号成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分挂号",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = RegisterPrintables(res),
                    TipImage = "提示_凭条"
                });

                Navigate(A.XC.Print);
                return Result.Success();
            }).Result;
        }

        private Queue<IPrintable> RegisterPrintables(Res挂号 res)
        {
            var queue = PrintManager.NewQueue("挂号单");
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{res.病人姓名}\n");
            sb.Append($"就诊卡号：{res.就诊卡号}\n");
            sb.Append($"就诊号码：{res.就诊号码}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"科室名称：{res.挂号科室}\n");
            sb.Append($"就诊医生：{res.挂号医生}\n");
            sb.Append($"就诊时间：{DateTimeCore.Today:yyyy-MM-dd}\n");
            sb.Append($"就诊场次：{res.挂号班别.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{res.就诊地址}\n");
            sb.Append($"就诊序号：{res.就诊序号}\n");
            sb.Append($"交易时间：{res.挂号日期}\n");
            sb.Append($"挂号费总额：{res.挂号费总额}\n");
            sb.Append($"账户支付：{res.病人账户支付}\n");
            sb.Append($"医保支付：{res.医保支付}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}