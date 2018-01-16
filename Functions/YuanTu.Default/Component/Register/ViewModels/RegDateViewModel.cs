using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.Register.ViewModels
{
    public class RegDateViewModel : ViewModelBase
    {
        public override string Title => "请触摸下方卡片选择预约日期";
        protected readonly string[] Week = { "日", "一", "二", "三", "四", "五", "六" };
        protected virtual int AppointingDays { get; set; } = 7;
        protected int AppointingStartOffset { get; set; } = 1;
        protected string DayOfWeek(DateTime date)
        {
            return "星期" + Week[Convert.ToInt32(date.DayOfWeek)];
        }

        [Dependency]
        public IRegDateModel RegDateModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = new InfoMore[AppointingDays];
          
            for (var i = 0; i < AppointingDays; i++)
            {
                var date = DateTimeCore.Today.AddDays(i + AppointingStartOffset);
                list.SetValue(new InfoMore
                {
                    Title = date.ToString("yyyy-MM-dd"),
                    SubTitle = DayOfWeek(date),
                    Amount = null,
                    ConfirmCommand = confirmCommand,
                    Tag = i
                }, i);
            }
            Data = new ObservableCollection<InfoMore>(list);
            
            PlaySound(SoundMapping.选择预约日期);
        }

        protected virtual void Confirm(Info i)
        {
            RegDateModel.RegDate = i.Title;
            ChangeNavigationContent(i.Title);
            Next();
        }

        #region Binding

        private ObservableCollection<InfoMore> _data;

        public ObservableCollection<InfoMore> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}