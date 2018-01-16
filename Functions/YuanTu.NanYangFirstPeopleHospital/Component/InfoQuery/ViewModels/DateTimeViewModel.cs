using Prism.Regions;
using YuanTu.Consts;

namespace YuanTu.NanYangFirstPeopleHospital.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            var today = DateTimeCore.Today;

            DateTimeStartStartDate = today.AddYears(-10);
            DateTimeStartEndDate = today;
            DateTimeEndStartDate = today.AddYears(-10);
            DateTimeEndEndDate = today;

            DateTimeStart = today.AddDays(-2);
            DateTimeEnd = today;
        }
    }
}