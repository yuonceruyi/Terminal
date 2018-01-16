using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.ChongQingArea.Services;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class IDCardViewModel : Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders, IRFCardDispenser[] rfCardDispenser) : base(idCardReaders)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "ZBR_RF");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            TimeOut = 20;

            PlaySound(SoundMapping.请插入身份证);
            StartRead();

            CardModel.CardType = CardType.身份证;
            Hint = CreateModel.CreateType == CreateType.儿童 ? "请刷监护人身份证" : "请刷身份证";
        }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.建档)
                {

                    if (CreateModel.CreateType == CreateType.成人)
                    {
                        PatientModel.Req病人信息查询 = new req病人信息查询
                        {
                            Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                            cardNo = idCardNo,
                            cardType = ((int)CardModel.CardType).ToString(),
                            patientName = IdCardModel.Name
                        };
                        PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                        if (PatientModel.Res病人信息查询.success)
                        {
                            if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                            {
                                Navigate(A.CK.Info);
                            }
                            else
                            {
                                ShowAlert(false, "建档发卡", "您已经在平台建过档，请勿重复建档！",extend:new AlertExModel(){HideCallback = ap => Navigate(A.Home) });
                            }
                        }
                        else
                        {
                            if (PatientModel.Res病人信息查询.code == 0)
                            {
                                Navigate(A.CK.Info);
                            }
                            else
                            {
                                ShowAlert(false, "建档发卡", "系统异常，无法建档\r\n" + PatientModel.Res病人信息查询.msg, 5);
                                Navigate(A.Home);
                            }
                        }
                    }
                    else
                    {
                        Navigate(A.CK.Info);
                    }
                    }
                else if (ChoiceModel.Business == Business.补卡)
                {
                    if (CreateModel.CreateType == CreateType.儿童)
                    {
                        //儿童建卡输入姓名
                        ReSendModel.idNo = "";
                        ReSendModel.guarderId = IdCardModel.IdCardNo;
                        StackNavigate(A.ChaKa_Context, B.CK.InputText);
                        ReSendModel.Check = ResendCard;
                    }
                    else
                    {
                        ReSendModel.idNo = IdCardModel.IdCardNo;
                        ReSendModel.name = IdCardModel.Name;
                        ReSendModel.guarderId = "";
                        var r = ResendCard();
                        if (r.IsSuccess)
                        { StackNavigate(B.BuKa_Context,B.BK.Confirm); }
                        else
                        { Navigate(A.Home); }
                    }
                }
                else
                {
                    PatientModel.Req病人信息查询 = new req病人信息查询
                    {
                        Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                        cardNo = idCardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
                        patientName = IdCardModel.Name
                    };
                    PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                    if (PatientModel.Res病人信息查询.success)
                    {
                        if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                        {
                            ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                            StartRead();
                            return;
                        }
                        //CardModel.CardNo = PatientModel.Res病人信息查询?.data[0]?.idNo;
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                        StartRead();
                    }
                }
            });
        }


        #region 补卡

        [Dependency]
        public IReSendModel ReSendModel { get; set; }
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
        protected IRFCardDispenser _rfCardDispenser;

        //检查是否可补卡
        protected Result ResendCard()
        {
            Logger.Main.Info("开始ResendCard");
            ReSendModel.Req补卡查询 = new req补卡查询();
            ReSendModel.Req补卡查询.idNo = ReSendModel.idNo;
            ReSendModel.Req补卡查询.name = ReSendModel.name;
            ReSendModel.Req补卡查询.guarderId = ReSendModel.guarderId;
            //挂失状态查询
            ReSendModel.Res补卡查询 = DataHandlerEx.补卡查询(ReSendModel.Req补卡查询);
            string ErrMsg = "";
            if (ReSendModel.Res补卡查询.success && ReSendModel.Res补卡查询.data.Count>0)
            {
                switch (ReSendModel.Res补卡查询.data[0].cardStatus)
                {
                    case "-99"://已注销
                        ErrMsg = "卡已注销，请重新办卡";
                        break;
                    case "-1"://已挂失
                        break;
                    case "0"://未领卡
                        //ErrMsg = "卡状态正常，不需要补卡";
                        break;
                    case "1"://已领卡
                        ErrMsg = "卡状态正常，不需要补卡";
                        break;
                }
            }
            else
            {
                ErrMsg = "未查询到病人的挂失信息";
            }
            if (ErrMsg == "")
            {
                PaymentModel.MidList = new List<PayInfoItem>()
                {
                    new PayInfoItem("患者姓名：",ReSendModel.Req补卡查询.name),
                    new PayInfoItem("身份证号：",ReSendModel.Req补卡查询.idNo.Mask(14, 3)),
                    new PayInfoItem("补卡卡费：",Startup.ReSendCost.In元()),
                    new PayInfoItem("温馨提示：","试运行阶段 补卡费用为"+Startup.ReSendCost.In元()),
                };
                if (CreateModel.CreateType == CreateType.儿童)
                {
                    //PaymentModel.MidList[1].Title = "监护人身份证号：";
                    PaymentModel.MidList[1].Content = ReSendModel.Req补卡查询.guarderId.Mask(14, 3);
                }
                PaymentModel.Self = Startup.ReSendCost;
                PaymentModel.Total = Startup.ReSendCost;
                PaymentModel.ConfirmAction = Confirm;
                PaymentModel.NoPay = PaymentModel.Self == 0;
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = ReSendModel.Res补卡查询.data[0].cardNo,
                    cardType = ((int)CardType.就诊卡).ToString(),
                    patientName = ReSendModel.name
                };
                var res = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                return Result.Success();
            }
            else
            {
                ShowAlert(false, "病人挂失信息查询", ErrMsg);
                return Result.Fail(ErrMsg);
            }
        }
        protected Result Confirm()
        {
            return DoCommand(Confirm).Result;
        }
        private Result Confirm(LoadingProcesser lp)
        {
            lp.ChangeText("正在进行补卡，请稍候...");
            
            if (GetNewCardNo())
            {
                if (!WriteCardNo(CardModel.CardNo))
                {
                   ShowAlert(false,"补卡失败","将身份信息写入卡片内失败！");
                    return Result.Fail("写卡号失败") ;
                }
            }
            else
            {
                ShowAlert(false, "补卡失败", "获取新卡卡号失败！");
                return Result.Fail("读卡号失败");
            }
            ReSendModel.Req补卡 = new req补卡
            {
                cardNo = ReSendModel.Res补卡查询.data[0].patientCard,
                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.PayMethod.GetEnumDescription().BackNotNullOrEmpty(PayMethod.预缴金.GetEnumDescription()),
                cash = PaymentModel.Self.ToString("0"),

                newSeqNo = CardModel.CardNo,
                platformId = ReSendModel.Res补卡查询.data[0].platformId,
            };
            FillResendCardRequest(ReSendModel.Req补卡);
            ReSendModel.Res补卡 = DataHandlerEx.补卡(ReSendModel.Req补卡);
            if (ReSendModel.Res补卡.success)
            {
                //支付完成
                ExtraPaymentModel.Complete = true;
                lp.ChangeText("正在发卡，请及时取卡。");
                //if (!FrameworkConst.DoubleClick)
                    PrintCard();
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "补卡成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分补卡",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = ReSendCardPrintables(),
                    //Printables = BillPayPrintables(),
                    TipImage = "提示_凭条"
                });
                Navigate(B.BK.Print);
                return Result.Success();
            }
            
            //第三方支付失败时去支付流程里面处理，不在业务里面处理
            if (NavigationEngine.State != A.Third.PosUnion)
            {
                //PrintModel.SetPrintInfo(false, "缴费失败", errorMsg: BillPayModel.Res缴费结算?.msg);
                PrintModel.SetPrintInfo(false, new PrintInfo
                {
                    TypeMsg = "补卡失败",
                    DebugInfo = ReSendModel.Res补卡.msg
                });
                Navigate(B.BK.Print);
            }

            ExtraPaymentModel.Complete = true;
            return Result.Fail(ReSendModel.Res补卡.code, ReSendModel.Res补卡.msg);
        }
        protected  Queue<IPrintable> ReSendCardPrintables()
        {

            var queue = PrintManager.NewQueue("自助补卡");

            var sb = new StringBuilder();
            sb.Append($"状态：补卡成功\n");
            sb.Append($"补卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{ReSendModel.name}\n");
            sb.Append($"就诊卡号：{ReSendModel.Res补卡查询.data[0].patientCard}\n");
            if (PaymentModel.Total>0)
            {

                sb.Append($"补卡费用：{PaymentModel.Total.In元()}\n");
                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"试运行阶段 补卡费用为{PaymentModel.Total.In元()}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        protected Result WriteCardNo(string cardNo)
        {
            cardNo = cardNo.PadLeft(10, '0');
            var readRet = _rfCardDispenser.ReadBlock(0x00, 0x01, true, Startup.SiKey);
            if (readRet && readRet.Value.ByteToString().Substring(0,10) == cardNo) { return Result.Success(); }

            var cardNoBts = cardNo.StringToByte();
            var writeRet = _rfCardDispenser.WirteBlock(0x00, 0x01, true, Startup.SiKey, cardNoBts);
            if (writeRet)
            {
                return Result.Success();
            }
            Logger.Device.Error($"[发卡写卡]写卡失败,{writeRet.Message}");
            return writeRet;
        }
        protected void PrintCard()
        {
            var printText = new List<ZbrPrintTextItem>
            {
                new ZbrPrintTextItem()
                {
                    X = 160,
                    Y = 55,
                    Text = ReSendModel.name
                },
                new ZbrPrintTextItem()
                {
                    X = 550,
                    Y = 55,
                    FontSize = 11,
                    Text =  ReSendModel.Res补卡查询.data[0].patientCard
                }
            };
            _rfCardDispenser.PrintContent(printText);
        }

        protected virtual bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return Invoke(() =>
                    {
                        var ret = YuanTu.Default.Tools.InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });

                if (!_rfCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_rfCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _rfCardDispenser.EnterCard();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机读卡号失败");
                    return false;
                }
                CardModel.CardNo = BitConverter.ToUInt32(result.Value, 0).ToString();
                Logger.Main.Info("[建档发卡]读卡完成 卡号:" + CardModel.CardNo);
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected virtual void FillResendCardRequest(req补卡 req)
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

            var tradeModeList = new List<支付信息>();

            var item = new 支付信息
            {
                tradeMode = req.tradeMode,
                accountNo = req.accountNo,
                cash = req.cash,
                posTransNo = req.posTransNo,
                bankTransNo = req.bankTransNo,
                bankDate = req.bankDate,
                bankTime = req.bankTime,
                bankSettlementTime = req.bankSettlementTime,
                bankCardNo = req.bankCardNo,
                posIndexNo = req.posIndexNo,
                sellerAccountNo = req.sellerAccountNo,
                transNo = req.transNo,
                payAccountNo = req.payAccountNo,
                outTradeNo = req.outTradeNo
            };
            tradeModeList.Add(item);

            req.tradeModeList = tradeModeList.ToJsonString();
            req.tradeMode = "MIX";
        }
        #endregion 
    }
}