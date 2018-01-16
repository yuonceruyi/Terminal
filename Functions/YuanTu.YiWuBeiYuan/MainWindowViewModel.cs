namespace YuanTu.YiWuBeiYuan.Clinic
{
    public class MainWindowViewModel: YuanTu.Default.MainWindowViewModel
    {

        public override bool ShowNavigating
        {
            get { return false; }
            set
            {
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowBack));
            }
        }

        public override bool ShowBack => true;
    }
}
