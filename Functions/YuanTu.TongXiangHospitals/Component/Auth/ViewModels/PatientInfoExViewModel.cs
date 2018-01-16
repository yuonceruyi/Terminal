using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;

namespace YuanTu.TongXiangHospitals.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        private const long PatientCreatedCode = -13;
        private bool MoveCardOutFail { get; set; }
        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        public PatientInfoExViewModel(IMagCardDispenser[] magCardDispenser) : base(null)
        {
            _magCardDispenser = magCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
            ConfirmCommand = new DelegateCommand(Confirm);
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
        }

        public override void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请输入就诊人姓名");
                return;
            }
            if (!IsBoy && !IsGirl)
            {
                ShowAlert(false, "温馨提示", "请选择就诊人性别");
                return;
            }
            if (CardModel.CardType == CardType.社保卡)
            {
                CreatePatient();
            }
            else
            {
                if (!DoCommand(lp =>
                {
                    lp.ChangeText("正在准备就诊卡，请稍候...");
                    //todo 发卡机发卡
                    if (!GetNewCardNo())
                        return Result.Fail("获取新卡失败");
                    return Result.Success();
                }).Result.IsSuccess)
                    return;
                PaymentModel.Self = 100;
                PaymentModel.Insurance = 0;
                PaymentModel.Total = 100;
                PaymentModel.NoPay = false;
                //PaymentModel.PayMethod = PayMethod.银联;
                PaymentModel.ConfirmAction = SetPatientInfo;
                PaymentModel.MidList = new List<PayInfoItem>()
                {
                  new PayInfoItem("办卡费用：",PaymentModel.Self.In元(),true),
                };
                Next();
            }
        }

        protected virtual Result SetPatientInfo()
        {
            return DoCommand(lp =>
            {
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = Name.Trim(),
                    sex = IsBoy ? Sex.男.ToString() : Sex.女.ToString(),
                    birthday = DateTime.ToString("yyyy-MM-dd"),
                    idNo = "",
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = CreateModel.Phone,
#pragma warning disable 612
                    guardianName = IdCardModel.Name,
                    guardianNo = IdCardModel.IdCardNo,
                    school = null,
#pragma warning restore 612
                    pwd = CardModel.CardType == CardType.社保卡 ? SiModel.CardHardInfo : "123456",
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString(),
                    accountNo = null,
                    patientType = CardModel.CardType == CardType.社保卡 ? "医保" : "自费",
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = CardModel.CardType == CardType.社保卡 ? SiModel.RetMessage : null,
                };
                FillRequest(CreateModel.Req病人建档发卡);

                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    if (CardModel.CardType != CardType.社保卡)
                    {
                        lp.ChangeText("正在发卡，请及时取卡。");
                        if (!FrameworkConst.DoubleClick)
                        {
                            var result = _magCardDispenser.MoveCardOut();
                            MoveCardOutFail = !result.IsSuccess;
                            if (!result.IsSuccess)
                                _magCardDispenser.MoveCard(CardPosF6.吞入, "弹卡到前端失败，故回收卡片");
                        }
                    }

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{CardModel.CardNo}");
                    Navigate(A.JD.Print);
                    return Result.Success();
                }
                if (CardModel.CardType != CardType.社保卡 && CreateModel.Res病人建档发卡?.code == PatientCreatedCode)
                    _magCardDispenser.MoveCard(CardPosF6.吞入, $"此卡[{CardModel.CardNo}]已建档，故回收卡片");
                ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                return Result.Fail(CreateModel.Res病人建档发卡?.msg);
            }).Result;
        }

        protected virtual void FillRequest(req病人建档发卡 req)
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
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                if (CardModel.CardType != CardType.社保卡)
                {
                    lp.ChangeText("正在准备就诊卡，请稍候...");
                    //todo 发卡机发卡
                    if (!GetNewCardNo()) return;
                }

                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = Name.Trim(),
                    sex = IsBoy ? Sex.男.ToString() : Sex.女.ToString(),
                    birthday = DateTime.ToString("yyyy-MM-dd"),
                    idNo = "",
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = CreateModel.Phone,
#pragma warning disable 612
                    guardianName = IdCardModel.Name,
                    guardianNo = IdCardModel.IdCardNo,
                    school = null,
#pragma warning restore 612
                    pwd = CardModel.CardType == CardType.社保卡 ? SiModel.CardHardInfo : "123456",
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = CardModel.CardType == CardType.社保卡 ? "医保" : "自费",
                 
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = CardModel.CardType == CardType.社保卡 ? SiModel.RetMessage : null,
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    if (CardModel.CardType != CardType.社保卡)
                    {
                        lp.ChangeText("正在发卡，请及时取卡。");
                        if (!FrameworkConst.DoubleClick)
                        {
                            var result = _magCardDispenser.MoveCardOut();
                            MoveCardOutFail = !result.IsSuccess;
                            if (!result.IsSuccess)
                                _magCardDispenser.MoveCard(CardPosF6.吞入, "弹卡到前端失败，故回收卡片");
                        }
                    }

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });

                    ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                    Navigate(A.JD.Print);
                }
                else
                {
                    if (CardModel.CardType != CardType.社保卡 && CreateModel.Res病人建档发卡?.code == PatientCreatedCode)
                        _magCardDispenser.MoveCard(CardPosF6.吞入, $"此卡[{CardModel.CardNo}]已建档，故回收卡片");
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick)
                {
                    CardModel.CardNo = DateTimeCore.Now.ToString("HHmmssff");
                    return true;
                }

                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                    return GetNewCardNo();
                }
                CardModel.CardNo = result.Value[TrackRoad.Trace2];
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }
        protected override Queue<IPrintable> CreatePrintables()
        {
            if (CardModel.CardType != CardType.社保卡)
            {
                var queue = PrintManager.NewQueue("自助发卡");

                var sb = new StringBuilder();
                sb.Append($"状态：办卡成功\n");
                sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
                sb.Append($"姓名：{IdCardModel.Name}\n");
                sb.Append($"就诊卡号：{CardModel.CardNo}\n");
                sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                if (MoveCardOutFail)
                    sb.Append($"由于硬件故障，出卡失败，请联系工作人员取卡\n");
                sb.Append($"请妥善保管好您的个人信息。\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                return queue;
            }
            else
            {
                var queue = PrintManager.NewQueue("医保卡激活");

                var sb = new StringBuilder();
                sb.Append($"状态：医保卡激活成功\n");
                sb.Append($"激活单位：{FrameworkConst.HospitalName}\n");
                sb.Append($"姓名：{IdCardModel.Name}\n");
                sb.Append($"医保卡号：{CardModel.CardNo}\n");
                sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"请妥善保管好您的个人信息。\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                return queue;
            }
        }
    }
}