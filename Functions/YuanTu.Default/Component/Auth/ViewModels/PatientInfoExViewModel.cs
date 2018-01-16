using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
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

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class PatientInfoExViewModel : ViewModelBase
    {
        protected IMagCardDispenser _magCardDispenser;
        protected IRFCardDispenser _rfCardDispenser;

        public PatientInfoExViewModel(IRFCardDispenser[] rfCardDispenser)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "ZBR_RF");
            ConfirmCommand = new DelegateCommand(Confirm);
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
        }

        public override string Title => "档案建立";

        public ICommand ConfirmCommand { get; set; }
        public ICommand ModifyNameCommand { get; set; }
        public virtual void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请输入就诊人姓名");
                return;
            }
            if (!IsBoy && !IsGirl)
            {
                ShowAlert(false, "温馨提示", "请选择就诊人性别");
                return;
            }
            if (DateTime.CompareTo(DateTime.Now) > 0)
            {
                ShowAlert(false, "温馨提示", "出生日期不能大于当前日期");
                return;
            }
            if (DateTime.Date.CompareTo(IdCardModel.Birthday.Date) == 0)
            {              
                ShowAlert(false, "温馨提示", "本人身份证信息，不能用监护人办卡");
                return;
            }
            CreatePatient();
        }


        public virtual void ModifyNameCmd()
        {
            Name = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { Name = p; },
                KeyAction = p =>
                {
                    StartTimer();
                    if (p == KeyType.CloseKey)
                        ShowMask(false);
                }
            }, 0.1, pt => { ShowMask(false); });
        }

        protected virtual bool GetNewCardNo()
        {
            try
            {
                if (FrameworkConst.DoubleClick)
                {
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
                        PrintCard();
                    //PrintModel.SetPrintInfo(true, "建档发卡成功", $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                    //    ConfigurationManager.GetValue("Printer:Receipt"), CreatePrintables(),null, "提示_凭条和发卡");
                    //PrintModel.SetPrintImage(ResourceEngine.GetImageResourceUri("提示_凭条和发卡"));
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "建档发卡成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = CreatePrintables(),
                        TipImage = "提示_凭条和发卡"
                    });

                    ChangeNavigationContent($"{Name}\r\n卡号{CardModel.CardNo}");
                    Next();
                }
                else
                {
                    ShowAlert(false, "建档发卡", "建档发卡失败", debugInfo: CreateModel.Res病人建档发卡?.msg);
                }
            });
        }
        protected virtual void PrintCard()
        {
            _rfCardDispenser.PrintContent(new List<ZbrPrintTextItem> { new ZbrPrintTextItem() },
                                        new List<ZbrPrintCodeItem> { new ZbrPrintCodeItem() });
        }
        protected virtual Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{Name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }



        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            ModifyNameCommand.Execute(null);
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return true;
        }

        #region Binding

        private string _name;
        private bool _isBoy;
        private string _hint = "就诊人信息";

        private DateTime _dateTime = DateTimeCore.Today;
        private bool _isGirl;

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

        public bool IsBoy
        {
            get { return _isBoy; }
            set
            {
                _isBoy = value;
                OnPropertyChanged();
            }
        }

        public bool IsGirl
        {
            get { return _isGirl; }
            set
            {
                _isGirl = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

        #region Ioc

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        #endregion Ioc
    }
}