namespace YuanTu.QDArea.QingDaoSiPay.Common
{
    /// <summary>
    /// 类型的类型。
    /// </summary>
    internal enum TypeOfType
    {
        /// <summary>
        /// 接口。
        /// </summary>
        Interface,
        /// <summary>
        /// 类。
        /// </summary>
        Class,
        /// <summary>
        /// 枚举。
        /// </summary>
        Enum
    }

    public enum EnumDiseaseSortSp
    {
        大病病种 = 1,
        工伤病种 = 2,
        生育病种 = 3,
        单病种 = 4,
        日间病房 = 5,
        乙类单病种 = 0//自行扩展
    }
    public enum EnumDiseaseSort
    {
        甲类病种 = 1,
        乙类病种 = 2,
        外伤病种 = 3,
        急性病种 = 4,
        丙类病种 = 5,
        少儿病种 = 6
    }
    public enum EnumInOrOutPatient
    {
        门诊 = 1,
        住院 = 2
    }
    public enum EnumMedicalType
    {
        门诊医保 = 1,
        门诊工伤 = 2,
        门诊生育 = 3,
        医保住院 = 11,
        生育住院 = 12,
        工伤住院 = 13
    }
    public enum EnumBalanceTypeMY//医疗保险门诊交易结算类别
    {
        社区普通门诊 = 1,
        门诊大病 = 2,
        离休人员门诊 = 3
    }
    public enum EnumBalanceTypeIndustrialinjury
    {
        医治 = 1,
        康复性治疗 = 2
    }    
    public enum EnumBalanceTypeZY
    {
        普通住院 = 1,
        转入医院 = 2,
        急诊住院 = 3,
        家庭病床 = 4,
        家床转住院 = 5,
        社区家床 = 6,
        护理包干 = 7,
        床日限额结算 = 8,
        老年专护 = 9,
        长期护理 = 10,
        居家护理 = 11,
        长期专护 = 12,
        日间病房 = 13,
        医保居民生育 = 14,
        社区护理 = 15,
        农村日间护理 = 16
    }
    public enum EnumEducationType//学历
    {
        博士 = 1,
        硕士 = 2,
        本科 = 3,
        大专 = 4,
        中专 = 5,
        其他 = 6
    }
    public enum EnumPost//职务
    {
        院长 = 1,
        副院长 = 2,
        主任 = 3,
        副主任 = 4,
        其他 = 5
    }
    public enum EnumTitle//技术职称
    {
        主任医师 = 1,
        副主任医师 = 2,
        主治医师 = 3,
        //副主治医师 = 4,
        医师 = 5,
        医士 = 6,
        住院医师 = 7,
        乡村医生 = 8
    }
    public enum EnumMaintainType//维护类型
    {
        收费终端维护 = 1,
        医师维护 = 2,
        操作人员维护 = 3,
        药品目录维护 = 4,
        诊疗目录维护 = 5,
        执业护士维护 = 6,
        护理员维护 = 7
    }
    public enum EnumOperType//操作要求
    {
        新增 = 1,
        变更 = 2,
        终止 = 3,
        变更价格 = 4,
        新增外购项目 = 5
    }
    public enum EnumFundType//基金类型
    {
        全部 = 0,
        医保 = 1,
        工伤 = 2,
        生育 = 3
    }
    public enum EnumBalanceMode//结算方式
    {
        正常结算 = 0,
        单病种结算 = 1,
        乙类单病种结算 = 2
    }
    public enum EnumExamType//审批类型
    {
        意外伤害准入审批 = 01,
        医保异地转诊审批 = 02,
        社区转诊住院审批 = 03,
        家庭病床老年护理审批 = 04,
        乙类病种审批 = 05,
        农民工住院审批 = 06,
        社区门诊统筹签约 = 07,
        家庭医生签约 = 08,
        农民工门诊定点 = 09,
        延期入院网上备案 = 10,
        精神病包干审批 = 11,
        工伤门诊定点住院审批 = 12,
        工伤异地转诊审批 = 13
    }
    /// <summary>
    /// 业务类别
    /// </summary>
    public enum EnumBusinessType
    {
        全部门诊 = 0,
        门诊大病 = 1,
        门诊离休 = 2,
        门诊工伤 = 3,
        门诊生育 = 4
    }
    /// <summary>
    /// 出院原因治疗效果
    /// //1 治愈 2 好转 3 未愈 4 死亡 5 转院 6 转外 7 家床转住院 8 中途结算 9 其他
    /// </summary>
    public enum EnumOutReasonType
    {
        治愈 = 1,
        好转 = 2,
        未愈 = 3,
        死亡 = 4,
        转院 = 5,
        转外 = 6,
        家床转住院 = 7,
        中途结算 = 8,
        其他 = 9
    }
    /// <summary>
    /// 生活自理能力评分(长期医疗护理审批)    
    /// </summary>
    public enum EnumLivingScore
    {
        分数0 = 0,
        分数5 = 5,
        分数10 = 10,
        分数15 = 15,
    }
    /// <summary>
    /// 护士技术职称    
    /// </summary>
    public enum EnumNurseTitle
    {
        护士 = 1,
        护师 = 2,
        主管护师 = 3,
        副主任护师 = 4,
        主任护师 = 5
    }
    /// <summary>
    /// 护理员技术职称    
    /// </summary>
    public enum EnumParamedicTitle
    {
        初级 = 1,
        中级 = 2,
        高级 = 3,
    }

    /// <summary>
    /// 长期医疗护理审批
    /// 申办类别    
    /// </summary>
    public enum EnumChronicCareExamType
    {
        长期护理 = 10,
        居家护理 = 11,
        社区护理 = 13,
        农村日间护理 = 14
    }

    /// <summary>
    /// 长期医疗护理审批
    /// 申办类别
    /// </summary>
    public enum EnumDocRange
    {
        内科专业 = 01,
        外科专业 = 02,
        妇产科专业 = 03,
        儿科专业 = 04,
        眼耳鼻咽喉科专业 = 05,
        皮肤病与性病专业 = 06,
        精神卫生专业 = 07,
        职业病专业 = 08,
        医学影像和放射治疗专业 = 09,
        医学检验病理专业 = 10,
        全科医学专业 = 11,
        急救医学专业 = 12,
        康复医学专业 = 13,
        预防保健专业 = 14,
        特种医学与军事医学专业 = 15,
        计划生育技术服务专业 = 16,
        口腔专业 = 17,
        公共卫生类别专业 = 18,
        中医专业 = 19,
        中西医结合专业 = 20,
        蒙医专业 = 21,
        藏医专业 = 22,
        维医专业 = 23,
        傣医专业 = 24,
        省级以上卫生行政部门规定的其他专业 = 25,
    }
    #region Windows Hook Codes
    public enum HookType
    {
        WH_MSGFILTER = (-1),
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }
    #endregion
}
