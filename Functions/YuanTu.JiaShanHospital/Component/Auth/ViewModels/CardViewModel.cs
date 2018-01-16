using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Core.Systems;
using YuanTu.Devices.CardReader;
using YuanTu.JiaShanHospital.Component.Auth.Dialog;

namespace YuanTu.JiaShanHospital.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {

        #region MyRegion

        private string _hospitalCardNo;
        private string _buttonContent;
        private string _numMaxLength;
        private bool _hospitalInputFocus;
        private string _siPassword;

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        public CardViewModel(IRFCpuCardReader[] rfCpuCardReader) : base(null, null)
        {
            ShowInputMaskCommand = new DelegateCommand(ShowInputMask);
            CancelHospitalCardNoCommand = new DelegateCommand(CancelHospitalCardNo);
            ConfirmHospitalCardNoCommand = new DelegateCommand(ConfirmHospitalCardNo);
        }
        private Visibility _showBarCodeCardAnimation;
        public Visibility ShowBarCodeCardAnimation
        {
            get { return _showBarCodeCardAnimation; }
            set { _showBarCodeCardAnimation = value; OnPropertyChanged(); }
        }
        private Uri _jiuZhenCard;
        public Uri JiuZhenCard
        {
            get { return _jiuZhenCard; }
            set
            {
                _jiuZhenCard = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Number> numbers;
        public ObservableCollection<Number> Numbers
        {
            get { return numbers; }
            set
            {
                numbers = value;
                OnPropertyChanged();
            }
        }

        public override void OnSet()
        {
            base.OnSet();
            Numbers = new ObservableCollection<Number>();
            if (ChoiceModel.Business == (Business)100)
            {
                ButtonContent = "手输回执单号";
                NumMaxLength = "12";
                JiuZhenCard = ResourceEngine.GetImageResourceUri("回执单");

                InitNumbers(11);
            }
            else
            {
                ButtonContent = "手输就诊卡号";
                NumMaxLength = "10";

                JiuZhenCard = ResourceEngine.GetImageResourceUri("卡_条码卡");
                InitNumbers(9);
            }


        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ShowBarCodeCardAnimation = Visibility.Visible;
            HospitalInputFocus = true;
            if (ChoiceModel.Business != (Business)100)
            {
                PlaySound(SoundMapping.请扫描就诊卡条形码);
            }
            else
            {
                PlaySound(SoundMapping.请扫描回执单条形码);
            }
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return base.OnLeaving(navigationContext);

        }

        private void InitNumbers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Numbers.Add(new Number());
            }
        }

        protected override void StartRead()
        {
        }


        #region[手动输入卡号]

        public string HospitalCardNo
        {
            get { return _hospitalCardNo; }
            set
            {
                _hospitalCardNo = value;
                Numbers.Clear();
                if (_hospitalCardNo != null)
                {
                    _hospitalCardNo.ForEach(p => Numbers.Add(new Number { Value = p.ToString() }));
                    if (ChoiceModel.Business == (Business)100)
                    {
                        InitNumbers(11 - Numbers.Count);
                    }
                    else
                    {
                        InitNumbers(10 - Numbers.Count);
                    }
                }
                OnPropertyChanged();
            }
        }

        public string NumMaxLength
        {
            get { return _numMaxLength; }
            set
            {
                _numMaxLength = value;
                OnPropertyChanged();
            }
        }

        public bool HospitalInputFocus
        {
            get { return _hospitalInputFocus; }
            set
            {
                _hospitalInputFocus = value;
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

        public Visibility ShowHospitalCardKeyboard => Visibility.Visible;
        //取消手动输入卡号
        public ICommand CancelHospitalCardNoCommand { get; set; }
        //确认输入的卡号
        public ICommand ConfirmHospitalCardNoCommand { get; set; }
        //弹出手输卡号框
        public ICommand ShowInputMaskCommand { get; set; }
        private void ShowInputMask()
        {
            HospitalCardNo = null;
            HospitalInputFocus = true;
            ShowMask(true, new HospitalCardDialog() { DataContext = this });
            HospitalInputFocus = true;
        }

        private void CancelHospitalCardNo()
        {
            ShowMask(false);
        }
        private void ConfirmHospitalCardNo()
        {
            CardModel.CardType = CardType.就诊卡;
            if (string.IsNullOrEmpty(HospitalCardNo))
            {
                return;
            }
            if (ChoiceModel.Business==(Business)100)
            {
                PrintDiagReport();
                HospitalCardNo = String.Empty;
                return;
            }
            OnGetInfo(HospitalCardNo);
            HospitalCardNo = String.Empty;
        }

        protected void PrintDiagReport()
        {
            Logger.Main.Info($"[确认打印] No=" + HospitalCardNo);
            var config = GetInstance<IConfigurationManager>();
            var path = config.GetValue("LisExePath");
            if (!File.Exists(path) || string.IsNullOrEmpty(path))
            {
                ShowAlert(false, "检验结果打印", "本机不支持打印检验报告，请到其它自助机打印。");
                Navigate(A.Home);
                return;
            }
            Process.Start(
                new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    FileName = Path.Combine(path),
                    CreateNoWindow = false,
                    Arguments = HospitalCardNo
                });
            //Navigate(A.Home);
        }
        #endregion

        #endregion

    }

    public class Number
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
