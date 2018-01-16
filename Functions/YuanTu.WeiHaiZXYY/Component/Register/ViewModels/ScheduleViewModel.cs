using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.Models.Register;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Payment;

namespace YuanTu.WeiHaiZXYY.Component.Register.ViewModels
{
    public class ScheduleViewModel : YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {
        #region Overrides of ScheduleViewModel

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var infoList = new List<Info>();
            ScheduleModel.Res排班信息查询.data.ForEach((p) =>
            {
                var startTime = DateTime.Parse(p.medDate);
                infoList.Add(new Info
                {
                    Title = $"{startTime.ToString("HH:mm")}-{DateTime.Parse(p.extend.Split('|')[1]).ToString("HH:mm")}",
                    ConfirmCommand = confirmCommand,
                    Tag = p,
                });
            });
            Data = new ObservableCollection<Info>(infoList);
        }

        private ObservableCollection<Info> _data;

        public new ObservableCollection<Info> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;
            SourceModel.Req号源明细查询 = new req号源明细查询
            {
                operId = FrameworkConst.OperatorId,
                regMode = "1",
                regType = ScheduleModel.所选排班.regType,
                deptCode = DeptartmentModel.所选科室.deptCode,
                scheduleId = ScheduleModel.所选排班.scheduleId
            };
            SourceModel.Res号源明细查询 = DataHandlerEx.号源明细查询(SourceModel.Req号源明细查询);
            if (SourceModel.Res号源明细查询?.success ?? false)
            {
                var value = SourceModel.Res号源明细查询?.data;
                if (value.Count > 0 && !string.IsNullOrEmpty(value.FirstOrDefault().appoNo))
                {
                    var regAmount = decimal.Parse(value.FirstOrDefault().appoNo) * 100;
                    PaymentModel.Self = regAmount;
                    PaymentModel.Insurance = decimal.Parse("0");
                    PaymentModel.Total = regAmount;
                    PaymentModel.NoPay = false;//默认预约或者自费金额为0时不支付            
                    PaymentModel.ConfirmAction = Confirm;
                    PaymentModel.LeftList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("日期：",DateTimeCore.Now.ToString("yyyy-MM-dd")),
                        new PayInfoItem("时间：",ScheduleModel.所选排班.medDate),
                        new PayInfoItem("科室：", DeptartmentModel.所选科室.deptName),
                        new PayInfoItem("诊室：",DoctorModel?.所选医生?.deptName),

                    };

                    PaymentModel.RightList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("医生：",DoctorModel.所选医生.doctName),
                        new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                        new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                    };
                    Next();
                }
                else
                {
                    ShowAlert(false, "号源明细查询", "没有获得号源信息(列表为空)");
                }
            }
            else
            {
                ShowAlert(false, "号源明细查询", "没有获得号源信息", debugInfo: SourceModel.Res号源明细查询?.msg);
            }
        }
        protected override Result Confirm()
        {
            try
            {
                return DoCommand(lp =>
                {
                    lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    var scheduleInfo = ScheduleModel.所选排班;
                    var deptInfo = DepartmentModel.所选科室;
                    var sex = PatientModel.当前病人信息.sex == "男" ? "1" : "2";
                    RegisterModel.Req预约挂号 = new req预约挂号
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        operId = FrameworkConst.OperatorId,
                        tradeMode =
                            PaymentModel.NoPay
                                ? PayMethod.预缴金.GetEnumDescription()
                                : PaymentModel.PayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientId,
                        cash = PaymentModel.Total.ToString(),
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = scheduleInfo.regType,
                        medAmPm = scheduleInfo.medAmPm,
                        medDate = scheduleInfo.medDate,
                        deptCode = deptInfo.deptCode,
                        scheduleId = scheduleInfo.scheduleId,
                        appoNo = SourceModel.所选号源?.appoNo,
                        doctCode = DoctorModel.所选医生.doctCode,
                        //idNo = patientInfo.idNo,
                        //patientName = patientInfo.name,
                        extend = $"{PatientModel.当前病人信息.idNo}|{sex}|{ PatientModel.当前病人信息.birthday.Age()}|{ PatientModel.当前病人信息.phone}",
                    };

                    FillRechargeRequest(RegisterModel.Req预约挂号);
                    LocalRequest(RegisterModel.Req预约挂号);

                    RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                    if (RegisterModel.Res预约挂号?.success ?? false)
                    {
                        var yue = decimal.Parse(PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].accountNo);
                        yue -= PaymentModel.Total / 100;
                        PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].accountNo = yue.ToString();
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables =
                                ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : base.AppointPrintables(),
                            TipImage = "提示_凭条"
                        });
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);

                        return Result.Success();
                    }
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    var state = NavigationEngine.State;
                    if (state != A.Third.PosUnion && state != A.Third.SiPay)
                    {
                        //PrintModel.SetPrintInfo(false, ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败",errorMsg: RegisterModel.Res预约挂号?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败",
                            TipMsg = RegisterModel.Res预约挂号?.msg,
                            DebugInfo = RegisterModel.Res预约挂号?.msg
                        });
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    }

                    ExtraPaymentModel.Complete = true;
                    return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
                }).Result;
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"异常，原因:{ex}");
                ShowAlert(false, ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败", "发生系统异常");
                return Result.Fail("系统异常");
            }
        }

        protected override void FillRechargeRequest(req预约挂号 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;

                    req.accountNo = posinfo?.Ref;
                    req.transNo = posinfo?.Trace;

                    req.bankTime = DateTime.ParseExact(posinfo?.TransTime, "HHmmss", null).ToString("HHmmss");
                    req.bankDate = DateTime.ParseExact(posinfo?.TransDate, "MMdd", null).ToString("yyyyMMdd");
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.社保)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.posTransNo = posinfo.MId;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.Memo;//医保支付方式：银联交易流水号

                    req.accountNo = posinfo?.Trace;
                    req.transNo = posinfo?.Ref;

                    req.bankTime = DateTime.ParseExact(posinfo?.TransTime, "HHmmss", null).ToString("HHmmss");
                    req.bankDate = DateTime.ParseExact(posinfo?.TransDate, "yyyyMMdd", null).ToString("yyyyMMdd");
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.bankCardNo = thirdpayinfo.buyerAccount;
                }
            }
        }

        protected virtual void LocalRequest(req预约挂号 req)
        {

        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("");
            var register = RegisterModel.Res预约挂号.data;
            var department = DeptartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var time = DateTime.Parse(ScheduleModel.所选排班.medDate).ToString("yyyy-MM-dd HH:mm:ss");
            var sb = new StringBuilder();

            sb.Append($"------------自助挂号凭条-----------\n");
            sb.Append($"就诊卡号:{CardModel?.CardNo}\n");
            sb.Append($"门诊号码:\n");
            sb.Append($"{patientInfo.patientId}\n");
            sb.Append($"患者姓名：{patientInfo.name}\n");
            sb.Append($"账 单 号：{register?.appoNo}\n");
            sb.Append($"就诊科室：{department.deptName}\n");
            sb.Append($"科室地址：{department.parentDeptName}\n");
            sb.Append($"诊室名称：{DoctorModel.所选医生.deptName}\n");
            sb.Append($"就诊医师：{DoctorModel.所选医生.doctName}\n");
            sb.Append($"挂号费用：{PaymentModel.Total.In元()}\n");
            sb.Append($"就诊日期：{DateTimeCore.Now.ToString("yyyy-MM-dd")}\n");
            sb.Append($"就诊时间：{time.Substring(11)}(就诊时间为大约时间)\n");
            sb.Append($"-----------------------------------\n");
            sb.Append($"打印时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"设备编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"温馨提示：\n");
            sb.Append("1.预约时间为大约时间\n");
            sb.Append("2.请挂号后在挂号等候区看叫号显示屏等待叫号\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        #endregion
    }
}
