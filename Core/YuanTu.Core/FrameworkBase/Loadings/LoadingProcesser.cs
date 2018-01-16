using System.Windows;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Core.FrameworkBase.Loadings
{
    public class LoadingProcesser
    {
        private readonly IShellViewModel _shellViewModel;
        public LoadingProcesser(IShellViewModel shellViewModel)
        {
            _shellViewModel = shellViewModel;
        }

        public string CurrentContent => _shellViewModel.Busy?.IsBusy ?? false ? _shellViewModel.Busy?.BusyContent : null;

        public void ChangeText(string content, string extra = null)
        {
            if (_shellViewModel.Busy.IsBusy)
            {
                _shellViewModel.Busy.BusyMutiContent = null;
                _shellViewModel.Busy.BusyContent = content;
                _shellViewModel.Busy.ExtraMessage = extra;
            }
        }

        public void ChangeMutiText(FrameworkElement element, string extra = null)
        {
            if (_shellViewModel.Busy.IsBusy)
            {
                _shellViewModel.Busy.BusyContent = null;
                _shellViewModel.Busy.BusyMutiContent = element;
                _shellViewModel.Busy.ExtraMessage = extra;
            }
        }
    }
}
