using System;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Default.House.Component.Auth.Models;

namespace YuanTu.Default.House.Part.ViewModels
{
    public class NavigateBarViewModel : Default.Part.ViewModels.NavigateBarViewModel
    {
        private Uri _doneUri;
        private PatInfo _info;

        public NavigateBarViewModel()
        {
        }

        public NavigateBarViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public Uri DoneUri
        {
            get { return _doneUri; }
            set
            {
                _doneUri = value;
                OnPropertyChanged();
            }
        }

        public PatInfo Info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }

        protected override void ViewIsChanging(ViewChangingEvent eveEvent)
        {
            var nav = GetInstance<INavigationModel>();
            nav.JumpSourceAware = false;

            base.ViewIsChanging(eveEvent);

            var patientInfo = GetInstance<IHealthModel>()?.Res查询是否已建档?.data;
            Info = new PatInfo
            {
                Name = patientInfo?.name,
                Sex = patientInfo?.sex,
                Age = patientInfo?.age
            };
        }

        protected override void OnItemsChanged()
        {
            var items = Items;
            if (MainContext == AInner.Health_Context)
                while (items.Count > Startup.HealthDetectCount)
                    items.RemoveAt(Startup.HealthDetectCount);
            else if (MainContext == A.YuYue_Context || MainContext == A.XianChang_Context)
                items.RemoveAt(0);

            var res = ResourceEngine;
            foreach (var item in items)
                item.Tag = res.GetImageResourceUri(item.Icon ?? item.Title);

            for (var i = items.Count; i < MinimalItemCount; i++)
                items.Add(new NavigationItem());

            DoneUri = res.GetImageResourceUri("已测量");
        }

        protected override void OnGoBack(NavigationItem item)
        {
            //测量流程不清数据
            if (MainContext != AInner.Health_Context)
            {
                item.Content = null;
                item.HasFootprint = false;
            }

            if (item.IsAmbiguous)
                item.FormContext = null;
        }

        public class PatInfo
        {
            public string Name { get; set; }
            public string Sex { get; set; }
            public string Age { get; set; }
        }
    }
}