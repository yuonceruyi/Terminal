using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using YuanTu.Consts.EventModels;

namespace YuanTu.YuHangZYY.Models
{
    public class TopBottomModel:YuanTu.Core.Models.TopBottomModel
    {
        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public TopBottomModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(ViewHasChanged2);
        }

        private void ViewHasChanged2(ViewChangeEvent eveEvent)
        {
            var ishome = NavigationEngine.IsHome(eveEvent.To);
            if (ishome)
                Message = null;
        }
    }
}
