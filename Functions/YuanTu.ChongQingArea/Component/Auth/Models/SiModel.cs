using System;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.ChongQingArea.Component.Auth.Models
{
    public interface ISiModel : IModel
    {
        bool NeedCreate { get; set; }
        string 社保卡卡号 { get; set; }

        SiContext SiContext { get; set; }
        Res获取人员基本信息 Res获取人员基本信息 { get; set; }
        Func<Result> ConfirmSi { get; set; }
        bool UseSi { get; set; }
    }
    class SiModel:ModelBase,ISiModel
    {
        public bool NeedCreate { get; set; }
        public string 社保卡卡号 { get; set; }
        public SiContext SiContext { get; set; }
        public Res获取人员基本信息 Res获取人员基本信息 { get; set; }
        public Func<Result> ConfirmSi { get; set; }
        public bool UseSi { get; set; }
    }
}
