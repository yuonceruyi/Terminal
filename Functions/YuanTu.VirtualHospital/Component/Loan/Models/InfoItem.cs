using Prism.Mvvm;

namespace YuanTu.VirtualHospital.Component.Loan.Models
{
    public class InfoItem : BindableBase
    {
        public InfoItem()
        {

        }

        public InfoItem(string title, string value)
        {
            _title = title;
            _value = value;
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
}