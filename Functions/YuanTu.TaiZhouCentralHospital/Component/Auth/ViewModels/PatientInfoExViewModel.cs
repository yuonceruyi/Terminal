using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Practices.Unity;
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
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.TaiZhouCentralHospital.HealthInsurance;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;

namespace YuanTu.TaiZhouCentralHospital.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
            : base(rfCardDispenser)
        {
            _magCardDispenser = magCardDispenser.FirstOrDefault(p => p.DeviceId == "ZBR_Mag");
        }

        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return View.Dispatcher.Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });

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
                if (InnerConfig.发卡类型 == 发卡类型.就诊卡) //就诊卡
                {
                    var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", $"发卡机读卡号失败：{result.Message}");
                        return false;
                    }
                    CardModel.CardNo = result.Value[TrackRoad.Trace2];
                    return true;
                }
                if (InnerConfig.发卡类型 == 发卡类型.健康卡) //健康卡
                {
                    //todo 尝试健康卡读卡

                    var result = HealthCard.ReadCitizenCard();
                    if (!result.IsSuccess)
                    {
                        //todo 移卡到桥接读卡位置 CardPosF6.移到读卡器内部 入参无意义
                        if (!_magCardDispenser.MoveCard(CardPosF6.移到读卡器内部).IsSuccess)
                        {
                            ShowAlert(false, "建档发卡", "发卡机移卡失败");
                            return false;
                        }
                        Thread.Sleep(300);
                        result = HealthCard.ReadCitizenCard();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "建档发卡", result.Message);
                            return false;
                        }
                    }

                    //todo 健康卡写卡
                    var indata = new InData
                    {
                        健康卡卡号 = result.Value.市民卡健康卡卡号,
                        姓名 = Name.Trim(),
                        出生日期 = DateTime.ToString("yyyyMMdd"),
                        性别 = IsBoy ? "1" : "2",
                        证件类型 = "00",
                        联系电话 = CreateModel.Phone,
                        证件号码 = IdCardModel.IdCardNo,
                        地址 = IdCardModel.Address
                    };
                    var resultWrite = HealthCard.WriteCitizenCard(indata);
                    if (!resultWrite.IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", result.Message);
                        return false;
                    }
                    SiModel.健康卡信息 = result.Value;
                    CardModel.CardNo = result.Value.市民卡健康卡卡号;
                    return true;
                }
                ShowAlert(false, "建档发卡", $"建档的卡类型配置错误:{InnerConfig.发卡类型}");
                return false;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Device.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
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
            if (DateTime.CompareTo(DateTime.Now) > 0)
            {
                ShowAlert(false, "温馨提示", "出生日期不能大于当前日期");
                return;
            }

            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
            
                return GetNewCardNo();
            }).ContinueWith(ctx =>
            {
                if (!ctx.Result) return;
                PaymentModel.Self = 100;
                PaymentModel.Insurance = 0;
                PaymentModel.Total = 100;
                PaymentModel.NoPay = false;
                PaymentModel.ConfirmAction = CreatePatientCallBack;
                PaymentModel.MidList = new List<PayInfoItem>
                {
                    new PayInfoItem("新卡号：", CardModel.CardNo),
                    new PayInfoItem("办卡费用：", PaymentModel.Self.In元(), true)
                };
                Next();
            });
        }

        protected virtual Result CreatePatientCallBack()
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
                    idType = "1",
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = CreateModel.Phone,
#pragma warning disable 612
                    guardianName = IdCardModel.Name,
                    guardianNo = IdCardModel.IdCardNo,
                    school = null,
#pragma warning restore 612
                    pwd = "123456",
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString(),
                    accountNo = null,
                    patientType = null,
                    setupType = ((int) CreateModel.CreateType).ToString()
                };
                FillRequest(CreateModel.Req病人建档发卡);
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);

                ExtraPaymentModel.Complete = true;

                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    if (!FrameworkConst.DoubleClick)
                        PrintCard();

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });

                    ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                    Navigate(A.JD.Print);
                    return Result.Success();
                }

                //todo 现金支付时打印单边帐凭条
                if (PaymentModel.PayMethod == PayMethod.现金)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "建档发卡失败",
                        TipMsg = $"原因:{CreateModel.Res病人建档发卡?.msg}",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreateFailPrintables(),
                        TipImage = null
                    });
                    Navigate(A.JD.Print);
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }

               
                return Result.Fail(CreateModel.Res病人建档发卡?.code ?? -100, CreateModel.Res病人建档发卡?.msg);
            }).Result;
        }

        protected virtual Queue<IPrintable> CreateFailPrintables()
        {
            var queue = PrintManager.NewQueue($"现金{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：建档发卡失败\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.现金)
            {
                req.cash = ExtraPaymentModel.TotalMoney.ToString();
                req.transNo = req.flowId;
            }
        }

        protected override void PrintCard()
        {
            if (InnerConfig.发卡类型==发卡类型.健康卡) //健康卡
                _magCardDispenser.PrintContent(new List<ZbrPrintTextItem>
                    {
                        new ZbrPrintTextItem
                        {
                            X = 160,
                            Y = 55,
                            Text = Name
                        },
                        new ZbrPrintTextItem
                        {
                            X = 550,
                            Y = 55,
                            FontSize = 11,
                            Text = CardModel.CardNo
                        }
                    },
                    new List<ZbrPrintCodeItem> {new ZbrPrintCodeItem()});
            else
                _magCardDispenser.PrintContent(new List<ZbrPrintTextItem>
                    {
                        new ZbrPrintTextItem
                        {
                            X = 160,
                            Y = 55,
                            FontSize = 15,
                            Text = Name
                        },
                        new ZbrPrintTextItem
                        {
                            X = 400,
                            Y = 55,
                            FontSize = 15,
                            Text = CardModel.CardNo.SafeSubstring(CardModel.CardNo.Length-7,7)
                        }
                    },
                    new List<ZbrPrintCodeItem> { new ZbrPrintCodeItem() });
        }
    }
}