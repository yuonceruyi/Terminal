using System;
using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class AppointRegLogVO : AppointRegLogDO
    {
        /// <summary>
        ///     就诊日期
        /// </summary>
        public long medDate { get; set; }

        /// <summary>
        ///     就诊时间段
        /// </summary>
        public long medBegTime { get; set; }

        /// <summary>
        ///     就诊时间段
        /// </summary>
        public long medEndTime { get; set; }

        /// <summary>
        ///     就诊时间
        /// </summary>
        public string medTime { get; set; }

        /// <summary>
        ///     返回接收预约短信的手机号
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        ///     缴费单状态
        /// </summary>
        public int paymentStatus { get; set; }

        public List<PaymentItemDO> prescribedReport { get; set; }

        /// <summary>
        ///     支付流水
        /// </summary>
        public PlatfomFeeLogDO platfomFeeLogDO { get; set; }

        /// <summary>
        ///     退费订单
        /// </summary>
        public RefundLogDO refundLogDO { get; set; }

        /// <summary>
        ///     状态描述
        /// </summary>
        public string statusDes { get; set; }

        /// <summary>
        ///     支付方式
        /// </summary>
        public string payTypeDesc { get; set; }

        /// <summary>
        ///     过期时间，当<0时， 过期
        /// </summary>
        public long expirationTime { get; set; }

        /// <summary>
        ///     是否能进行评价
        /// </summary>
        public bool canEvaluate { get; set; }

        /// <summary>
        ///     渠道类型名称
        /// </summary>
        public string channelTypeName { get; set; }

        /// <summary>
        ///     预约挂号类型名称
        /// </summary>
        public string regModeName { get; set; }

        /// <summary>
        ///     门诊类型名称
        /// </summary>
        public string regTypeName { get; set; }

        /// <summary>
        ///     性别
        /// </summary>
        public string sexDesc { get; set; }

        /// <summary>
        ///     病情主诉
        /// </summary>
        public string illComplained { get; set; }

        /// <summary>
        ///     医生诊断
        /// </summary>
        public string doctorDiagnosis { get; set; }

        /// <summary>
        ///     医嘱
        /// </summary>
        public string doctorOrder { get; set; }

        /// <summary>
        ///     检查检验
        /// </summary>
        public string inspection { get; set; }

        /// <summary>
        ///     处方，用药
        /// </summary>
        public string prescription { get; set; }

        /// <summary>
        ///     初诊 复诊
        /// </summary>
        public string visitDoctor { get; set; }
    }
}