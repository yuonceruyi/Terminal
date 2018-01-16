using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Models.Print;
using YuanTu.WeiHaiArea.Card;
using YuanTu.WeiHaiArea;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Core.Log;
using System.Windows.Input;
using Microsoft.Practices.ObjectBuilder2;
using Prism.Commands;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Consts.Models.Create;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IMagCardDispenser[] magCardDispenser) : base(null)
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

        private bool _licensePlateControlVisibility;

        public bool LicensePlateControlVisibility
        {
            get => _licensePlateControlVisibility;
            set
            {
                _licensePlateControlVisibility = value;
                OnPropertyChanged();
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

        public override void OnEntered(NavigationContext navigationContext)
        {
            KeyBoradVisibility = false;
            ChangeNavigationContent("");
            if (ChoiceModel.Business == Business.建档)
            {
                LicensePlateControlVisibility = true;
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
                if (ChoiceModel.Business == (Business)100)
                {
                    LicensePlateControlVisibility = true;
                }
                IsAuth = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = false;
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
            }
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
                    name = IdCardModel.Name,
                    sex = IdCardModel.Sex.ToString(),
                    birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                    idNo = IdCardModel.IdCardNo,
                    idType = "1", //测试必传
                    nation = IdCardModel.Nation,
                    address = IdCardModel.Address,
                    phone = Phone,
                    pwd = "123456",
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString(),
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
                    ChangeNavigationContent($"{IdCardModel.Name}\r\n卡号{CardModel.CardNo}");
                    _magCardDispenser.MoveCardOut();
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }

        public override void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowUpdatePhone = true;
                return;
            }
            if (ChoiceModel.Business == Business.建档)
            {
                CreatePatient();
                return;
            }

            if (ChoiceModel.Business == (Business)100)
            {
                if ((string.IsNullOrEmpty(InputNo) || string.IsNullOrEmpty(ProvinceStr) ||
                     string.IsNullOrEmpty(AreaStr)))
                {
                    ShowAlert(false, "温馨提示", "请补全车牌信息");
                    return;
                }
                Update();
                return;
            }
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accountNo}元");
            Next();
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

        public void Update()
        {
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
                    operId = FrameworkConst.OperatorId,
                    licensePlateNo = $"{ProvinceStr}{AreaStr}{InputNo}"
                };
                PatientModel.Res病人基本信息修改 = DataHandlerEx.病人基本信息修改(PatientModel.Req病人基本信息修改);
                if (PatientModel.Res病人基本信息修改?.success ?? false)
                {
                    ShowUpdatePhone = false;
                    PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].phone = NewPhone;
                    Phone = NewPhone.Mask(3, 4);
                    ShowAlert(true, "个人信息", "车牌信息更新成功");
                }
                else
                {
                    ShowAlert(false, "个人信息", "车牌信息更新失败");
                }
            });
        }

    }
}
