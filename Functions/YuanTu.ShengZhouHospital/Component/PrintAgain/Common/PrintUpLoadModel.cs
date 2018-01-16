using System;

namespace YuanTu.ShengZhouHospital.Component.PrintAgain.Common
{
    public class PrintUpLoadModel
    {
        public Type Type { get; set; }
        public string JsonStr { get; set; }
    }

    public class PrintUpLoadImageModel : PrintUpLoadModel
    {
        public string ImageStr { get; set; }
    }
}
