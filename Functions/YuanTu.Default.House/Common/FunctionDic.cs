using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.House.Device.BloodPressure;
using YuanTu.Default.House.Device.Ecg;
using YuanTu.Default.House.Device.Fat;
using YuanTu.Default.House.Device.HeightWeight;
using YuanTu.Default.House.Device.SpO2;
using YuanTu.Default.House.Device.Temperature;

namespace YuanTu.Default.House.Common
{
    public class FunctionDic
    {
        private FunctionDic()
        {
            Init();
        }

        public static readonly FunctionDic Instance = new FunctionDic();
        public Dictionary<IModel, string> ModelDic = new Dictionary<IModel, string>();
        public Dictionary<string, string> RouteDic = new Dictionary<string, string>();

        private void Init()
        {
            var heightWeightModel = ServiceLocator.Current.GetInstance<IHeightWeightModel>();
            var spO2Model = ServiceLocator.Current.GetInstance<ISpO2Model>();
            var bloodPressureModel = ServiceLocator.Current.GetInstance<IBloodPressureModel>();
            var fatModel = ServiceLocator.Current.GetInstance<IFatModel>();
            var temperatureModel = ServiceLocator.Current.GetInstance<ITemperatureModel>();
            var ecgModel = ServiceLocator.Current.GetInstance<IEcgModel>();
            ModelDic.Add(heightWeightModel, "身高体重");
            ModelDic.Add(fatModel, "体脂");
            ModelDic.Add(bloodPressureModel, "血压");
            ModelDic.Add(spO2Model, "血氧");
            ModelDic.Add(temperatureModel, "心电");
            ModelDic.Add(ecgModel, "血氧");

            RouteDic.Add("身高体重", AInner.Health.HeightWeight);
            RouteDic.Add("体脂", AInner.Health.Fat);
            RouteDic.Add("血压", AInner.Health.BloodPressure);
            RouteDic.Add("血氧", AInner.Health.SpO2);
            RouteDic.Add("体温", AInner.Health.Temperature);
            RouteDic.Add("心电", AInner.Health.Ecg);
        }
    }
}