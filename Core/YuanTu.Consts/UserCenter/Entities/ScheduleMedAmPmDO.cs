namespace YuanTu.Consts.UserCenter.Entities
{
    public class ScheduleMedAmPmDO
    {
        public string deptCode { get; set; }
        public string deptName { get; set; }

        /// <summary>
        ///     挂号班次  可空  1上午 2 下午  ；指定就诊日期时有效
        /// </summary>
        public int medAmPm { get; set; } = -1;

        /// <summary>
        ///     挂号费 	 单位为分  一般费用为0
        /// </summary>
        public string regFee { get; set; }

        /// <summary>
        ///     诊疗费   	 单位为分 问诊费用
        /// </summary>
        public string treatFee { get; set; }

        /// <summary>
        ///     挂号金额  单位为分 挂号费+诊疗费
        /// </summary>
        public string regAmount { get; set; }

        /// <summary>
        ///     排班ID" //排班ID  string  不可空
        /// </summary>
        public string scheduleId { get; set; }

        /// <summary>
        ///     余号
        /// </summary>
        public string restnum { get; set; }

        /// <summary>
        ///     已使用数量。视频问诊使用
        /// </summary>
        public string appointedNum { get; set; }

        /// <summary>
        ///     pc-web 端 新加
        /// </summary>
        public string hosRegType { get; set; }

        /// <summary>
        ///     挂号类别  1普通，2专家，3名医 ，4 急诊，5 便民
        /// </summary>
        public int regType { get; set; } = -1;

        /// <summary>
        ///     预约挂号  1 预约   2 挂号
        /// </summary>
        public int regMode { get; set; } = -1;

        /// <summary>
        ///     专家子类型：1专家主任医师   2专家副主任医师
        /// </summary>
        public int subRegType { get; set; }

        /// <summary>
        ///     专家子类型名称：主任医师，副主任医师
        /// </summary>
        public string subRegTypeName { get; set; }
    }
}