using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Devices.CardReader;
using YuanTu.Devices.FingerPrint;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        private IFingerPrintDevice[] _fingerPrintDevices;
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IFingerPrintDevice[] fingerPrintDevices) : base(rfCardDispenser)
        {
            _fingerPrintDevices = fingerPrintDevices;
            ModifyOccupationCommand = new DelegateCommand(ModifyOccupationCmd);
            ModifyAddressCommand = new DelegateCommand(ModifyAddressCmd);
        }

        [Dependency]
        public ISiModel SiModel { get; set; }

        #region DataBinding

        private string _occupation;

        public string Occupation
        {
            get { return _occupation; }
            set
            {
                _occupation = value;
                OnPropertyChanged();
            }
        }

        private string _address;

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        private List<string> _occupations;

        public List<string> Occupations
        {
            get { return _occupations; }
            set
            {
                _occupations = value;
                OnPropertyChanged();
            }
        }

        public bool ShowChangeAddress
        {
            get => _showChangeAddress;
            set { _showChangeAddress = value;OnPropertyChanged(); }
        }

        private IReadOnlyList<AddressItem> _level0S;

        public IReadOnlyList<AddressItem> Level0s
        {
            get { return _level0S; }
            set
            {
                _level0S = value;
                OnPropertyChanged();
            }
        }
        private IReadOnlyList<AddressItem> _level1S;

        public IReadOnlyList<AddressItem> Level1s
        {
            get { return _level1S; }
            set
            {
                _level1S = value;
                OnPropertyChanged();
            }
        }
        private IReadOnlyList<AddressItem> _level2S;

        public IReadOnlyList<AddressItem> Level2s
        {
            get { return _level2S; }
            set
            {
                _level2S = value;
                OnPropertyChanged();
            }
        }
        private IReadOnlyList<AddressItem> _level3S;

        public IReadOnlyList<AddressItem> Level3s
        {
            get { return _level3S; }
            set
            {
                _level3S = value;
                OnPropertyChanged();
            }
        }

        private AddressItem _level0;

        public AddressItem Level0
        {
            get { return _level0; }
            set
            {
                _level0 = value;
                Level1 = null;
                Level1s = value?.Children;
                OnPropertyChanged();
            }
        }

        private AddressItem _level1;

        public AddressItem Level1
        {
            get { return _level1; }
            set
            {
                _level1 = value;
                Level2 = null;
                Level2s = value?.Children;
                OnPropertyChanged();
            }
        }

        private AddressItem _level2;

        public AddressItem Level2
        {
            get { return _level2; }
            set
            {
                _level2 = value;
                Level3 = null;
                Level3s = value?.Children;
                OnPropertyChanged();
            }
        }

        private AddressItem _level3;

        public AddressItem Level3
        {
            get { return _level3; }
            set
            {
                _level3 = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public DelegateCommand ModifyOccupationCommand { get; set; }
        public DelegateCommand ModifyAddressCommand { get; set; }
        public List<string> fingetDataList =new List<string>();
        private bool _showChangeAddress;

        public virtual void ModifyOccupationCmd()
        {
            Occupation = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { Occupation = p; },
                KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }
            }, 0.1, pt => { ShowMask(false); });
        }

        public virtual void ModifyAddressCmd()
        {
            // Address = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { if (!string.IsNullOrEmpty(p)) Address = p; },
                KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }
            }, 0.1, pt => { ShowMask(false); });
        }

        public override void OnSet()
        {
            Occupations = new List<string>()
            {
                "工人",
                "学生",
                "农民",
                "其他",
                "干部",
                "退(离)休人员",
                "国家公务员",
                "专业技术人员",
                "职员",
                "企业管理人员",
                "现役军人",
                "自由职业者",
                "个体经营者",
                "无业人员",
            };
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            TopBottom.InfoItems = null;
           

            if (ChoiceModel.Business == Business.建档 && CardModel.CardType == CardType.身份证)
            {
                IsAuth = false;
                ShowChangeAddress = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;

                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                Address = IdCardModel.Address;
            }
            else if ((ChoiceModel.Business == Business.建档 || SiModel.NeedCreate) && CardModel.CardType == CardType.社保卡)
            {
                IsAuth = false;
                ShowChangeAddress = true;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;

                Name = IdCardModel.Name;
                Sex = IdCardModel.Sex.ToString();
                Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                Phone = null;
                IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                Address = IdCardModel.Address;
            }
            
            else
            {
                IsAuth = true;
                ShowChangeAddress = false;
                ShowUpdatePhone = false;
                CanUpdatePhone = true;

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                Name = patientInfo.name;
                Sex = patientInfo.sex;
                Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
                Phone = patientInfo.phone.Mask(3, 4);
                IdNo = patientInfo.idNo.Mask(14, 3);
                GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
                Address = "";
                Logger.Main.Info($"患者信息 Name={Name}，Phone={Phone}，IdNo={IdNo}");
            }
            if (ChoiceModel.Business == Business.建档 || SiModel.NeedCreate)
            {
                try
                {
                  
                    Logger.Main.Info($"[测试]身份证号:{IdNo}");
                    Level0s = Startup.AddressInfo.Level0;
                    if (IdNo != null && IdNo.Length >= 6)
                    {
                        var l0 = IdNo.Substring(0, 2);
                        var l1 = IdNo.Substring(0, 4);
                        var l2 = IdNo.Substring(0, 6);
                        Level0 = Level0s.FirstOrDefault(i => i.Code.StartsWith(l0));
                        Level1 = Level1s?.FirstOrDefault(i => i.Code.StartsWith(l1));
                        Level2 = Level2s?.FirstOrDefault(i => i.Code.StartsWith(l2));
                        if (Address.StartsWith(Level0?.Name??"ABCEDF_")) { Address = Address.Remove(0, Level0.Name.Length); }
                        if (Address.StartsWith(Level1?.Name?? "ABCEDF_")) { Address = Address.Remove(0, Level1.Name.Length); }
                        if (Address.StartsWith(Level2?.Name?? "ABCEDF_")) { Address = Address.Remove(0, Level2.Name.Length); }
                    }
                    else
                    {
                        Level0 = null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Info($"加载区域信息失败{ex.Message}\r\n{ex.StackTrace}");
                }
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
                if (string.IsNullOrEmpty(Occupation))
                {
                    ShowAlert(false, "建档发卡", "请输入职业");
                    return;
                }
                if (string.IsNullOrEmpty(Address))
                {
                    ShowAlert(false, "建档发卡", "请输入住址");
                    return;
                }
                if (Level0==null|| Level1==null|| Level2==null)
                {
                    ShowAlert(false, "建档发卡", "请选择住址");
                    return;
                }
                if (Level3 == null)
                {
                    Level3 = new AddressItem();
                    Level3.Name = "";
                }
                // 用IdCardModel传递地址和职业
                IdCardModel.Address = Level0.Name + Level1.Name + Level2.Name + Level3.Name + Address;
                IdCardModel.GrantDept = Occupation;
                switch (CreateModel.CreateType)
                {
                    case CreateType.成人:
                        if (Startup.Biometric)
                            CreatePatient2();
                        else
                            DoCommand(CreatePatient);
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

            Logger.Main.Info("准备跳转");
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
                    birthday =  patientInfo.birthday.SafeToSplit(' ', 1)[0],
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

        protected void CreatePatient(LoadingProcesser lp)
        {
            string cardType;//实际用的卡的类型 身份证发就诊卡用就诊卡 社保卡激活用社保卡
            if (CardModel.CardType != CardType.社保卡 || Startup.SendCardBySiCard)
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                Logger.Main.Info("开始读卡写卡");
                if (GetNewCardNo())
                {
                    Logger.Main.Info("读卡完成 cn=" + CardModel.CardNo);
                    if (WriteCardNo(CardModel.CardNo))
                    {
                        Logger.Main.Info("写卡号完成");
                        cardType = ((int)CardType.就诊卡).ToString();
                    }
                    else
                    {
                        Logger.Main.Info("写卡号失败");
                        return;
                    }
                }
                else
                {
                    Logger.Main.Info("读卡失败");
                    return;
                }
            }
            else
            {
                cardType = ((int)CardType.社保卡).ToString();
            }

            CreateModel.Req病人建档发卡 = new req病人建档发卡
            {
                operId = FrameworkConst.OperatorId,
                cardNo = CardModel.CardNo,
                cardType = cardType,
                name = IdCardModel.Name,
                sex = IdCardModel.Sex.ToString(),
                birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                idNo = IdCardModel.IdCardNo,
                idType = "1", //测试必传
                nation = IdCardModel.Nation,
                address = IdCardModel.Address,
                phone = Phone,
                setupType = ((int)CreateModel.CreateType).ToString(),
                extend = IdCardModel.GrantDept,

            };
            lp.ChangeText("正在建档，请稍候...");
            Logger.Main.Info("建档发卡信息赋值完成");

            CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
            if (!CreateModel.Res病人建档发卡.success)
            {
                ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                return;
            }
            if (CardModel.CardType==CardType.社保卡)//顺便把社保卡也建了得了
            {
                try
                {
                    lp.ChangeText("社保卡信息上送平台，请稍后...");
                    var reqJDd = new req病人建档发卡
                    {
                        operId = FrameworkConst.OperatorId,
                        cardNo = SiModel.社保卡卡号,
                        cardType = ((int) CardType.社保卡).ToString(),
                        name = IdCardModel.Name,
                        sex = IdCardModel.Sex.ToString(),
                        birthday = IdCardModel.Birthday.ToString("yyyy-MM-dd"),
                        idNo = IdCardModel.IdCardNo,
                        idType = "1", //测试必传
                        nation = IdCardModel.Nation,
                        address = IdCardModel.Address,
                        phone = Phone,
                        setupType = ((int) CreateModel.CreateType).ToString(),
                        extend = IdCardModel.GrantDept,

                    };
                    DataHandlerEx.病人建档发卡(reqJDd);
                }
                catch (Exception e)
                {

                }

            }

            if (CardModel.CardType == CardType.社保卡 && !Startup.SendCardBySiCard)
            {
                Logger.Main.Info("社保卡建档，重庆已废弃");
                if (SiModel.NeedCreate)
                    if (!SiCreate())
                        return;
            }
            else
            {
                #region 指纹上传
                try
                {
                    Logger.Main.Info("开始指纹上传");
                    var 指纹 = new req指纹信息上传()
                    {
                        idNo = IdCardModel.IdCardNo,
                        name = IdCardModel.Name,
                        cardNo = CardModel.CardNo,
                        cardType = cardType,
                        sex = IdCardModel.Sex.ToString(),
                        address = IdCardModel.Address,
                        phone = Phone,
                        rightFinger = fingetDataList[0],
                        leftFinger = fingetDataList[1],
                    };
                    var r = DataHandlerEx.指纹信息上传(指纹);
                    Logger.Main.Info("指纹上传" + (r.success ? "成功" : "失败"));
                }
                catch (Exception ex)
                {
                    Logger.Main.Info("指纹上传异常 " + ex.Message + "\r\n" + ex.StackTrace);
                }
                #endregion
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
            }

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
            Wait(false);
            Next();
        }

        protected bool WriteCardNo(string CardNo)
        {
            CardNo= CardNo.PadLeft(10, '0');
            var r_B00 = _rfCardDispenser.ReadBlock(0x00, 0x01, true, Startup.SiKey);
            if (r_B00.IsSuccess && r_B00.Value.ByteToString().Substring(0, 10) == CardNo) { return true; }
            var b00 = CardNo.StringToByte();
            var w_B00 = _rfCardDispenser.WirteBlock(0x00, 0x01, true, Startup.SiKey, b00);
            return w_B00.IsSuccess;
        }
        protected void CreatePatient2()
        {
            var viewModel = new FingerPrintViewModel(_fingerPrintDevices)
            {
                Action = (lp, s) =>
                {
                    Logger.Main.Info($"指纹返回");
                    fingetDataList = s;
                    fingetDataList.Add("");
                    fingetDataList.Add("");
                    Logger.Main.Info($"ShowMask(false);");
                    ShowMask(false);
                    CreatePatient(lp);
                },
                ResourceEngine = ResourceEngine,
            };
            var element = new FingerPrintView()
            {
                DataContext = viewModel,
            };
            ShowMask(true, element, 0.4, p =>
           {
               viewModel.OnLeaving(null);
               ShowMask(false);
           });
            viewModel.OnSet();
            viewModel.OnEntered(null);
        }

        protected override void PrintCard()
        {
            var printText = new List<ZbrPrintTextItem>
            {
                new ZbrPrintTextItem()
                {
                    X = 160,
                    Y = 55,
                    Text = Name
                },
                new ZbrPrintTextItem()
                {
                    X = 550,
                    Y = 55,
                    FontSize = 11,
                    Text =  CreateModel.Res病人建档发卡.data.patientCard
                }
            };
            _rfCardDispenser.PrintContent(printText);
        }

        protected virtual bool SiCreate()
        {
            //建档成功后查询病人信息
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                cardNo = SiModel.Res获取人员基本信息.身份证号,
                cardType = ((int)CardType.身份证).ToString()
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);
            if (!PatientModel.Res病人信息查询.success)
            {
                ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                return false;
            }
            if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
            {
                ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                return false;
            }
            return true;
        }
        protected override Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{IdCardModel.Name}\n");
            //sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"就诊卡号：{CreateModel.Res病人建档发卡.data.patientCard}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请及时取走卡片\n");
            sb.Append($"请妥善保管好您的个人信息\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

    }
}