using System;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.TaiZhouCentralHospital.Component.Register.Models
{
    public interface ISiRegHelpModel : IModel
    {
        res预约挂号 ResAppoint { get; set; }
        Func<Result> CancelAppoint { get; set; }
    }

    public class SiRegHelpModel : ModelBase, ISiRegHelpModel
    {
        public res预约挂号 ResAppoint { get; set; }
        public Func<Result> CancelAppoint { get; set; }
    }
}