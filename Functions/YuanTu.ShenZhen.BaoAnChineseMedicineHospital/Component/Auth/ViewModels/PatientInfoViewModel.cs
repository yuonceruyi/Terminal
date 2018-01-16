using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Models;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.ShenZhenArea.Insurance;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Services;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : YuanTu.Default.Component.Auth.ViewModels.PatientInfoViewModel
    {

        [Dependency]
        public IYBService YBServices { get; set; }


        [Dependency]
        public IYBModel YBModel { get; set; }

        private static readonly byte[] _keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }; 

        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "Act_F3_RF");
            //TimeOut = 60;
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
            TopBottom.InfoItems = null;

            if (ChoiceModel.Business == Business.建档)
            {
                IsSheBao = false;
                IsShowBddz = false;
                switch (CardModel.CardType)
                {
                    case CardType.NoCard:
                    case CardType.居民健康卡:
                    case CardType.扫码:
                    case CardType.医保卡:
                    case CardType.刷脸:
                    case CardType.省医保卡:
                    case CardType.市医保卡:
                    case CardType.门诊号:
                    case CardType.住院号:
                        break;
                    case CardType.身份证:
                    case CardType.社保卡:
                        Name = IdCardModel.Name;
                        Sex = IdCardModel.Sex.ToString();
                        Birth = IdCardModel.Birthday.ToString("yyyy-MM-dd");
                        Phone = null;
                        IdNo = IdCardModel.IdCardNo.Mask(14, 3);
                        IsAuth = false;
                        ShowUpdatePhone = false;
                        CanUpdatePhone = true;
                        break;
                    default:
                        break;
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
                Balance = patientInfo.accBalance.In元();

                IsSheBao = CardModel.CardType == CardType.社保卡;
                Brlx = YBModel?.参保类型 ?? Cblx.不参加;

                IsShowBddz = IsSheBao && YBModel.参保类型 != Cblx.基本医疗保险一档;
                if (IsShowBddz)
                {
                    if (YBModel?.医保个人基本信息?.BDSK.ToLower() == "hbz90")
                    {
                        Bddz = "绑定我院本部就医点（急诊科）";
                    }
                    else
                    {
                        if (ConfigBaoAnChineseMedicineHospital.SheKangBianMa.ContainsKey(YBModel?.医保个人基本信息?.BDSK.ToLower()))
                        {
                            Bddz = "绑定我院" + ConfigBaoAnChineseMedicineHospital.SheKangBianMa[YBModel?.医保个人基本信息?.BDSK.ToLower()];
                        }
                        else
                        {
                            Bddz = "未绑定我院";
                        }
                    }
                }
                if (IsSheBao)
                {
                    if (YBModel.参保类型 == Cblx.基本医疗保险一档 || Convert.ToDecimal(YBModel?.医保个人基本信息?.ACCOUNT ?? "0") > 0)
                    {
                        BalanceYB = YBModel?.医保个人基本信息?.ACCOUNT + "元";
                        BalanceYBTitle = "个人账户：";
                    }
                    else
                    {
                        BalanceYB = YBModel?.医保个人基本信息?.NDBGKY + "元";
                        BalanceYBTitle = "门诊包干可用：";
                    }
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
            if (ChoiceModel.Business == Business.建档)
            {
                switch (CreateModel.CreateType)
                {
                    case CreateType.成人:
                        CreatePatient();
                        break;
                    case CreateType.儿童:
                        Navigate(A.CK.InfoEx);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }
            var patientInfo = PatientModel.当前病人信息;
            if(CardModel.CardType== CardType.社保卡)
            {
                ChangeNavigationContent($"{patientInfo.name}({YBModel.参保类型})\r\n余额{patientInfo.accBalance.In元()}\r\n{BalanceYBTitle}{BalanceYB}");
            }
            else
            {
                ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
            }

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

        protected override void CreatePatient()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在准备就诊卡，请稍候...");
                if (CardModel.CardType == CardType.身份证)
                {
                    if (!GetNewCardNo()) return;
                }
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = CardModel.CardNo,
                    cardType = ((int)(CardModel.CardType == CardType.身份证 ? CardType.就诊卡 : CardModel.CardType)).ToString(),
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
                    patientType = CardModel.CardType == CardType.身份证 ? "01" : YBServices.获取HIS需要的患者类型(YBModel.参保类型), //todo 取社保卡的病人类型
                    setupType = ((int)CreateModel.CreateType).ToString()
                };
                lp.ChangeText("正在建档，请稍候...");
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡?.success ?? false)
                {
                    if (CardModel.CardType == CardType.身份证)
                    {
                        lp.ChangeText("正在发卡，请及时取卡。");
                        if (!FrameworkConst.DoubleClick)
                            PrintCard();
                    }
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
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }
        protected override void PrintCard()
        {
            if (CardModel.CardType == CardType.身份证)
            {
                _rfCardDispenser.MoveCardOut();
                ShowAlert(true, "友情提示", "您当前办的是自费的就诊卡；如您有深圳市社保卡，请到人工窗口去将其与就诊卡绑定关联，以保证在自助机上能正常使用社保卡记账缴费", 20);
            }
        }
        protected override bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick && FrameworkConst.VirtualHardWare)
                    return Invoke(() =>
                    {
                        var ret = InputTextView.ShowDialogView("输入物理卡号");
                        if (ret.IsSuccess)
                        {
                            CardModel.CardNo = ret.Value;
                            return true;
                        }
                        return false;
                    });
                Logger.Main.Info($"[建档发卡]进入到获取卡号逻辑");
                if (!_rfCardDispenser.Connect().IsSuccess)
                {
                    ReportService.发卡器离线(null, ErrorSolution.发卡器离线);
                    ShowAlert(false, "建档发卡", "发卡机连接失败");
                    return false;
                }
                Logger.Main.Info($"[建档发卡]发卡器判断是否含有卡");
                if (!_rfCardDispenser.EnterCard().IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡机获取序列号失败");
                    return false;
                }
                if (!_rfCardDispenser.MfVerifyPassword(0, true, _keyA).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡密码验证失败");
                    return false;
                }
                byte[] data1;//序列号
                if (!_rfCardDispenser.MfReadSector(0, 0, out data1).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 序列号读取失败");
                    return false;
                }
                if (!_rfCardDispenser.MfVerifyPassword(2, true, _keyA).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 密码验证失败");
                    return false;
                }
                byte[] data2;//卡号
                if (!_rfCardDispenser.MfReadSector(2, 2, out data2).IsSuccess)
                {
                    ShowAlert(false, "建档发卡", "发卡器新卡 卡号读取失败");
                    return false;
                }

                byte[] bCardNo = new byte[16];
                byte[] bSerialNo = new byte[4];
                Array.Copy(data2, bCardNo, 16);
                Array.Copy(data1, bSerialNo, 4);
                Array.Reverse(bSerialNo);
                var serialNo = bSerialNo.ByteToString();
                var cardNo = bCardNo.ByteToString();

                if (string.IsNullOrEmpty(cardNo) || string.IsNullOrEmpty(serialNo))
                {
                    ShowAlert(false, "建档发卡", "获取数据失败失败");
                    return false;
                }
                if (cardNo.Contains("\0"))
                {
                    cardNo = cardNo.Substring(0, cardNo.IndexOf("\0"));
                }
                Logger.Main.Info($"[建档发卡]发卡器获取序列号完毕Default:{serialNo}");
                Logger.Main.Info($"[建档发卡]发卡器获取卡号完毕Default:{ cardNo}");
                if (cardNo.Length < 12)
                {
                    Logger.Main.Info($"[建档发卡]发卡器获取卡号完毕,卡号少于12位，读取错误Default:{ cardNo}");
                }
                else
                {
                    cardNo = cardNo.Substring(0, 12);
                }
                CardModel.CardNo = cardNo;
                CardModel.ExternalCardInfo = serialNo;
                return true;
            }
            catch (Exception ex)
            {
                ShowAlert(false, "建档发卡", "发卡机读卡失败");
                Logger.Main.Error($"[建档发卡]{ex.Message + ex.StackTrace}");
                return false;
            }
        }


        #region bing
        private string _balance;
        private string _balanceYB;
        private string _balanceYBTitle;
        private bool _isSheBao;
        private Cblx _brlx;
        private string _bddz;  //绑定地址
        private bool _isShowBddz;


        public string Balance { get => _balance; set  { _balance = value; OnPropertyChanged(); } }
        public string BalanceYB { get => _balanceYB; set { _balanceYB = value; OnPropertyChanged(); }
}
        public bool IsSheBao { get => _isSheBao; set { _isSheBao = value;  OnPropertyChanged(); } }

        public Cblx Brlx { get => _brlx; set { _brlx = value; OnPropertyChanged(); } }

        /// <summary>
        /// 绑定地址，主要是是否本院
        /// </summary>
        public string Bddz { get => _bddz; set { _bddz = value; OnPropertyChanged(); } }

        public bool IsShowBddz { get => _isShowBddz; set { _isShowBddz = value; OnPropertyChanged(); } }

        public string BalanceYBTitle { get => _balanceYBTitle; set { _balanceYBTitle = value; OnPropertyChanged(); } }

        

        #endregion

    }
}
