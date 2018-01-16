using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;

namespace YuanTu.Default.House.Component.ViewModels
{
    public class PrintViewModel : ViewModelBase
    {
        private Uri _source;

        private bool _success;
        private string _tipMsg;
        private string _typeMsg;
        private bool _doCommand;
        #region Overrides of ViewModelBase

        public override bool DisablePreviewButton { get; set; } = true;

        #endregion

        public PrintViewModel()
        {
            TimeOut = 20;
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public string Hint { get; set; } = "温馨提示";
        public override string Title => "打印";

        public bool Success
        {
            get { return _success; }
            set
            {
                _success = value;
                OnPropertyChanged();
            }
        }

        public Uri Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        public string TypeMsg
        {
            get { return _typeMsg; }
            set
            {
                _typeMsg = value;
                OnPropertyChanged();
            }
        }

        public string TipMsg
        {
            get { return _tipMsg; }
            set
            {
                _tipMsg = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        public ICommand ConfirmCommand { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            // 阻止导航栏点击
            GetInstance<INavigationModel>().PreventClick = true;

            Success = PrintModel.Success;
            TypeMsg = PrintModel.PrintInfo.TypeMsg;
            TipMsg = PrintModel.PrintInfo.TipMsg;
#if DEBUG
            if (!Success)
            {
                TipMsg = PrintModel.PrintInfo.TipMsg + "\r\n调试:" + PrintModel.PrintInfo.DebugInfo;
            }
#endif
            var resource =ResourceEngine;
         
            Source = resource.GetImageResourceUri(PrintModel.PrintInfo.TipImage ?? (Success ? "提示_正确" : "提示_感叹号"));
         
            if (PrintModel.NeedPrint)
            {
                
                
                PlaySound(SoundMapping.取走卡片及凭条);
                PrintManager.Print();
            }
            _doCommand = false;


        }

        protected virtual void Confirm()
        {
            if (_doCommand)
                return;
            _doCommand = !_doCommand;
            Next();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            return base.OnLeaving(navigationContext);
        }
    }
}