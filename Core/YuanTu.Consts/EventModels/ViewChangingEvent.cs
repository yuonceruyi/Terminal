using Prism.Events;

namespace YuanTu.Consts.EventModels
{
    public class ViewChangingEvent : PubSubEvent<ViewChangingEvent>
    {
        public string FromContext { get; set; }
        public string From { get; set; }
        public string ToContext { get; set; }
        public string To { get; set; }
    }
}