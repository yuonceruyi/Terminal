using Prism.Events;

namespace YuanTu.Consts.EventModels
{
    public class SystemInfoEvent : PubSubEvent<SystemInfoEvent>
    {
        public bool DisableHomeButton { get; set; }
        public bool DisablePreviewButton { get; set; }
        public bool HideClock { get; set; }
        public bool HideVersion { get; set; }
    }
}