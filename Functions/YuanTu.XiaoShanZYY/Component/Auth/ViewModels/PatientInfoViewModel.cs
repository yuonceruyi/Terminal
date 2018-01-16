using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Models;
using YuanTu.Devices.CardReader;
using YuanTu.XiaoShanZYY.Component.Auth.Models;

namespace YuanTu.XiaoShanZYY.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser, IMagCardDispenser[] magCardDispenser)
            : base(rfCardDispenser)
        {
        }

        [Dependency]
        public IAuthModel Auth { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(string.Empty);
            TopBottom.InfoItems = null;

            IsAuth = true;
            ShowUpdatePhone = false;
            CanUpdatePhone = false;

            var info = Auth.人员信息;
            Name = info.病人姓名;
            Sex = info.病人性别.SafeToSex().ToString();
            Birth = info.出生日期;
            Phone = info.联系电话;
            IdNo = info.身份证号.Mask(14, 3);
            Remain = Auth.Info.Remain.In元();
        }

        public override void Confirm()
        {
            var info = Auth.人员信息;
            ChangeNavigationContent($"{info.病人姓名}");

            var resource = ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = info.病人姓名,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
            });

            Next();
        }

        private string _remain;

        public string Remain
        {
            get { return _remain; }
            set
            {
                _remain = value;
                OnPropertyChanged();
            }
        }
    }
}