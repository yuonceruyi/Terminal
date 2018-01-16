using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.WeiHaiZXYY.Component.Register.Services;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Core.Navigating;
using YuanTu.Consts.Dtos.UnionPos;
using System.IO;

namespace YuanTu.WeiHaiZXYY.Component.Register.ViewModels
{
    public class DoctorViewModel : YuanTu.Default.Component.Register.ViewModels.DoctorViewModel
    {
        #region Dependency
        [Dependency]
        public ISourceModel SourceModel { get; set; }
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IRegisterModel RegisterModel { get; set; }
        #endregion

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var res = ResourceEngine;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", FrameworkConst.HospitalId, "Images", "Doctor");
            var list = DoctorModel.Res医生信息查询.data.Select(p => new InfoDoc
            {
                Title = p.doctName,
                Tag = p,
                ConfirmCommand = confirmCommand,
                IconUri = !File.Exists(Path.Combine(path, $"{p.doctCode}.jpg")) ? (
                  p.sex == "女"
                  ? res.GetImageResourceUri("图标_通用医生_女")
                  : res.GetImageResourceUri("图标_通用医生_男")) :
                  new Uri(Path.Combine(path, $"{p.doctCode}.jpg")),
                Description = p.deptName,
                Rank = p.doctProfe,
                // 无法获取
                Amount = decimal.Parse(p.extend.Split('|')[0]),
                Remain = int.Parse(p.extend.Split('|')[2])
            });
            Data = new ObservableCollection<Info>(list);


            PlaySound(SoundMapping.选择挂号医生);
        }

        protected override void Confirm(Info i)
        {
            DoctorModel.所选医生 = i.Tag.As<医生信息>();
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
                ScheduleModel.排班信息查询 = new req排班信息查询
                {
                    regMode = "2",
                    deptCode = DeptartmentModel.所选科室.deptCode,
                    doctCode = i.Tag.As<医生信息>().doctCode,
                    parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                    startDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                    endDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                    extend = DoctorModel.所选医生.extend.Split('|')[1]
                };
                ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                if (ScheduleModel.Res排班信息查询?.success ?? false)
                {
                    if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                    {
                        for (int c = ScheduleModel.Res排班信息查询.data.Count - 1; c >= 0; c--)
                        {
                            var startTime = DateTime.Parse(ScheduleModel.Res排班信息查询.data[c].medDate);
                            var endTime = startTime.AddMinutes(-5);
                            var patientId = ScheduleModel.Res排班信息查询.data[c].extend.Split('|')[0];
                            if (DateTime.Compare(DateTimeCore.Now, endTime) > 0 || !string.IsNullOrEmpty(patientId))
                            {
                                ScheduleModel.Res排班信息查询.data.RemoveAt(c);
                            }
                        }
                        ChangeNavigationContent(i.Title);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                }
            });
        }

        protected void QuerySource(Info i)
        {
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
                    PaymentModel.ConfirmAction = 挂号;
                    PaymentModel.LeftList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("日期：",DateTimeCore.Now.ToString("yyyy-MM-dd")),
                        new PayInfoItem("时间：",DateTimeCore.Now.ToString("HH-mm-ss")),
                        new PayInfoItem("科室：", DeptartmentModel.所选科室.deptName),

                    };

                    PaymentModel.RightList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("医生：",i.Tag.As<医生信息>().doctName),
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

        private Result 挂号()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var scheduleInfo = ScheduleModel.所选排班;
                var deptInfo = DeptartmentModel.所选科室;
                RegisterModel.Req预约挂号 = new req预约挂号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    idNo = patientInfo.idNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ScheduleModel.所选排班.regType,
                    medAmPm = scheduleInfo.medAmPm,
                    medDate = scheduleInfo.medDate,
                    deptCode = deptInfo.deptCode,
                    scheduleId = scheduleInfo.scheduleId,
                    appoNo = SourceModel.所选号源?.appoNo,
                    patientName = patientInfo.name,
                    doctCode = DoctorModel.所选医生.doctCode
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = RegisterPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    return Result.Success();
                }
                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    //PrintModel.SetPrintInfo(false, ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败", errorMsg: RegisterModel.Res预约挂号?.msg);
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

        protected virtual void FillRechargeRequest(req预约挂号 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
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

        protected virtual Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DeptartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            //sb.Append($"排班类型：{paiban.doctTech}\n");
            sb.Append($"科室名称：{DoctorModel.所选医生}\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{DoctorModel.所选医生.doctName}\n");
            sb.Append($"挂号费：{schedule.regfee.In元()}\n");
            sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            //sb.Append($"个人支付：{guahao.selfFee.In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(guahao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            sb.Append($"温馨提示：\n");
            sb.Append("1.时间为大约时间\n");
            sb.Append("2.请挂号后在挂号等候区看叫号显示屏等待叫号\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
