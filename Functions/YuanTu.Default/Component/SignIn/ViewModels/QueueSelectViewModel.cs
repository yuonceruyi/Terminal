using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.SignIn.ViewModels
{
    public class QueueSelectViewModel : ViewModelBase
    {
        public override string Title => "选择签到排队队列";        
        [Dependency]
        public ICardModel CardModel { get; set; }
      
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("选择签到队列");

        }

        protected virtual void Confirm(Info i)
        {
           
        }

        #region Binding
        private ObservableCollection<Info> _data;

        public ObservableCollection<Info> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }
        #endregion Binding
    }
}
