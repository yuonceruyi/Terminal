using System;
using System.Windows.Forms;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Default.Tablet.Component.Cashier.Models;

namespace YuanTu.Default.Tablet.Component.Cashier.ViewModels
{
    internal class PrintViewModel : ViewModelBase
    {
        #region Overrides of ViewModelBase

        public override bool DisablePreviewButton { get; set; } = true;

        #endregion Overrides of ViewModelBase

        public PrintViewModel()
        {
            TimeOut = 20;
            ConfirmCommand = new DelegateCommand(Confirm);
            CopyCommand  = new DelegateCommand(Copy);
        }

        #region DataBindings

        public string Hint { get; set; } = "友好提醒";
        public override string Title => "打印";

        protected bool _success;

        public bool Success
        {
            get { return _success; }
            set
            {
                _success = value;
                OnPropertyChanged();
            }
        }

        private Uri _source;

        public Uri Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        protected string _typeMsg;

        public string TypeMsg
        {
            get { return _typeMsg; }
            set
            {
                _typeMsg = value;
                OnPropertyChanged();
            }
        }

        protected string _tipMsg;

        public string TipMsg
        {
            get { return _tipMsg; }
            set
            {
                _tipMsg = value;
                OnPropertyChanged();
            }
        }

        #endregion DataBindings

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public ICashierModel Cashier { get; set; }

        public DelegateCommand ConfirmCommand { get; set; }
        public DelegateCommand CopyCommand { get; set; }



        public override void OnEntered(NavigationContext navigationContext)
        {
            // 阻止导航栏点击
            GetInstance<INavigationModel>().PreventClick = true;

            Success = PrintModel.Success;
            TypeMsg = PrintModel.PrintInfo.TypeMsg;
            TipMsg = PrintModel.PrintInfo.TipMsg;
#if DEBUG
            if (!Success)
            {
                TipMsg = $"{PrintModel.PrintInfo.TipMsg}\r\n调试:{PrintModel.PrintInfo.DebugInfo}";
            }
#endif
            var resource = ResourceEngine;

            Source = resource.GetImageResourceUri(PrintModel.PrintInfo.TipImage ?? (Success ? "提示_正确" : "提示_感叹号"));

            if (PrintModel.NeedPrint)
            {
                PlaySound(SoundMapping.取走卡片及凭条);
                PrintManager.Print();
            }
            else
            {
                PlaySound(SoundMapping.请取走卡片);
            }
        }

        protected virtual void Confirm()
        {
            Navigate(A.Home);
        }

        private void Copy()
        {
            Clipboard.SetDataObject(Cashier.OutTradeNo);
            ShowAlert(true, "复制凭证号","复制成功");
        }
    }
}