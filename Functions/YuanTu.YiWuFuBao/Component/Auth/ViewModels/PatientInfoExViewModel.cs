using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel:YuanTu.Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        [Dependency]
        public IPaymentModel PaymentModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public string Tips
        {
            get { return _tips; }
            set { _tips = value;OnPropertyChanged(); }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                ButtonContent = _phone == null ? "添加" : "修改";
                Tips = _phone == null ? "添加手机号" : "修改手机号";
                OnPropertyChanged();
            }
        }

        public bool IsAuth
        {
            get { return _isAuth; }
            set { _isAuth = value; OnPropertyChanged();}
        }

        public string ButtonContent
        {
            get { return _buttonContent; }
            set { _buttonContent = value; OnPropertyChanged();}
        }

        public bool CanUpdatePhone
        {
            get { return _canUpdatePhone; }
            set { _canUpdatePhone = value; OnPropertyChanged(); }
        }

        public bool ShowUpdatePhone
        {
            get { return _showUpdatePhone; }
            set
            {
                _showUpdatePhone = value;
                if (value)
                {
                    ShowMask(true, new UpdatePhone {DataContext = this});
                }
                else
                {
                    ShowMask(false);
                }
            }
        }

        public string NewPhone
        {
            get { return _newPhone; }
            set { _newPhone = value; OnPropertyChanged();}
        }

        public ICommand UpdateCommand { get; set; }
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }
        private readonly IRFCpuCardDispenser _rfCpuCardDispenser;
        private string _phone;
        private bool _isAuth;
        private bool _canUpdatePhone;
        private string _buttonContent;
        private bool _showUpdatePhone;
        private string _newPhone;
        private string _tips;

        public PatientInfoExViewModel(IRFCpuCardDispenser[] rfCpuCardDispenser) : base(null)
        {
            _rfCpuCardDispenser = rfCpuCardDispenser.FirstOrDefault(p => p.DeviceId == "Act_F3_RFIC");
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
            base.OnEntered(navigationContext);
            var patient = PatientModel.Res病人信息查询?.data?.FirstOrDefault();
            if (patient?.phone.IsHandset() ?? false)
            {
                CreateModel.Phone =NewPhone= Phone = patient?.phone;

            }
            else
            {
                Phone = null;
            }
            Name = patient?.name;
            CanUpdatePhone = true;

        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {

            _rfCpuCardDispenser.UnInitialize();
            _rfCpuCardDispenser.DisConnect();
            return base.OnLeaving(navigationContext);
        }

        public override void ModifyNameCmd()
        {
            //不进行任何操作
            //base.ModifyNameCmd();
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            base.Confirm();
        }

        protected override bool GetNewCardNo()

        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                {
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
                    //CardModel.CardNo = "1234567890";

                }

                if (!_rfCpuCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                if (!_rfCpuCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                var result = _rfCpuCardDispenser.EnterCard();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机内无卡", debugInfo: result.Message);
                    ReportService.卡已耗尽(null, ErrorSolution.卡已耗尽);
                    return false;
                }
                //00A4040009A00000000386980701
                //00  A4  04  00  09  A0  00  00  00  03  86  98  07  01
                //00 B0 95 0C 0A
                var appOrder = new byte[] { 0x00, 0xA4, 0x04, 0x00, 0x09, 0xA0, 0x00, 0x00, 0x00, 0x03, 0x86, 0x98, 0x07, 0x01 };
                var rest = _rfCpuCardDispenser.CpuTransmit(appOrder);//选择应用
                var cardIdOrder = new byte[] { 0x00, 0xB0, 0x95, 0x0C, 0x0A };
                rest = _rfCpuCardDispenser.CpuTransmit(cardIdOrder);//读文件内容
                if (!rest.IsSuccess)
                {
                    ShowAlert(false, "友好提示", $"发卡器指令发送失败", debugInfo: rest.Message);
                    return false;
                }
                if (rest.Value.Skip(rest.Value.Length - 2).SequenceEqual(new byte[] { 144, 0 }))//这个F3
                                                                                                // if (rest.Value.Skip(rest.Value.Length - 2).ToArray().SequenceEqual(new byte[] {111, 0}))//这个F6

                {
                    CardModel.CardNo = BitConverter.ToString(rest.Value, 0, 8).Replace("-", "");
                    return true;
                }
                ShowAlert(false, "友好提示", $"发卡指令发送失败", debugInfo: rest.Message);
                return false;
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
            if (CardModel.CardType == CardType.社保卡 || (CardModel.ExternalCardInfo ?? "").Contains("补全信息"))
            {
                Task.Run(() =>
                {
                    CreateModel.Phone = NewPhone;
                    DoCreatePatient();
                });
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                if (!GetNewCardNo())
                    return;
                PaymentModel.Self = 1;
                PaymentModel.Insurance = 0;
                PaymentModel.Total = PaymentModel.Self + PaymentModel.Insurance;
                PaymentModel.NoPay = false;
                PaymentModel.PayMethod = PayMethod.未知;
                PaymentModel.ConfirmAction = DoCreatePatient;
                PaymentModel.MidList = new List<PayInfoItem>()
                {
                    new PayInfoItem("病人姓名：", Name),
                    new PayInfoItem("监护人姓名：", IdCardModel.Name),
                    new PayInfoItem("监护人身份证号：", IdCardModel.IdCardNo.Mask(14,3)),
                    new PayInfoItem("办卡费用：", PaymentModel.Self.In元(), true),
                };
                Next();
            });
        }

        /// <summary>
        /// 金额支付成功后，向平台建档
        /// </summary>
        /// <returns></returns>
        private Result DoCreatePatient()
        {
            return DoCommand(lp =>
            {
                var ybPatientInfo = string.Empty;
                if (CardModel.CardType == CardType.社保卡)
                {
                    var cd = CardModel as CardModel;
                    ybPatientInfo = cd?.参保人员信息?.报文出参;
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
                    pwd = "123456",
                    tradeMode = "CA",
                    cash = "0",
                    accountNo = null,
                    patientType = null,
                    //bankCardNo = pos?.CardNo,
                    //bankTime = pos?.TransTime,
                    //bankDate = pos?.TransDate,
                    //posTransNo = pos?.Trace,
                    //bankTransNo = pos?.Ref,
                    //deviceInfo = pos?.TId,
                    //sellerAccountNo = pos?.MId,
                    //setupType = ChaKa.GrardId ? "2" : "1",
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = ybPatientInfo
                };
                FillRequest(CreateModel.Req病人建档发卡);
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    if (CardModel.CardType == CardType.社保卡 || (CardModel.ExternalCardInfo ?? "").Contains("补全信息"))
                    {
                        lp.ChangeText("获取最新信息，请稍后...");
                        PatientModel.Req病人信息查询 = new req病人信息查询
                        {
                            cardNo = CardModel.CardNo,
                            cardType = ((int)CardModel.CardType).ToString()
                        };
                        PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                        ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                        var sm = GetInstance<IShellViewModel>();
                        sm.Busy.IsBusy = false;
                        Next();
                        return Result.Success();
                    }

                    lp.ChangeText("正在发卡，请及时取卡");
                    if (!FrameworkConst.DoubleClick)
                        _rfCpuCardDispenser.MoveCardOut();
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{CardModel.CardNo}");
                    Next();
                    return Result.Success();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                    return Result.Fail(CreateModel.Res病人建档发卡?.code ?? -100, CreateModel.Res病人建档发卡?.msg);
                }
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
                    req.bankCardNo = thirdpayinfo.buyerAccount;
                }
            }
        }
        public virtual void UpdateConfirm()
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
            Phone = NewPhone;
            CreateModel.Phone = NewPhone;
            ShowUpdatePhone = false;
            return;

        }
    }
}
