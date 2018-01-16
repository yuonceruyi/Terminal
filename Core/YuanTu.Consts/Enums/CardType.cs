using System;

namespace YuanTu.Consts.Enums
{
    public enum CardType
    {
        /// <summary>
        /// 无卡
        /// </summary>
        NoCard = 0,
        /// <summary>
        /// 身份证
        /// </summary>
        身份证 = 1,
        /// <summary>
        /// 医院就诊卡
        /// </summary>
        就诊卡 = 2,
        /// <summary>
        /// 银行卡
        /// </summary>
        银行卡 = 5,
        /// <summary>
        /// 社保卡
        /// </summary>
        社保卡 = 10,
        /// <summary>
        /// 居民健康卡
        /// </summary>
        居民健康卡 = 11,
        /// <summary>
        /// 扫码
        /// </summary>
        扫码 = 12,
        /// <summary>
        /// 医保卡，目前为特殊项目使用
        /// </summary>
       [Obsolete] 医保卡 = 13,
        /// <summary>
        /// 刷脸
        /// </summary>
        刷脸 = 22,

        /// <summary>
        /// 指纹
        /// </summary>
        指纹 = 23,

        /// <summary>
        /// 条码卡
        /// </summary>
        条码卡 = 24,
        #region Local

        /// <summary>
        /// 省医保卡
        /// </summary>
        省医保卡 = -10,
        /// <summary>
        /// 市医保卡
        /// </summary>
        市医保卡 = -11,
        /// <summary>
        /// 门诊号
        /// </summary>
        门诊号 = -12,
        /// <summary>
        /// 住院号
        /// </summary>
        住院号 = -13,

        #endregion
    }
}