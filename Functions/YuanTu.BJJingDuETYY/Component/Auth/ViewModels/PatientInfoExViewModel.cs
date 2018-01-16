using System;
using System.Collections.Generic;
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
using YuanTu.BJArea.Card;
using YuanTu.BJArea;
using YuanTu.Core.Reporter;
using YuanTu.Core.Log;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Component.Auth.Dialog.Views;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;
using YuanTu.BJArea.Base;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoExViewModel
    {
        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
            UpdateCommand = new DelegateCommand(() =>
            {
                ShowUpdatePhone = true;
            });
            UpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePhone = false; });
            UpdateConfirmCommand = new DelegateCommand(UpdateConfirm);
            ModifyNameCommand = new DelegateCommand<string>(ModifyNameCmd);
        }
        public ICommand UpdateCommand { get; set; }
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Hint = "请录入办卡信息" ;
            Phone = null;
            LoadData();
        }
        public void LoadData()
        {
            try
            {
                ShowChangeAddress = true;
                var IdNo = IdCardModel.IdCardNo;
                Address = IdCardModel.Address??"";
                Logger.Main.Info($"身份证号:{IdNo}，地址{Address}");
                Level0s = YuanTu.BJArea.Startup.AddressInfo.Level0;
                if (IdNo != null && IdNo.Length >= 6)
                {
                    var l0 = IdNo.Substring(0, 2);
                    var l1 = IdNo.Substring(0, 4);
                    var l2 = IdNo.Substring(0, 6);
                    Level0 = Level0s.FirstOrDefault(i => i.Code.StartsWith(l0));
                    Level1 = Level1s?.FirstOrDefault(i => i.Code.StartsWith(l1));
                    Level2 = Level2s?.FirstOrDefault(i => i.Code.StartsWith(l2));
                    if (Address.StartsWith(Level0?.Name ?? "ABCEDF_")) { Address = Address.Remove(0, Level0.Name.Length); }
                    if (Address.StartsWith(Level1?.Name ?? "ABCEDF_")) { Address = Address.Remove(0, Level1.Name.Length); }
                    if (Address.StartsWith(Level2?.Name ?? "ABCEDF_")) { Address = Address.Remove(0, Level2.Name.Length); }
                }
                else
                {
                    Level0 = null;
                }

                RelationL = BJArea.Startup.RelationInfo.List;
                ReligionL = BJArea.Startup.ReligionInfo.List;
                NationL = BJArea.Startup.NationInfo.List;
                EducationL = BJArea.Startup.EducationInfo.List;
                Guardian = IdCardModel.Name??"";
                Nation =NationL.FirstOrDefault(i => i.Name.StartsWith(IdCardModel.Nation));
                OnPropertyChanged();
            }
            catch (Exception ex)
            {
                Logger.Main.Info($"加载区域信息失败{ex.Message}\r\n{ex.StackTrace}");
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
            if (Guardian.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请输入监护人姓名");
                return;
            }
            if (Relation==null || Relation.Name.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请选择监护人关系");
                return;
            }
            
            if (DateTime.Now > DateTime.AddYears(18))
            {
                ShowAlert(false, "超龄", "您已年满18岁，不允许办卡");
                return;
            }
            else
            {
                base.Confirm();
            }
        }
        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                //todo 发卡机发卡
                if (!GetNewCardNo()) return;

                if (Level0 == null || Level1 == null || Level2 == null)
                {
                    ShowAlert(false, "建档发卡", "请选择住址");
                    return;
                }
                if (Level3 == null)
                {
                    Level3 = new AddressItem();
                    Level3.Name = "";
                }
                IdCardModel.Address = Level0.Name + Level1.Name + Level2.Name + Level3.Name ;

                CreateModel.Req病人建档发卡 = new BJArea.Base.req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = "2",
                    name = Name.Trim(),
                    sex = IsBoy ? Sex.男.ToString() : Sex.女.ToString(),
                    birthday = DateTime.ToString("yyyy-MM-dd"),
                    idNo = "",
                    idType = "1", //测试必传
                    nation = Nation.Code,
                    address = IdCardModel.Address,
                    phone = CreateModel.Phone,
#pragma warning disable 612
                    guardianName = Guardian,
                    guardianNo = IdCardModel.IdCardNo,
                    school = null,
#pragma warning restore 612
                    pwd = "123456",
                    tradeMode = "CA",
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    relation = Relation.Code,
                    religion=Religion.Code,
                    education=Education.Code,
                    nationality="中国",
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerBJ.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    lp.ChangeText("正在发卡，请及时取卡。");
                    WriteCard();
                    PrintCard();
                    //PrintModel.SetPrintInfo(true, "建档发卡成功", $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                    //    ConfigurationManager.GetValue("Printer:Receipt"), null);
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        TipImage = "提示_发卡"
                    });



                    PlaySound(SoundMapping.取走卡片);

                    ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }
        protected override bool GetNewCardNo()
        {
            try
            {
                if (ConfigBJ.M1Local)
                {
                    CardModel.CardNo = DateTimeCore.Now.ToString("MMddhh24mmss");
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
        protected void WriteCard()
        {
            return;//北京暂不写卡
            M1DispenserRW.RfCardDispenser = _rfCardDispenser;
            var now = DateTimeCore.Now.ToString("yyyyMMdd");
            var eIdType = EnumM1IdType.身份证;
            if (CreateModel.CreateType == CreateType.儿童)
            {
                eIdType = EnumM1IdType.监护人身份证;
            }
            try
            {
                M1DispenserRW.WriteCommChunk0(CreateModel.Res病人建档发卡.data.patientCard, FrameworkConst.HospitalAreaCode, EnumM1Valid.启用);
                M1DispenserRW.WriteCommChunk1(now, "99991231", now);
                M1DispenserRW.WriteCommChunk2(CreateModel.Res病人建档发卡.data.platformId);
                M1DispenserRW.WritePatientChunk0(Name.Trim(), IdCardModel.Sex.ToString());
                M1DispenserRW.WritePatientChunk1(eIdType, IdCardModel.IdCardNo, Phone);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[建档发卡]写卡失败：{ex.Message + ex.StackTrace}");
            }
        }
        private void PrintCard()
        {
            if (ConfigBJ.M1Local)
            {
                return;
            }
            List<ZbrPrintTextItem> PrintText = new List<ZbrPrintTextItem>();
            ZbrPrintTextItem itemName = new ZbrPrintTextItem()
            {
                X = 160,
                Y = 55,
                Text = Name.Trim()
            };
            ZbrPrintTextItem itemNo = new ZbrPrintTextItem()
            {
                X = 550,
                Y = 55,
                FontSize = 11,
                Text = CreateModel.Res病人建档发卡.data.patientCard
            };
            PrintText.Add(itemName);
            PrintText.Add(itemNo);

            List<ZbrPrintCodeItem> PrintCode = new List<ZbrPrintCodeItem>()
                                        {
                                            new ZbrPrintCodeItem()
                                        };

            _rfCardDispenser.PrintContent(PrintText, PrintCode);
        }

        public void ModifyNameCmd(string txtId)
        {
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => {
                    if (!string.IsNullOrEmpty(p))
                    {
                        if (txtId == "监护人")
                        {
                            Guardian = p;
                        }
                        else
                        {
                            Name = p;
                        }
                    }
                },
                KeyAction = p =>
                {
                    StartTimer();
                    if (p == KeyType.CloseKey)
                        ShowMask(false);
                }
            }, 0.1, pt => { ShowMask(false); });
        }
        #region Binding
        private string _guardian;
        private bool _showUpdatePhone;
        private string _buttonContent;
        private string _phone;
        private string _tips;
        private string _newPhone;
        private bool _isAuth;

        public string Guardian
        {
            get { return _guardian; }
            set
            {
                _guardian = value;
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
        #endregion Binding
        #region 下拉框绑定数据
        //
        private IReadOnlyList<RelationItem> _relationL;
        public IReadOnlyList<RelationItem> RelationL
        {
            get { return _relationL; }
            set
            {
                _relationL = value;
                OnPropertyChanged();
            }
        }
        private RelationItem _relation;
        public RelationItem Relation
        {
            get { return _relation; }
            set
            {
                _relation = value;
                OnPropertyChanged();
            }
        }
        private IReadOnlyList<ReligionItem> _religionL;
        public IReadOnlyList<ReligionItem> ReligionL
        {
            get { return _religionL; }
            set
            {
                _religionL = value;
                OnPropertyChanged();
            }
        }
        private ReligionItem _religion;
        public ReligionItem Religion
        {
            get { return _religion; }
            set
            {
                _religion = value;
                OnPropertyChanged();
            }
        }

        private IReadOnlyList<NationItem> _nationL;
        public IReadOnlyList<NationItem> NationL
        {
            get { return _nationL; }
            set
            {
                _nationL = value;
                OnPropertyChanged();
            }
        }
        private NationItem _nation;
        public NationItem Nation
        {
            get { return _nation; }
            set
            {
                _nation = value;
                OnPropertyChanged();
            }
        }
        private IReadOnlyList<EducationItem> _educationL;
        public IReadOnlyList<EducationItem> EducationL
        {
            get { return _educationL; }
            set
            {
                _educationL = value;
                OnPropertyChanged();
            }
        }
        private EducationItem _education;
        public EducationItem Education
        {
            get { return _education; }
            set
            {
                _education = value;
                OnPropertyChanged();
            }
        }

        #region 地址相关

        private bool _showChangeAddress;
        public bool ShowChangeAddress
        {
            get => _showChangeAddress;
            set { _showChangeAddress = value; OnPropertyChanged(); }
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
        public DelegateCommand ModifyAddressCommand { get; set; }
        public virtual void ModifyAddressCmd()
        {
            // Address = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { if (!string.IsNullOrEmpty(p)) Address = p; },
                KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }
            }, 0.1, pt => { ShowMask(false); });
        }
        #endregion
        #endregion

        public override void ModifyNameCmd()
        {
            var t = Name;
            Name = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { Name = p; },
                KeyAction = p =>
                {
                    StartTimer();
                    if (p == KeyType.CloseKey)
                        ShowMask(false);
                },
                 Text = t,
            }, 0.1, pt => { ShowMask(false); });
        }
    }
}
