using System;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.Auth
{
    public interface IIdCardModel : IModel
    {
        string Name { get; set; }
        Sex Sex { get; set; }
        string Nation { get; set; }
        DateTime Birthday { get; set; }
        string Address { get; set; }
        string IdCardNo { get; set; }
        string GrantDept { get; set; }
        DateTime EffectiveDate { get; set; }
        DateTime ExpireDate { get; set; }
        string PortraitPath { get; set; }
    }

    public class IdCardModel : ModelBase, IIdCardModel
    {
        /// <summary>
        ///     身份证姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     性别
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        ///     民族
        /// </summary>
        public string Nation { get; set; }

        /// <summary>
        ///     出生日期
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        ///     家庭住址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     身份证号
        /// </summary>
        public string IdCardNo { get; set; }

        /// <summary>
        ///     发证机关
        /// </summary>
        public string GrantDept { get; set; }

        /// <summary>
        ///     有效日期
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        ///     失效日期
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// 肖像地址
        /// </summary>
        public string PortraitPath { get; set; }
    }
}