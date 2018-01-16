using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.BJArea.QueueSignIn;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts;

namespace YuanTu.BJJingDuETYY.Component.SignIn.ViewModels
{
    public class QueueSelectViewModel : ViewModelBase
    {
        public override string Title => "选择签到排队队列";        
        [Dependency]
        public ICardModel CardModel { get; set; }
        [Dependency]
        public IQueues Queues { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("选择签到队列");

            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list2 = Queues.list.Select(p => new Info
            {
                Title = p.queueName,//队列
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);
        }

        protected virtual void Confirm(Info i)
        {
            var quene = i.Tag.As<ResQueryQueueByDevice.Data>();

            var TakeNo = SignInService.TakeNo(CardModel.CardNo, CardModel.CardType, quene.queueCode);
            if (TakeNo == null)
            {
                ShowAlert(false, "签到", "签到失败");
            }
            else
            {
                ShowAlert(true, "签到成功", TakeNo.username + "\n" + TakeNo.orderNoTag + "号");
            }
            Navigate(A.Home);
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
