using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn.ViewModels
{
    public class SignInRegisterInfoViewModel : ViewModelBase
    {
        public override string Title => "签到挂号记录选择";


        [Dependency]
        public IQueueSiginInModel QueueSiginInModel { get; set; }

        [Dependency]
        public IQueueSignInService QueueSignInService { get; set; }

        private ObservableCollection<InfoMore> _data;

        public ObservableCollection<InfoMore> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = QueueSiginInModel.RegisterInfos.Select(p => new InfoMore
            {
                Title = p.regTime,
                Tag = p,
                Extends=p.smallDeptName+"\n"+p.doctorName,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoMore>(list);
        }

        protected virtual void Confirm(Info i)
        {
            QueueSiginInModel.SelectRegisterInfo = i.Tag.As<RegisterInfo>();
            var ret = QueueSignInService.QueueSignIn(QueueSiginInModel.SignType,
                QueueSiginInModel.SelectRegisterInfo);
            if (ret.IsSuccess)
            {
                ShowAlert(true, "签到", "签到成功");
            }
            else
            {
                ShowAlert(false, "签到失败", ret.Message);
            }
        }
    }
}
