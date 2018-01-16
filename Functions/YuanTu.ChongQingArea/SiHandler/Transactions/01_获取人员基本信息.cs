using System;

namespace YuanTu.ChongQingArea.SiHandler
{
    #region 01_获取人员基本信息

    /// <summary>
    ///     功能描述: 通过可以唯一表示人员身份的个人社会保障号，获取参保人员指定险种的个人基础信息。
    /// </summary>
    public class Req获取人员基本信息 : Req
    {
        public override string 交易类别代码 => "01";
        public override string 交易类别 => "获取人员基本信息";

        public override string ToQuery()
        {
            return $"{交易类别代码}|{(string.IsNullOrEmpty(社保卡卡号) ? 老医保卡卡号 : 社保卡卡号)}|{险种类别}|{工伤个人编号}|{工伤单位编号}";
        }

        #region Properties

        /// <summary>
        ///     社保卡卡号
        ///     Varchar2(20)
        ///     非空
        ///     社保卡卡号、老医保卡卡号二选一，其中： 社保卡卡号 9 位； 老医保卡卡号 8 位; 注：城居参保人未拿到卡之前，用身份证号 或者金保号输入
        /// </summary>
        public string 社保卡卡号 { get; set; }

        /// <summary>
        ///     老医保卡卡号
        ///     Varchar2(8)
        ///     非空
        /// </summary>
        public string 老医保卡卡号 { get; set; }

        /// <summary>
        ///     险种类别
        ///     Varchar2(3)
        ///     非空
        ///     1、医疗保险；2、工伤保险；3、生育保险；
        /// </summary>
        public string 险种类别 { get; set; }

        /// <summary>
        ///     工伤个人编号
        ///     Varchar2(10)
        ///     若是工伤参保患者时，此字段不可以为空
        /// </summary>
        public string 工伤个人编号 { get; set; }

        /// <summary>
        ///     工伤单位编号
        ///     Varchar2(10)
        ///     若是工伤参保患者时，此字段不可以为空
        /// </summary>
        public string 工伤单位编号 { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 通过可以唯一表示人员身份的个人社会保障号，获取参保人员指定险种的个人基础信息。
    /// </summary>
    public class Res获取人员基本信息 : Res
    {
        //public string ToQuery()
        //{
        //    //医疗返回参数【包括职工医保、居民医保、离休干部医保】:
        //    var s1 = $"{执行代码}|{姓名}|{性别}|{实足年龄}|{身份证号}|{民族}|{住址}|{人员类别}|{是否享受公务员待遇}|{单位名称}|{行政区划编码}|{封锁状况}|{封锁原因}|{人员变更类型}|{人员类型变更时间}|{待遇封锁开始时间}|{待遇封锁终止时间}|{民政人员类别}|{居民缴费档次}|{参保类别}|{患者联系电话}";
        //    //工伤返回参数:
        //    var s2 = $"{执行代码}|{姓名}|{性别}|{身份证号}|{单位名称}|{行政区划编码}|{参保状态}|{工伤参保时间}|{封锁状况}|{封锁原因}|{待遇封锁开始时间}|{待遇封锁终止时间}|{转院转诊}|{辅助器具超标审批}";
        //    //生育返回参数:
        //    return $"{执行代码}|{姓名}|{性别}|{生育参保时间}|{身份证号}|{就医登记证号}|{可否享受就诊标志}|{不能享受就诊原因}|{就医登记机构编码}|{并发症标志}|{享受待遇实际开始日期}|{享受待遇实际结束日期}|{行政区划编码}";
        //}
        public static Res获取人员基本信息 Parse(string s, string 险种类别 = "1")
        {
            var list = s.Split('|');
            var res = new Res获取人员基本信息();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            switch (险种类别)
            {
                case "1":
                    res.姓名 = list[1];
                    res.性别 = list[2];
                    res.实足年龄 = list[3];
                    res.身份证号 = list[4];
                    res.民族 = list[5];
                    res.住址 = list[6];
                    res.人员类别 = list[7];
                    res.是否享受公务员待遇 = list[8];
                    res.单位名称 = list[9];
                    res.行政区划编码 = list[10];
                    res.封锁状况 = list[11];
                    res.封锁原因 = list[12];
                    res.人员变更类型 = list[13];
                    res.人员类型变更时间 = list[14];
                    res.待遇封锁开始时间 = list[15];
                    res.待遇封锁终止时间 = list[16];
                    res.民政人员类别 = list[17];
                    res.居民缴费档次 = list[18];
                    res.参保类别 = list[19];
                    res.患者联系电话 = list[20];
                    break;

                case "2":
                    res.姓名 = list[1];
                    res.性别 = list[2];
                    res.身份证号 = list[3];
                    res.单位名称 = list[4];
                    res.行政区划编码 = list[5];
                    res.参保状态 = list[6];
                    res.工伤参保时间 = list[7];
                    res.封锁状况 = list[8];
                    res.封锁原因 = list[9];
                    res.待遇封锁开始时间 = list[10];
                    res.待遇封锁终止时间 = list[11];
                    res.转院转诊 = list[12];
                    res.辅助器具超标审批 = list[13];
                    break;

                case "3":
                    res.姓名 = list[1];
                    res.性别 = list[2];
                    res.生育参保时间 = list[3];
                    res.身份证号 = list[4];
                    res.就医登记证号 = list[5];
                    res.可否享受就诊标志 = list[6];
                    res.不能享受就诊原因 = list[7];
                    res.就医登记机构编码 = list[8];
                    res.并发症标志 = list[9];
                    res.享受待遇实际开始日期 = list[10];
                    res.享受待遇实际结束日期 = list[11];
                    res.行政区划编码 = list[12];
                    break;

                default:
                    throw new NotSupportedException(nameof(险种类别));
            }
            // 医疗返回参数【包括职工医保、居民医保、离休干部医保】:执行代码|姓名|性别|实足年龄|身份证号|民族|住址|人员类别|是否享受公务员待遇|单位名称|行政区划编码|封锁状况|封锁原因|人员变更类型|人员类型变更时间|待遇封锁开始时间|待遇封锁终止时间|民政人员类别|居民缴费档次|参保类别|患者联系电话工伤返回参数:执行代码|姓名|性别|身份证号|单位名称|行政区划编码|参保状态|参保时间|封锁状况|封锁原因|待遇封锁开始时间|待遇封锁终止时间|转院转诊|辅助器具超标审批生育返回参数:执行代码|姓名|性别|参保时间|身份证号|就医登记证号|可否享受就诊标志|不能享受就诊原因|就医登记机构编码|并发症标志|享受待遇实际开始日期|享受待遇实际结束日期|行政区划编码
            return res;
        }

        #region Properties

        /// <summary>
        ///     姓名
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 姓名 { get; set; }

        /// <summary>
        ///     性别
        ///     Varchar2(10) 
        ///     非空
        /// </summary>
        public string 性别 { get; set; }

        /// <summary>
        ///     实足年龄
        ///     Number(3)
        ///     非空
        /// </summary>
        public string 实足年龄 { get; set; }

        /// <summary>
        ///     身份证号
        ///     Varchar2(18)
        ///     为空表示系统未记录该人员的身份证号
        /// </summary>
        public string 身份证号 { get; set; }

        /// <summary>
        ///     民族
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 民族 { get; set; }

        /// <summary>
        ///     住址
        ///     Varchar2(50)
        ///     为空表示系统未记录该人员住址
        /// </summary>
        public string 住址 { get; set; }

        /// <summary>
        ///     人员类别
        ///     Varchar2(3)
        ///     非空
        ///     11	在职 21	退休 41   成年人 42   未成年人 43   大学生
        /// </summary>
        public string 人员类别 { get; set; }

        /// <summary>
        ///     是否享受公务员待遇
        ///     Varchar2(3)
        ///     非空
        ///     0	不享受 1	享受
        /// </summary>
        public string 是否享受公务员待遇 { get; set; }

        /// <summary>
        ///     单位名称
        ///     Varchar2(50)
        ///     为空表示该人员无单位
        /// </summary>
        public string 单位名称 { get; set; }

        /// <summary>
        ///     行政区划编码
        ///     Varchar2(14)
        ///     非空
        ///     详细说明见附件。
        /// </summary>
        public string 行政区划编码 { get; set; }

        /// <summary>
        ///     封锁状况
        ///     Varchar2(3)
        ///     10	待遇审核期 11	个体参保待遇审核期 15	基本转个体待遇审核 16	个体转基本待遇审核 20	欠费封锁 21	基本医疗欠费 23	大额医疗欠费 29	住院医疗欠费 30	停保 40	退保 45	暂停待遇享受 50	IC
        ///     卡挂失 51	IC 卡注销 52	假卡 90	其他 为空表示该人员无封锁。
        /// </summary>
        public string 封锁状况 { get; set; }

        /// <summary>
        ///     封锁原因
        ///     Varchar2(128)
        ///     封锁状况为空时，封锁原因为空。
        /// </summary>
        public string 封锁原因 { get; set; }

        /// <summary>
        ///     人员变更类型
        ///     Varchar2(3)
        ///     11	新参保 12	续保 13	统筹范围外转入 14	统筹内本区转入 15	退休转在职 16	统筹内跨区转入 19	退保恢复 21	退保 22	停保 23	转出统筹范围外 24	统筹内本区转出 25	在职转退休 26
        ///     统筹内跨区转出 30	修改参保时间 44	暂停待遇享受 45	恢复待遇享受 46	参加农民工医疗保险 47	农民工转非农民工 99	其他 为空表示该人员无变更
        /// </summary>
        public string 人员变更类型 { get; set; }

        /// <summary>
        ///     人员类型变更时间
        ///     Date
        ///     为空表示该人员无变更
        /// </summary>
        public string 人员类型变更时间 { get; set; }

        /// <summary>
        ///     待遇封锁开始时间
        ///     Date
        ///     为空表示该人员无封锁
        /// </summary>
        public string 待遇封锁开始时间 { get; set; }

        /// <summary>
        ///     待遇封锁终止时间
        ///     Date
        ///     为空表示该人员无封锁或封锁无终止日期
        /// </summary>
        public string 待遇封锁终止时间 { get; set; }

        /// <summary>
        ///     民政人员类别
        ///     VARCHAR2(3)
        ///     1 城乡低保对象 2 城市三无人员 3 农村五保对象 4 城乡孤儿 5  在乡重点优抚对象(不含 1-6 级残疾军人) 6 城乡重度(一、二级)残疾人员 7 民政部门建档其他人员 8 家庭经济困难大学生 9 在乡老复原军人
        ///     为空时表示不享受民政相关待遇。
        /// </summary>
        public string 民政人员类别 { get; set; }

        /// <summary>
        ///     居民缴费档次
        ///     VARCHAR2(3)
        ///     一档；2 二档 为空时表示非居民参保人员。
        /// </summary>
        public string 居民缴费档次 { get; set; }

        /// <summary>
        ///     参保类别
        ///     VARCHAR2(3)
        ///     非空
        ///     1  职工参保；2  居民参保；3 离休干部
        /// </summary>
        public string 参保类别 { get; set; }

        /// <summary>
        ///     患者联系电话
        ///     VARCHAR2(20)
        /// </summary>
        public string 患者联系电话 { get; set; }

        /// <summary>
        ///     参保状态
        ///     VARCHAR2(3)
        ///     非空
        ///     1  正常参保；2  暂停参保；3 终止参保
        /// </summary>
        public string 参保状态 { get; set; }

        /// <summary>
        ///     工伤参保时间
        ///     Date
        ///     非空
        /// </summary>
        public string 工伤参保时间 { get; set; }

        /// <summary>
        ///     转院转诊
        ///     VARCHAR2(3)
        ///     为空表示无转院转诊发生
        /// </summary>
        public string 转院转诊 { get; set; }

        /// <summary>
        ///     辅助器具超标审批
        ///     VARCHAR2(3)
        ///     非空
        ///     1、需要审批；0、不需要审批
        /// </summary>
        public string 辅助器具超标审批 { get; set; }

        /// <summary>
        ///     生育参保时间
        ///     Date
        ///     非空
        /// </summary>
        public string 生育参保时间 { get; set; }

        /// <summary>
        ///     可否享受就诊标志
        ///     VARCHAR2(3)
        ///     非空
        ///     1、可以享受；0、不予享受
        /// </summary>
        public string 可否享受就诊标志 { get; set; }

        /// <summary>
        ///     不能享受就诊原因
        ///     VARCHAR2(200)
        ///     可以享受待遇时，参数为空
        /// </summary>
        public string 不能享受就诊原因 { get; set; }

        /// <summary>
        ///     就医登记机构编码
        ///     VARCHAR2(8)
        /// </summary>
        public string 就医登记机构编码 { get; set; }

        /// <summary>
        ///     并发症标志
        ///     VARCHAR2(3)
        ///     0、无并发症；1、有并发症
        /// </summary>
        public string 并发症标志 { get; set; }

        /// <summary>
        ///     享受待遇实际开始  (结束)日期
        ///     Date
        /// </summary>
        public string 享受待遇实际结束日期 { get; set; }

        /// <summary>
        ///     享受待遇实际开始  (结束)日期
        ///     Date
        /// </summary>
        public string 享受待遇实际开始日期 { get; set; }

        public string 就医登记证号 { get; set; }

        #endregion Properties
    }

    #endregion 01_获取人员基本信息
}