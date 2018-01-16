using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
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
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        private const long PatientCreatedCode = -13;

        private bool MoveCardOutFail { get; set; }
        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        protected IMagCardReader _magCardReader;

        public PatientInfoViewModel(IMagCardDispenser[] magCardDispenser, IMagCardReader[] magCardReaders) : base(null)
        {
            _magCardDispenser = magCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");

            ConfirmCommand = new DelegateCommand(Confirm);
            UpdateCommand = new DelegateCommand(() =>
            {
                IsAuth = !Phone.IsNullOrWhiteSpace();
                ShowUpdatePhone = true;
            });
            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
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
            else if ((ChoiceModel.Business == Business.建档 || SiModel.NeedCreate) && CardModel.CardType == CardType.社保卡)
            {
                try
                {
                    //todo 社保卡建档处理
                    IdCardModel.Name = SiModel.医保个人基本信息?.姓名;
                    switch (SiModel.医保个人基本信息?.性别)
                    {
                        case "1":
                            IdCardModel.Sex = Consts.Enums.Sex.男;
                            break;

                        case "2":
                            IdCardModel.Sex = Consts.Enums.Sex.女;
                            break;

                        default:
                            IdCardModel.Sex = Consts.Enums.Sex.未知;
                            break;
                    }
                    IdCardModel.Birthday = Convert.ToDateTime($"{SiModel.医保个人基本信息?.出生日期.SafeSubstring(0, 4)}-{SiModel.医保个人基本信息?.出生日期.SafeSubstring(4, 2)}-{SiModel.医保个人基本信息?.出生日期.SafeSubstring(6, 2)}");
                    IdCardModel.IdCardNo = SiModel.医保个人基本信息?.公民身份号;
                    IdCardModel.Nation = null;
                    IdCardModel.Address = SiModel.医保个人基本信息?.单位名称;

                    Name = IdCardModel.Name;
                    Sex = IdCardModel.Sex.ToString();
                    Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                    Phone = null;
                    IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                    IsAuth = false;
                    ShowUpdatePhone = false;
                    CanUpdatePhone = true;
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[社保卡建档处理] {ex.Message} {ex.StackTrace}");
                }
            }
            else
            {
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
            }
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (ChoiceModel.Business == Business.建档 || SiModel.NeedCreate)
            {
                switch (CreateModel.CreateType)
                {
                    case CreateType.成人:
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
                            //Navigate(AInner.JD.Confirm);
                            Next();
                        }

                        break;

                    case CreateType.儿童:
                        Navigate(A.CK.InfoEx);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            //ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
            ChangeNavigationContent($"{patientInfo.name}");

            Next();
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
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1",
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
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
                return Result.Fail(CreateModel.Res病人建档发卡?.code ?? -100, CreateModel.Res病人建档发卡?.msg);
            }).Result;
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
                int retry = 3;
                Result<Dictionary<TrackRoad, string>> result;
                do
                {
                    result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                    if (result.IsSuccess)
                        break;
                    //读卡失败时,回收卡片重新发卡
                    if (!_magCardDispenser.MoveCard(CardPosF6.吞入, "发卡机读卡号失败，故回收卡片").IsSuccess)
                    {
                        ShowAlert(false, "建档发卡", "发卡机读卡号失败后回收卡失败");
                        return false;
                    }
                } while (retry-- > 0);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机取新卡失败");
                    return false;
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

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                if (CardModel.CardType != CardType.社保卡)
                {
                    lp.ChangeText("正在准备就诊卡，请稍候...");
                    //todo 发卡机发卡
                    if (!GetNewCardNo())
                        return false;
                }

                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1",
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = CardModel.CardType == CardType.社保卡 ? SiModel.CardHardInfo : "123456",
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString(),
                    accountNo = null,
                    patientType = CardModel.CardType == CardType.社保卡 ? "医保" : "自费",
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = CardModel.CardType == CardType.社保卡 ? SiModel.SiPatientInfo : null,
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
                    if (SiModel.NeedCreate)
                    {
                        return SiCreateJump();
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
                    return false;
                }
                if (CardModel.CardType != CardType.社保卡 && CreateModel.Res病人建档发卡?.code == PatientCreatedCode)
                    _magCardDispenser.MoveCard(CardPosF6.吞入, $"此卡[{CardModel.CardNo}]已建档，故回收卡片");

                ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                return false;
            }).ContinueWith(ctx =>
            {
                if (ctx.Result)
                    Next();
            });
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
            if (ChoiceModel.Business == Business.建档 || SiModel.NeedCreate)
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }

            //todo Update
            DoCommand(lp =>
            {
                lp.ChangeText("正在更新个人信息，请稍候...");
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                PatientModel.Req病人基本信息修改 = new req病人基本信息修改
                {
                    patientId = patientInfo.patientId,
                    platformId = patientInfo.platformId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    phone = NewPhone,
                    birthday = patientInfo.birthday,
                    guardianNo = patientInfo.guardianNo,
                    idNo = patientInfo.idNo,
                    name = patientInfo.name,
                    sex = patientInfo.sex,
                    address = patientInfo.address,
                    operId = FrameworkConst.OperatorId
                };
                PatientModel.Res病人基本信息修改 = DataHandlerEx.病人基本信息修改(PatientModel.Req病人基本信息修改);
                if (PatientModel.Res病人基本信息修改?.success ?? false)
                {
                    ShowUpdatePhone = false;
                    PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].phone = NewPhone;
                    Phone = NewPhone.Mask(3, 4);
                    ShowAlert(true, "个人信息", "个人信息更新成功");
                }
                else
                {
                    ShowAlert(false, "个人信息", "个人信息更新失败");
                }
            });
        }

        protected virtual bool SiCreateJump()
        {
            //建档成功后查询病人信息
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                secrityNo = SiModel.CardHardInfo,
                extend = SiModel.SiPatientInfo
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            if (PatientModel.Res病人信息查询.success)
            {
                if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                    return false;
                }
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                ChangeNavigationContent($"{patientInfo.name}");
                //Next();
                return true;
            }
            ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
            return false;
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

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            _magCardDispenser.UnInitialize();
            _magCardDispenser.DisConnect();
            if (ChoiceModel.Business != Business.建档 && CardModel.CardType == CardType.就诊卡)
            {
                //todo 提示取卡
                if (!FrameworkConst.Local)
                {
                    var result = CheckMagCard();
                    return result.IsSuccess;
                }
            }
            return true;
        }

        /// <summary>
        /// 检测是否卡片未取走
        /// </summary>
        /// <returns></returns>
        protected virtual Result CheckMagCard()
        {
            var ret = _magCardReader.Connect();
            if (!ret.IsSuccess)
            {
                ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                ShowAlert(false, "友好提示", $"读卡器打开失败({ret.ResultCode})", debugInfo: ret.Message);
                return Result.Fail("");
            }
            if (!_magCardReader.Initialize().IsSuccess)
            {
                ShowAlert(false, "友好提示", $"读卡器初始化失败({ret.ResultCode})", debugInfo: ret.Message);
                return Result.Fail("");
            }
            var pos = _magCardReader.GetCardPosition();
            //_magCardReader.UnInitialize();
            _magCardReader.DisConnect();
            if (pos.IsSuccess && pos.Value == CardPos.不持卡位)
            {
                ShowAlert(false, "友好提示", $"请先取走您的就诊卡");
                return Result.Fail("");
            }
            return Result.Success();
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