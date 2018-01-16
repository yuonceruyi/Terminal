namespace YuanTu.TongXiangFourthHospital.Component.Register.ViewModels
{
    public class DeptsViewModel:TongXiangHospitals.Component.Register.ViewModels.DeptsViewModel
    {
        protected override string GetScheduleViewModelName()
        {
            return typeof(ScheduleViewModel).FullName;
        }
    }
}
