using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Register.Models;

namespace YuanTu.Default.Component.Register.ViewModels
{
    public class RegAmPmViewModel : ViewModelBase
    {
        public override string Title => "选择就诊场次";

        [Dependency]
        public IRegDateModel RegDateModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            Data = new ObservableCollection<InfoType>(
                AmPmConfig.GetInfoTypes(
                    ConfigurationManager,
                    ResourceEngine,
                    null,
                    new DelegateCommand<Info>(Confirm)
                )
            );
        }

        protected virtual void Confirm(Info i)
        {
            var cfg = i.Tag as AmPmConfig;
            var date = DateTimeCore.Today.ToString("yyyy-MM-dd");
            var start = DateTime.ParseExact($"{date} {cfg.StartTime}", "yyyy-MM-dd HH:mm", null);
            var end = DateTime.ParseExact($"{date} {cfg.EndTime}", "yyyy-MM-dd HH:mm", null);
            if (DateTimeCore.Now < start || DateTimeCore.Now > end)
            {
                ShowAlert(false, "挂号限制", $"该场次仅能在{cfg.StartTime}-{cfg.EndTime}时间范围内操作");
                return;
            }
            RegDateModel.AmPm = i.Title.SafeToAmPmEnum();
            ChangeNavigationContent(i.Title);
            Next();
        }

        #region Binding

        private ObservableCollection<InfoType> _data;

        public ObservableCollection<InfoType> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}