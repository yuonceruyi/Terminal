using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.House.Component.Auth.Models;
using YuanTu.Default.House.Component.Views;
using YuanTu.Default.House.HealthManager;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.House.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : ViewModelBase
    {
        protected IMagCardDispenser _magCardDispenser;
        protected IRFCardDispenser _rfCardDispenser;
        private string _hint = "个人信息";
        private bool _showUpdatePhone;

        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "ZBR_RF");

            ConfirmCommand = new DelegateCommand(Confirm);
            UpdateCommand = new DelegateCommand(() =>
            {
                IsAuth = !Phone.IsNullOrWhiteSpace();
                ShowUpdatePhone = true;
            });
            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
        }

        public override string Title => "个人信息";

        public bool ShowUpdatePhone
        {
            get { return _showUpdatePhone; }
            set
            {
                _showUpdatePhone = value;
                if (value)
                {
                    NewPhone = null;

                    PlaySound(SoundMapping.请输入手机号码);
                    ShowMask(true, new UpdatePhone { DataContext = this });
                }
                else
                {
                    ShowMask(false);
                }
            }
        }

        public ICommand ConfirmCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档)
            {
                HideNavigating = false;
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
                HideNavigating = true;
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;
                var patientInfo = HealthModel.Res查询是否已建档?.data;

                Name = patientInfo?.name;
                Sex = patientInfo?.sex;
                Birth = patientInfo?.birthday;
                Phone = patientInfo?.phone.Mask(3, 4);
                IdNo = patientInfo?.idNo.Mask(14, 3);
            }
            PlaySound(SoundMapping.个人信息);
        }

        public virtual void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                switch (CreateModel.CreateType)
                {
                    case CreateType.成人:
                        CreatePatient();
                        break;

                    case CreateType.儿童:
                        Navigate(AInner.JD.PatInfoEx);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }
            var patientInfo = HealthModel.Res查询是否已建档?.data;
            ChangeNavigationContent($"{patientInfo?.name}");
            Next();
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
            if (ChoiceModel.Business == Business.建档)
            {
                Phone = NewPhone;
                CreateModel.Phone = NewPhone;
                ShowUpdatePhone = false;
                return;
            }

            //todo Update
            DoCommand(lp =>
            {
                

                if (CardModel.CardType == CardType.就诊卡)
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
                }
               

                lp.ChangeText("正在更新健康档案个人信息，请稍候...");

                var req = new req修改手机号
                {
                    healthUserId = HealthModel.Res查询是否已建档?.data?.id,
                    idNo = HealthModel.Res查询是否已建档?.data?.idNo,
                    phone = NewPhone
                };
                var res = HealthDataHandlerEx.修改手机号(req);
                if (res?.success ?? false)
                {
                    ShowUpdatePhone = false;
                    HealthModel.Res查询是否已建档.data.phone = NewPhone;
                    Phone = NewPhone.Mask(3, 4);
                    ShowAlert(true, "个人信息", "健康档案个人信息更新成功");
                }
                else
                {
                    ShowAlert(false, "个人信息", "健康档案个人信息更新失败");
                }
            });
        }

        protected virtual void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;

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
                    tradeMode = "CA",
                    cash = null,
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
                    setupType = ((int)CreateModel.CreateType).ToString()
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    if (!FrameworkConst.DoubleClick)
                        _rfCardDispenser.PrintContent(new List<ZbrPrintTextItem> { new ZbrPrintTextItem() }, new List<ZbrPrintCodeItem> { new ZbrPrintCodeItem() });
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        //TipMsg= $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        TipMsg = $"请按上方提示的位置取走您的健康卡",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "发卡出口_House"
                    });
                    ChangeNavigationContent(".");
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }

        protected virtual bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick)
                {
                    //CardModel.CardNo = "1234567890";
                    CardModel.CardNo = DateTimeCore.Now.Ticks.ToString();
                    return true;
                }

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
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }

        protected virtual Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{IdCardModel.Name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            _rfCardDispenser?.UnInitialize();
            _rfCardDispenser?.DisConnect();
            return true;
        }

        #region Binding

        private string _name;
        private string _sex;
        private string _birth;
        private string _phone;
        private string _idNo;
        private string _newPhone;
        private bool _isAuth;
        private string _buttonContent;
        private string _tips;
        private bool _canUpdatePhone;
        private string _guardIdNo;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Sex
        {
            get { return _sex; }
            set
            {
                _sex = value;
                OnPropertyChanged();
            }
        }

        public string Birth
        {
            get { return _birth; }
            set
            {
                _birth = value;
                OnPropertyChanged();
            }
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

        public string IdNo
        {
            get { return _idNo; }
            set
            {
                _idNo = value;
                OnPropertyChanged();
            }
        }

        public string GuardIdNo
        {
            get { return _guardIdNo; }
            set
            {
                _guardIdNo = value;
                OnPropertyChanged();
            }
        }

        public string NewPhone
        {
            get { return _newPhone; }
            set
            {
                _newPhone = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuth
        {
            get { return _isAuth; }
            set
            {
                _isAuth = value;
                OnPropertyChanged();
            }
        }

        public string ButtonContent
        {
            get { return _buttonContent; }
            set
            {
                _buttonContent = value;
                OnPropertyChanged();
            }
        }

        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        public bool CanUpdatePhone
        {
            get { return _canUpdatePhone; }
            set
            {
                _canUpdatePhone = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

        #region Ioc

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        [Dependency]
        public IBusinessConfigManager BusinessConfigManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IHealthModel HealthModel { get; set; }

        #endregion Ioc
    }
}