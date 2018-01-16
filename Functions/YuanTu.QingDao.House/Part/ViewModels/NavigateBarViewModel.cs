using Prism.Events;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Core.Navigating;
using YuanTu.Default.House.Component.Auth.Models;

namespace YuanTu.QingDao.House.Part.ViewModels
{
    public class NavigateBarViewModel : Default.House.Part.ViewModels.NavigateBarViewModel
    {
        public NavigateBarViewModel()
        {
            
        }
        public NavigateBarViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        protected override void ViewIsChanging(ViewChangingEvent eveEvent)
        {
            base.ViewIsChanging(eveEvent);
            var patientInfo = GetInstance<IAuthModel>()?.当前就诊人信息;
            var healthInfo = GetInstance<IHealthModel>()?.Res查询是否已建档?.data;
            if (patientInfo != null)
                Info = new PatInfo
                {
                    Name = patientInfo?.patientName,
                    Sex = patientInfo?.sex == 1 ? "男" : "女",
                    Age = null
                };
            else if (healthInfo != null)
                Info = new PatInfo
                {
                    Name = healthInfo?.name,
                    Sex = healthInfo?.sex,
                    Age = healthInfo?.age
                };
        }
    }
}