using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.TakeNum.ViewModels
{
    public class QueryViewModel : ViewModelBase
    {
        private string _regNo;
        public override string Title => "输入取号密码";
        public string RegNo
        {
            get { return _regNo; }
            set
            {
                _regNo = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public ITakeNumModel TakeNumModel { get; set; }
        [Dependency]
        public IAppoRecordModel AppoRecordModel { get; set; }

        public QueryViewModel()
        {
            ConfirmCommand = new DelegateCommand(Do);
        }
        public override void OnSet()
        {
            base.OnSet();
           
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            RegNo = null;
        }

        public virtual void Do()
        {
            if (RegNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false,"取号密码","请输入取号密码！");
                return;
            }
            AppoRecordModel.RegNo = RegNo;
           QueryAppoInfo();
        }

        public virtual void QueryAppoInfo()
        {
           Next();
        }
    }
}