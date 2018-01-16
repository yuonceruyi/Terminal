using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.QueueSignIn
{
    public interface IQueueSiginInModel : IModel
    {
        RegisterInfo[] RegisterInfos { get; set; }
        RegisterInfo SelectRegisterInfo { get; set; }
        SignType SignType { get; set; }
    }

    public class QueueSiginInModel : ModelBase, IQueueSiginInModel
    {
        public RegisterInfo[] RegisterInfos { get; set; }
        public RegisterInfo SelectRegisterInfo { get; set; }
        public SignType SignType { get; set; }
    }

    public enum SignType
    {
        回诊 = 1,
        过号 = 2,

    }
}
