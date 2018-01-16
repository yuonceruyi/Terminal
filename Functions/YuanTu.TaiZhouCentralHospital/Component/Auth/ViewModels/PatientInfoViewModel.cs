using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Practices.Unity;
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
using YuanTu.TaiZhouCentralHospital.HealthInsurance;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;

namespace YuanTu.TaiZhouCentralHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
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

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;

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
            else if ((ChoiceModel.Business == Business.建档 || SiModel.NeedCreate) &&
                     (CardModel.CardType == CardType.社保卡 || CardModel.CardType == CardType.居民健康卡))
            {
                try
                {
                    //todo 社保卡建档处理
                    if (CardModel.CardType == CardType.社保卡)
                    {
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
                        IdCardModel.Birthday =
                            Convert.ToDateTime(
                                $"{SiModel.医保个人基本信息?.出生日期.SafeSubstring(0, 4)}-{SiModel.医保个人基本信息?.出生日期.SafeSubstring(4, 2)}-{SiModel.医保个人基本信息?.出生日期.SafeSubstring(6, 2)}");
                        IdCardModel.IdCardNo = SiModel.医保个人基本信息?.公民身份号;
                        IdCardModel.Nation = null;
                        IdCardModel.Address = SiModel.医保个人基本信息?.单位名称;
                    }
                    else
                    {
                        IdCardModel.Name = SiModel.健康卡信息?.姓名;
                        switch (SiModel.健康卡信息?.性别)
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
                        IdCardModel.Birthday =
                            Convert.ToDateTime(
                                $"{SiModel.健康卡信息?.出生日期.SafeSubstring(0, 4)}-{SiModel.健康卡信息?.出生日期.SafeSubstring(4, 2)}-{SiModel.健康卡信息?.出生日期.SafeSubstring(6, 2)}");
                        IdCardModel.IdCardNo = SiModel.健康卡信息?.证件号码;
                        IdCardModel.Nation = null;
                        IdCardModel.Address = SiModel.健康卡信息?.地址;
                    }

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
                    Logger.Main.Error($"[社保卡/健康卡建档处理] {ex.Message} {ex.StackTrace}");
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
                        if (CardModel.CardType == CardType.社保卡 || CardModel.CardType == CardType.居民健康卡)
                            CreatePatient();
                        else
                            DoCommand(lp =>
                            {
                                lp.ChangeText("正在准备就诊卡，请稍候...");
                                //todo 发卡机发卡
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
            ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");

            var resource = ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.name,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "余额",
                    Value = patientInfo.accBalance.In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });

            Next();
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
                    cardType = ((int) CardModel.CardType).ToString(),
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

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int) CardModel.CardType).ToString(),
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
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int) CreateModel.CreateType).ToString(),
                    extend =
                        CardModel.CardType != CardType.社保卡
                            ? null
                            : $"{SiModel.SiPatientInfo}&{SiModel.医保个人基本信息.个人社保编号}&{SiModel.医保个人基本信息.卡号}"
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    if (SiModel.NeedCreate)
                        return SiCreateJump();

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "激活成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条"
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

                    Next();
                    return false;
                }

                if (CardModel.CardType == CardType.社保卡)
                    ShowAlert(false, "医保卡建档", "医保卡建档失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                else if (CardModel.CardType == CardType.居民健康卡)
                    ShowAlert(false, "健康卡建档", "健康卡建档失败", debugInfo: CreateModel.Res病人建档发卡?.msg);

                return false;
            }).ContinueWith(ctx =>
            {
                if (ctx.Result)
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
                    cardType = ((int) CardModel.CardType).ToString(),
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
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString(),
                    accountNo = null,
                    patientType = null,
                    setupType = ((int) CreateModel.CreateType).ToString(),
                    extend =
                        CardModel.CardType != CardType.社保卡
                            ? null
                            : $"{SiModel.SiPatientInfo}&{SiModel.医保个人基本信息.个人社保编号}&{SiModel.医保个人基本信息.卡号}"
                };
                FillRequest(CreateModel.Req病人建档发卡);
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);

                ExtraPaymentModel.Complete = true;

                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡");
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
                    return Result.Success();
                }
                //todo 现金支付时打印单边帐凭条
                if (PaymentModel.PayMethod == PayMethod.现金)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = $"建档失败,已投入:{ExtraPaymentModel.TotalMoney.In元()},执凭条联系工作人员.",
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

        protected virtual bool SiCreateJump()
        {
            //建档成功后查询病人信息
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardType = ((int) CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo
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

        protected override Queue<IPrintable> CreatePrintables()
        {
            if (CardModel.CardType != CardType.社保卡 && CardModel.CardType != CardType.居民健康卡)
            {
                var queue = PrintManager.NewQueue("自助发卡");

                var sb = new StringBuilder();
                sb.Append($"状态：办卡成功\n");
                sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
                sb.Append($"姓名：{IdCardModel.Name}\n");
                sb.Append($"卡号：{CardModel.CardNo}\n");
                sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"请妥善保管好您的个人信息。\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText {Text = sb.ToString()});
                return queue;
            }
            if (CardModel.CardType == CardType.社保卡)
            {
                var queue = PrintManager.NewQueue("医保卡激活");

                var sb = new StringBuilder();
                sb.Append($"状态：医保卡激活成功\n");
                sb.Append($"激活单位：{FrameworkConst.HospitalName}\n");
                sb.Append($"姓名：{IdCardModel.Name}\n");
                sb.Append($"医保卡号：{SiModel.医保个人基本信息?.卡号}\n");
                sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"请妥善保管好您的个人信息。\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText {Text = sb.ToString()});
                return queue;
            }
            if (CardModel.CardType == CardType.居民健康卡)
            {
                var queue = PrintManager.NewQueue("健康卡激活");
                var sb = new StringBuilder();
                sb.Append($"状态：健康卡激活成功\n");
                sb.Append($"激活单位：{FrameworkConst.HospitalName}\n");
                sb.Append($"姓名：{IdCardModel.Name}\n");
                sb.Append($"健康卡卡号：{SiModel.健康卡信息?.市民卡健康卡卡号}\n");
                sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"请妥善保管好您的个人信息。\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText {Text = sb.ToString()});
                return queue;
            }
            return null;
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
                if (InnerConfig.发卡类型==发卡类型.就诊卡) //就诊卡
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
                if (InnerConfig.发卡类型==发卡类型.健康卡) //健康卡
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
                        姓名 = IdCardModel.Name,
                        出生日期 = IdCardModel.Birthday.ToString("yyyyMMdd"),
                        性别 = IdCardModel.Sex == Consts.Enums.Sex.男 ? "1" : "2",
                        证件类型 = "00",
                        联系电话 = Phone,
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
                    new List<ZbrPrintCodeItem> {new ZbrPrintCodeItem()});
        }
    }
}