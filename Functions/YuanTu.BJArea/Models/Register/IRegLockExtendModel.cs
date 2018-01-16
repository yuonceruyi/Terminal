using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.BJArea.Models.Register
{
    public interface IRegLockExtendModel : IModel
    {
        /// <summary>
        /// 挂号序号 C可空 预约时必传挂号可空	
        /// </summary>
        string appoNo { get; set; }

        /// <summary>
        /// 可空 默认1 HIS锁号2 平台锁号
        /// </summary>
        string version { get; set; }

        /// <summary>
        /// 锁号方式N不可空1.预约，2.挂号 
        /// </summary>
        string regMode { get; set; }

        /// <summary>
        /// 患者手机号N非空
        /// </summary>
        string phone { get; set; }

        /// <summary>
        /// 当前患者选择是否支付
        /// </summary>
        bool isCharge { get; set; }

    }

    public class RegLockExtendModel : ModelBase, IRegLockExtendModel
    {
        public string appoNo { get; set; }
        public string version { get; set; } = "0";
        public string regMode { get; set; }
        public string phone { get; set; }
        public bool isCharge { get; set; }
    }

    public interface IRegUnLockExtendModel : IModel
    {
        /// <summary>
        /// 可空 默认1 HIS锁号2 平台锁号
        /// </summary>
        string version { get; set; }

    }

    public class RegUnLockExtendModel : ModelBase, IRegUnLockExtendModel
    {
        public string version { get; set; } = "0";
    }
}
