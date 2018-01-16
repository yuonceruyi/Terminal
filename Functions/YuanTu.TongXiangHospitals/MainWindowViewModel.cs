namespace YuanTu.TongXiangHospitals
{
    public class MainWindowViewModel : Default.MainWindowViewModel
    {
        /// <summary>
        /// 默认隐藏导航栏
        /// </summary>
        public override bool ShowNavigating => false;

        /// <summary>
        /// 默认显示双击背景border
        /// </summary>
        public override bool ShowBack => true;
    }
}