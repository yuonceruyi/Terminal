using LiveCharts.Geared;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device.Ecg
{
    public interface IEcgModel : IModel
    {
        byte Diag { get; set; }
        decimal PR { get; set; }
        string 参考结果 { get; set; }

        GearedValues<double> Values { get; set; }
        double XMax { get; set; }
        double XMin { get; set; }
    }

    public class EcgModel : ModelBase, IEcgModel
    {
        private GearedValues<double> _values;

        private double _xMax = 300;

        private double _xMin;
        public byte Diag { get; set; }
        public decimal PR { get; set; }
        public string 参考结果 { get; set; }

        public GearedValues<double> Values
        {
            get => _values;
            set
            {
                _values = value;
                OnPropertyChanged();
            }
        }

        public double XMax
        {
            get => _xMax;
            set
            {
                _xMax = value;
                OnPropertyChanged();
            }
        }

        public double XMin
        {
            get => _xMin;
            set
            {
                _xMin = value;
                OnPropertyChanged();
            }
        }
    }
}