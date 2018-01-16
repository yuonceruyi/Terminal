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
using YuanTu.QDArea.Card;
using YuanTu.QDArea;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Core.Log;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Component.Auth.Dialog.Views;

namespace YuanTu.QDKouQiangYY.Component.Auth.ViewModels
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
        }
        public ICommand UpdateCommand { get; set; }
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Phone = null;
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
                    cash = null,
                    accountNo = null,
                    patientType = null,                    
                    setupType = ((int)CreateModel.CreateType).ToString()
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
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
                if (ConfigQD.M1Local)
                {
                    CardModel.CardNo = DateTimeCore.Now.ToString("MMddhh24:mm");
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
            M1DispenserRW.RfCardDispenser = _rfCardDispenser;
            var now = DateTimeCore.Now.ToString("yyyyMMdd");
            var eIdType = EnumM1IdType.身份证;
            if (CreateModel.CreateType== CreateType.儿童)
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
            if (ConfigQD.M1Local)
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
        #endregion Binding
    }
}
