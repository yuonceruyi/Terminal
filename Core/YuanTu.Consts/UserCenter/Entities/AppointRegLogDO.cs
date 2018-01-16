using System;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class AppointRegLogDO : BaseDO
    {
        /// <summary>
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     订单号(预约号或者挂号ID)
        /// </summary>
        public string orderNo { get; set; }

        /// <summary>
        ///     请使用 AppointRegStatusEnums  枚举类   订单状态
        ///     状态(100 待支付，101 支付成功-His失败，200 预约成功，201 挂号成功    预约记录上传 301 挂号失败，退款中...302 挂号失败，退款成功, 303 挂号失败，退款失败， 400 已取消,401
        ///     已过期，402 已作废)
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     挂号类型(1普通挂号，2专家挂号，3名医挂号，4普通预约，5专家预约，6名医预约)
        /// </summary>
        public int type { get; set; }

        /// <summary>
        ///     挂号方式 1 预约，2 挂号
        /// </summary>
        public int regMode { get; set; }

        /// <summary>
        ///     挂号类别  1普通，2专家，3名医     6视频问诊
        /// </summary>
        public int regType { get; set; }

        /// <summary>
        ///     医院的挂号类别
        /// </summary>
        public string hosRegType { get; set; }

        /// <summary>
        ///     挂号日期
        /// </summary>
        public long createDate { get; set; }

        /// <summary>
        ///     平台用户id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        ///     平台患者ID
        /// </summary>
        public long patientId { get; set; }

        /// <summary>
        ///     门诊号
        /// </summary>
        public string hisId { get; set; }

        /// <summary>
        ///     患者姓名
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     患者电话
        /// </summary>
        public string patientPhone { get; set; }

        /// <summary>
        ///     证件类型: 1 身份证, 2 军人证, 3 护照, 4 学生证, 5 回乡证, 6 驾驶证, 7 台胞证, 9 其它
        /// </summary>
        public int idType { get; set; }

        /// <summary>
        ///     平台患者身份证
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     '监护人id'
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     挂号序号  int 号源
        /// </summary>
        public string appoNo { get; set; }

        /// <summary>
        ///     就诊开始时间    就诊时间段 格式yyyy-MM-dd HH:mm:ss
        /// </summary>
        public long medDateBeg { get; set; }

        /// <summary>
        ///     就诊结束时间    就诊时间段 格式yyyy-MM-dd HH:mm:ss
        /// </summary>
        public long medDateEnd { get; set; }

        /// <summary>
        ///     挂号班次上午/下午
        /// </summary>
        public int medAmPm { get; set; }

        /// <summary>
        ///     就诊地点
        /// </summary>
        public string address { get; set; }

        /// <summary>
        ///     挂号金额 = 挂号费+诊疗费
        /// </summary>
        public int regAmount { get; set; }

        /// <summary>
        ///     优惠后挂号金额 = 挂号费+诊疗费
        /// </summary>
        public int benefitRegAmount { get; set; }

        /// <summary>
        ///     扩展信息
        /// </summary>
        public string extend { get; set; }

        /// <summary>
        ///     医院ID
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     医院名称
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     医联体ID
        /// </summary>
        public long corpUnionId { get; set; }

        /// <summary>
        ///     科室code
        /// </summary>
        public string deptCode { get; set; }

        /// <summary>
        ///     科室名
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        ///     医生code
        /// </summary>
        public string doctCode { get; set; }

        /// <summary>
        ///     医生姓名
        /// </summary>
        public string doctName { get; set; }

        /// <summary>
        ///     修改时间
        /// </summary>
        public long updateTime { get; set; }

        /// <summary>
        ///     院区代码  可空  可空，包含多个分院的情况需传入
        /// </summary>
        public string hospCode { get; set; }

        /// <summary>
        ///     排班ID" //排班ID  string  不可空
        /// </summary>
        public string scheduleId { get; set; }

        /// <summary>
        ///     锁号Id
        /// </summary>
        public string lockId { get; set; }

        /// <summary>
        ///     预约挂号渠道  1, "APP"  2, "自助机" 3, "微信" 4, "窗口" 9, "诊间"
        /// </summary>
        public int channelType { get; set; }

        /// <summary>
        ///     状态更新渠道
        /// </summary>
        public int statusChangeChannel { get; set; }

        public string diseaseDesc { get; set; }

        public string diseaseImageUrl { get; set; }

        public string doctAdvise { get; set; }

        public string billNo { get; set; }

        public string billDate { get; set; }

        public string billType { get; set; }

        public long billFee { get; set; }
        //视频问诊 添加，不知道用处，注释之
        //    public String password{get;set;}

        //附加状态，用来标识视频问诊是否已经结束1,或者处方单已开 2
        public int extraStatus { get; set; }

        /// <summary>
        ///     性别
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        ///     年龄
        /// </summary>
        public int age { get; set; }

        /// <summary>
        ///     收货电话
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        ///     地址
        /// </summary>
        public string expressAddress { get; set; }

        /// <summary>
        ///     收件人
        /// </summary>
        public string recipient { get; set; }

        /// <summary>
        ///     邮编
        /// </summary>
        public string postcode { get; set; }

        /// <summary>
        ///     取药类型 0为自取 1为快递
        /// </summary>
        public int getType { get; set; }

        /// <summary>
        ///     运单号
        /// </summary>
        public string expressCode { get; set; }

        /// <summary>
        ///     快递公司
        /// </summary>
        public string expressCompany { get; set; }

        /// <summary>
        ///     快递费用
        /// </summary>
        public int expressCost { get; set; }
    }
}