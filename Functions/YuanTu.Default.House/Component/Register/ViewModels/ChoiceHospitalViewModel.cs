using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.House.Component.Register.ViewModels
{
    public class ChoiceHospitalViewModel : ViewModelBase
    {
        public ChoiceHospitalViewModel()
        {
            Command = new DelegateCommand<HospitalButtonInfo>(Confirm, info => info.IsEnabled);
        }

        public DelegateCommand<HospitalButtonInfo> Command { get; set; }

        public override string Title { get; } = "选择医院界面";

        public override void OnSet()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var values = config.GetValues("ChoiceHospitals");
            var buttons = values
                .Select(s => s.Path)
                .Where(p => config.GetValue($"{p}:Visabled") == "1")
                .Select(p => new HospitalButtonInfo
                {
                    Name = config.GetValue($"{p}:Name") ?? "未定义",
                    HospitalId = config.GetValue($"{p}:HospitalId"),
                    Order = config.GetValueInt($"{p}:Order"),
                    IsEnabled = config.GetValueInt($"{p}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"{p}:ImageName"))
                })
                .OrderBy(p => p.Order)
                .ToList();

            Data = buttons;
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
        }

        protected virtual void Confirm(HospitalButtonInfo param)
        {
            FrameworkConst.HospitalId = param.HospitalId;
            Next();
        }

        #region DataBinding


        private IReadOnlyCollection<HospitalButtonInfo> _data;
        public IReadOnlyCollection<HospitalButtonInfo> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public class HospitalButtonInfo
        {
            public string Name { get; set; }
            public string HospitalId { get; set; }
            public int Order { get; set; }
            public bool Visabled { get; set; }
            public bool IsEnabled { get; set; }
            public ImageSource ImageSource { get; set; }
        }
    }
}