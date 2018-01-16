using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.YuHangZYY.Component.Auth.Models;
using YuanTu.YuHangZYY.NativeService;
using YuanTu.YuHangZYY.NativeService.Dto;

namespace YuanTu.YuHangZYY.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
            : base(rfCardDispenser)
        {
            _magCardDispenser = magCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
        }

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
                var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                    GetNewCardNo();
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

        protected override void PrintCard()
        {
            _magCardDispenser.Connect();
            _magCardDispenser.Initialize();
            _magCardDispenser?.MoveCardOut();
            _magCardDispenser?.UnInitialize();
            _magCardDispenser?.DisConnect();
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            if (CardModel.ExternalCardInfo == "社保_信息补全")
            {
                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                IsAuth = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
            }
            else
            {
                base.OnEntered(navigationContext);

            }

        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            _magCardDispenser?.UnInitialize();
            _magCardDispenser?.DisConnect();
            return true;
        }


        public override void UpdateConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPhone))
            {
                ShowAlert(false, "温馨提示", "请输入手机号");
                return;
            }
            if (!NewPhone.IsHandset())
            {
                ShowAlert(false, "温馨提示", "请输入正确的手机号");
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
            if (CardModel.ExternalCardInfo == "社保_信息补全")
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }
            base.UpdateConfirm();
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (CardModel.ExternalCardInfo == "社保_信息补全")
            {
                SiCardFillInfo();
            }
            else
            {
                base.Confirm();
                var local = TopBottom as YuanTu.YuHangZYY.Models.TopBottomModel;
                if (local!=null)
                {
                    var patientInfo = PatientModel.当前病人信息;
                    local.Message =
                        $"姓名:{patientInfo.name} 就诊卡号:{patientInfo.cardNo} 院内账户:{patientInfo.accBalance.In元()}";
                }
                // GetInstance<ITopBottomModel>().InfoItems
            }
        }

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;

                PatientModel.Res病人信息查询=new res病人信息查询()
                {
                    success = true,
                    data = new List<病人信息>()
                    {
                        new 病人信息()
                        {
                            name = IdCardModel.Name,
                            sex = IdCardModel.Sex.ToString(),
                            birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                            idNo = IdCardModel.IdCardNo,
                            cardNo = CardModel.CardNo,
                            address = IdCardModel.Address,
                            phone = NewPhone,
                            accBalance = "0",
                        }
                    }
                };
                var payment = GetInstance<IPaymentModel>();
                payment.Self = 1 * 100;
                payment.Insurance = 0;
                payment.Total = 1 * 100;
                payment.NoPay = false;
                payment.ConfirmAction = CreateConfirm;
                payment.MidList = new List<PayInfoItem>()
                {
                    new PayInfoItem("病人姓名：", IdCardModel.Name),
                    new PayInfoItem("身份证号：", IdCardModel.IdCardNo.Mask(14, 3)),
                    new PayInfoItem("办卡费用：", payment.Self.In元(), true),
                };

                Next();
            });
        }

        private Result CreateConfirm()
        {
            return DoCommand(lp =>
            {
                var payment = GetInstance<IPaymentModel>();
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = "123456",
                    tradeMode = payment.PayMethod.GetEnumDescription(),
                    cash = payment.Self.ToString("0"),
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString()
                };
                FillRechargeRequest(CreateModel.Req病人建档发卡);
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    PrintCard();
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{CardModel.CardNo}");

                    var resource = ResourceEngine;
                    GetInstance<ITopBottomModel>().InfoItems = new ObservableCollection<InfoItem>(new[]
                    {
                        new InfoItem
                        {
                            Title = "姓名",
                            Value = IdCardModel.Name,
                            Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                        },
                        new InfoItem
                        {
                            Title = "卡号",
                            Value = CardModel.CardNo,
                            Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                        }
                    });

                    Navigate(A.JD.Print);
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                    Preview();
                    return Result.Fail(CreateModel.Res病人建档发卡?.msg);
                }
                return Result.Success();
            }).Result;

        }

        protected void FillRechargeRequest(req病人建档发卡 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
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
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }


        private void SiCardFillInfo()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在注册您的身份信息，请稍后...");
                var cm = CardModel as CardModel;
                var ret = LianZhongHisService.ExcuteHospitalAddArchive(new AddArchiveRequest()
                {
                    CardNo = CardModel.CardNo,
                    Phone = NewPhone,
                    HomeAddress = IdCardModel.Address,
                    IdNumber = IdCardModel.IdCardNo,
                    HealthCareCardContent = cm.Res读接触卡号.卡号识别码
                });
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "注册失败", "你的身份信息注册失败，请到窗口处理！", debugInfo: ret.Message);
                    return;
                }
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
                if (!PatientModel.Res病人信息查询.success)
                {
                    ShowAlert(false, "注册失败", "你的身份信息注册失败，\r\n请到窗口处理！");
                    Navigate(A.Home);
                    return;
                }
                var patientInfo = PatientModel.当前病人信息;
                ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
                Next();
            });
        }
    }
}