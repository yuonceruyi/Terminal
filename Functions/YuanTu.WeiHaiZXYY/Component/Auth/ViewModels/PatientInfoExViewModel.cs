using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Sounds;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IMagCardDispenser[] magCardDispenser) :base(null)
        {
            _magCardDispenser = magCardDispenser.FirstOrDefault(p => p.DeviceId == "Act_F6_Mag");
            ConfirmCommand = new DelegateCommand(Confirm);
            UpdateCommand = new DelegateCommand(() =>
            {
                IsAuth = !Phone.IsNullOrWhiteSpace();
                ShowUpdatePhone = true;
            });
            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
            ButtonContent = "添加";
        }

        public ICommand UpdateCommand { get; set; }
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }
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
        }
        protected override bool GetNewCardNo()
        {
            Logger.Device.Info("进入发卡机操作");
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

                }
                Logger.Device.Info("开始连接");
                if (!_magCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                Logger.Device.Info("连接成功，开始初始化");
                if (!_magCardDispenser.Initialize().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机初始化失败");
                    return false;
                }
                Logger.Device.Info("初始化成功，开始获取卡号");
                var result = _magCardDispenser.EnterCard(TrackRoad.Trace2);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机读卡号失败");
                    return false;
                }
                Logger.Device.Info("获取卡号成功：" + result.Value[TrackRoad.Trace2]);
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

        public override void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ModifyNameCommand.Execute(null);
                return;
            }
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            base.Confirm();
        }

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;


                var portraitStr = string.Empty;
                int byteLength = 0;
                if (File.Exists(IdCardModel.PortraitPath))
                {
                    var bs = File.ReadAllBytes(IdCardModel.PortraitPath);
                    byteLength = bs.Length;
                    portraitStr = Convert.ToBase64String(bs);
                }
                Logger.Device.Info($"{byteLength},{portraitStr}");
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "1",
                    name = Name,
                    sex = IsBoy?"男":"女",
                    birthday = DateTime.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    pwd = "123456",
                    tradeMode = "CA",
                    phone=Phone,
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    guardianName=IdCardModel.Name,
                    guardianNo=IdCardModel.IdCardNo,
                    extend = $"{byteLength}|{portraitStr}",
                    licensePlateNo = $"{ProvinceStr}{AreaStr}{InputNo}"
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });
                    ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                    _magCardDispenser.MoveCardOut();
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }

        public override void ModifyNameCmd()
        {
            Name = "";
            ShowMask(true, new FullInputBoard()
            {
                InputMode= Default.Component.Auth.Dialog.Views.InputMode.HandInput,
                SelectWords = p => { Name = p; },
                KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }
            }, 0.1, pt => { ShowMask(false); });
        }

        #region Binding
        private bool _showUpdatePhone;
        private string _buttonContent;
        private string _phone;
        private string _tips;
        private string _newPhone;
        private bool _isAuth;

        public string ButtonContent
        {
            get { return _buttonContent; }
            set
            {
                _buttonContent = value;
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
        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
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
        public bool IsAuth
        {
            get { return _isAuth; }
            set
            {
                _isAuth = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _provinceResource = new ObservableCollection<string>();

        public ObservableCollection<string> ProvinceResource
        {
            get
            {
                if (_provinceResource.Count <= 0)
                {
                    "鲁京津沪渝冀豫云辽黑湘皖新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领".ForEach(p =>
                    {
                        _provinceResource.Add(p.ToString());
                    });
                }
                return _provinceResource;
            }
        }

        private ObservableCollection<string> _areaResource = new ObservableCollection<string>();

        public ObservableCollection<string> AreaResource
        {
            get
            {
                if (_areaResource.Count <= 0)
                {
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ForEach(p =>
                    {
                        _areaResource.Add(p.ToString());
                    });
                }
                return _areaResource;
            }
        }

        private bool _keyBoradVisibility;

        public bool KeyBoradVisibility
        {
            get => _keyBoradVisibility;
            set
            {
                _keyBoradVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _inputNo;

        public string InputNo
        {
            get => _inputNo;
            set
            {
                _inputNo = value;
                OnPropertyChanged();
            }
        }

        private string _provinceStr;

        public string ProvinceStr
        {
            get => _provinceStr;
            set
            {
                _provinceStr = value;
                OnPropertyChanged();
            }
        }

        private string _areaStr;

        public string AreaStr
        {
            get => _areaStr;
            set
            {
                _areaStr = value;
                OnPropertyChanged();
            }
        }

        public ICommand InputCommand => new DelegateCommand(() => { KeyBoradVisibility = !KeyBoradVisibility; });

        #endregion Binding
    }
}