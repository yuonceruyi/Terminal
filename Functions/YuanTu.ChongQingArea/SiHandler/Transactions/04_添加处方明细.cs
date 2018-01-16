using System.Collections.Generic;
using System.Linq;

namespace YuanTu.ChongQingArea.SiHandler
{

    #region 04_添加处方明细

    /// <summary>
    ///     功能描述: 根据 HIS 传入的参数，生成处方明细信息；同时完成费用的初步审核：判断录入项目的合法性，由目录或患者的待遇封锁情况设定项目的各项待遇指标 等。同时，根据医保相关政策，若该录入项目属需审批项目，则返回审批标
    ///     志。对于返回的自费和自付金额仅作参考。一切计算以结算结果为准。
    /// </summary>
    public class Req添加处方明细 : Req
    {
        public override string 交易类别代码 => "04";
        public override string 交易类别 => "添加处方明细";

        /// <summary>
        ///     注：在批量上传模式下，可批量进行处方明细的上传，最大上传条数 10 条， 具体格式为：
        ///     {住院号}
        ///     |{处方号}|{开方日期}|{项目医保流水号}|{医院内码}|{项目名称}|{单价}|{数量}|{急诊标志}|{处方医生}|{经办人}|{单位}|{规格}|{剂型}|{冲消明细流水号}|{金额}|{科室编码}|{科室名称}|{医师编码}|{每次用量}|{用法标准}|{执行周期}|{险种类别}|{转自费标识}
        ///     ${处方号}|{开方日期}|{项目医保流水号}|{医院内码}|{项目名称}|{单价}|{数量}|{急诊标志}|{处方医生}|{经办人}|{单位}|{规格}|{剂型}|{冲消明细流水号}|{金额}|{科室编码}|{科室名称}|{医师编码}|{每次用量}|{用法标准}|{执行周期}|{险种类别}|{转自费标识}
        ///     ……
        /// </summary>
        /// <returns></returns>
        public override string ToQuery()
        {
            return
                $"{交易类别代码}|{住院号_门诊号}|{string.Join("$", Items.Select(i => i.ToQuery()))}";
        }

        public List<添加处方明细Item> Items { get; set; }

        #region Properties

        /// <summary>
        ///     住院(门诊)号
        ///     Varchar2(18)
        ///     非空
        /// </summary>
        public string 住院号_门诊号 { get; set; }

        #endregion Properties
    }

    public class 添加处方明细Item
    {

        /// <summary>
        ///     处方号
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 处方号 { get; set; }

        /// <summary>
        ///     开方日期
        ///     DATE
        ///     非空
        ///     格式：YYYY-MM-DD HH24:MI:SS
        /// </summary>
        public string 开方日期 { get; set; }

        /// <summary>
        ///     项目医保流水号
        ///     Varchar2(10)
        ///     非空
        /// </summary>
        public string 项目医保流水号 { get; set; }

        /// <summary>
        ///     医院内码
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 医院内码 { get; set; }

        /// <summary>
        ///     项目名称
        ///     Varchar2(50)
        ///     非空
        /// </summary>
        public string 项目名称 { get; set; }

        /// <summary>
        ///     单价
        ///     Number(10,4)
        ///     非空
        /// </summary>
        public string 单价 { get; set; }

        /// <summary>
        ///     数量
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 数量 { get; set; }

        /// <summary>
        ///     急诊标志
        ///     Varchar2(3)
        ///     0、非急诊；1、急诊；2、术前门诊检查； 3、择期手术项目 非急诊或术前门诊检查时，可以为空
        /// </summary>
        public string 急诊标志 { get; set; }

        /// <summary>
        ///     处方医生名字
        ///     Varchar2(50)
        ///     非空
        /// </summary>
        public string 处方医生名字 { get; set; }

        /// <summary>
        ///     经办人
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 经办人 { get; set; }

        /// <summary>
        ///     单位
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 单位 { get; set; }

        /// <summary>
        ///     规格
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 规格 { get; set; }

        /// <summary>
        ///     剂型
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 剂型 { get; set; }

        /// <summary>
        ///     冲销流水号
        ///     Varchar2(20)
        ///     非冲销明细时，此字段为空
        /// </summary>
        public string 冲消明细流水号 { get; set; }

        /// <summary>
        ///     金额
        ///     Number(10,4)
        ///     非空
        /// </summary>
        public string 金额 { get; set; }

        /// <summary>
        ///     科室编码
        ///     Varchar2(10)
        ///     非空
        ///     定点医疗机构科室统一编码
        /// </summary>
        public string 科室编码 { get; set; }

        /// <summary>
        ///     科室名称
        ///     Varchar2(40)
        ///     非空
        ///     定点医疗机构科室名称
        /// </summary>
        public string 科室名称 { get; set; }

        /// <summary>
        ///     医师编号
        ///     Varchar2(10)
        ///     非空
        ///     处方医师编号
        /// </summary>
        public string 医师编码 { get; set; }

        /// <summary>
        ///     每次用量
        ///     Varchar2(20)
        ///     非药品时，此字段为空
        /// </summary>
        public string 每次用量 { get; set; }

        /// <summary>
        ///     用法标准
        ///     Varchar2(20)
        ///     非药品时，此字段为空
        /// </summary>
        public string 用法标准 { get; set; }

        /// <summary>
        ///     执行周期
        ///     Varchar2(20)
        ///     非药品时，此字段为空
        /// </summary>
        public string 执行周期 { get; set; }

        /// <summary>
        ///     险种类别
        ///     Varchar2(3)
        ///     非空
        ///     1、医疗保险；2、工伤保险；3、生育保险；
        /// </summary>
        public string 险种类别 { get; set; }

        /// <summary>
        ///     转自费标识
        ///     Varchar2(3)
        ///     0、 不转 ；1 、转自费； 非转自费时可为空； 注： 转自费时出参中的项目等级、自付比 例、自费金额、自付金额要做出相应调整
        /// </summary>
        public string 转自费标识 { get; set; }

        public string ToQuery()
        {
            return
                $"{处方号}|{开方日期}|{项目医保流水号}|{医院内码}|{项目名称}|{单价}|{数量}|{急诊标志}|{处方医生名字}|{经办人}|{单位}|{规格}|{剂型}|{冲消明细流水号}|{金额}|{科室编码}|{科室名称}|{医师编码}|{每次用量}|{用法标准}|{执行周期}|{险种类别}|{转自费标识}";
        }
    }

    /// <summary>
    ///     功能描述: 根据 HIS 传入的参数，生成处方明细信息；同时完成费用的初步审核：判断录入项目的合法性，由目录或患者的待遇封锁情况设定项目的各项待遇指标 等。同时，根据医保相关政策，若该录入项目属需审批项目，则返回审批标
    ///     志。对于返回的自费和自付金额仅作参考。一切计算以结算结果为准。
    /// </summary>
    public class Res添加处方明细 : Res
    {
        public List<处方明细Item> 处方明细 { get; set; }

        /// <summary>
        ///     注 ：如进行的是批量上传明细的方式，返回参数对应为：
        ///     {执行代码}
        ///     |{交易流水号}|{项目单价}|{审批标记}|{审批规则}|{项目费用总额}|{项目等级}|{自付比例}|{标准单价}|{自付金额}|{自费金额}
        ///     ${交易流水号}|{项目单价}|{审批标记}|{审批规则}|{项目费用总额}|{项目等级}|{自付比例}|{标准单价}|{自付金额}|{自费金额}
        ///     ……
        /// </summary>
        /// <returns></returns>
        //public string ToQuery()
        //{
        //    return $"{执行代码}|{交易流水号}|{项目单价}|{审批标记}|{审批规则}|{项目费用总额}|{项目等级}|{自付比例}|{标准单价}|{自付金额}|{自费金额}";
        //}
        public static Res添加处方明细 Parse(string s)
        {
            var list = s.Split('$');
            var res = new Res添加处方明细();
            var items = new List<处方明细Item>();
            for (var i = 0; i < list.Length; i++)
            {
                if (string.IsNullOrEmpty(list[i]))
                    continue;
                var subList = list[i].Split('|');
                if (i == 0)
                {
                    res.执行代码 = subList[0];
                    if (res.执行代码 != "1")
                    {
                        res.错误信息 = subList[1];
                        return res;
                    }
                    items.Add(处方明细Item.Parse(subList, 1));
                }
                else
                {
                    items.Add(处方明细Item.Parse(subList, 0));
                }
            }
            res.处方明细 = items;
            // 执行代码|交易流水号|项目单价|审批标记|审批规则|项目费用总额|项目等级|自付比例|标准单价|自付金额|自费金额注 ：如进行的是批量上传明细的方式，返回参数对应为：执行代码|交易流水号|项目单价|审批标记|审批规则|项目费用总额|项目等级|自付比例|标准单价|自付金额|自费金额$交易流水号|项目单价|审批标记|审批规则|项目费用总额|项目等级|自付比例|标准单价|自付金额|自费金额……
            return res;
        }

        ///// <summary>

        //#region Properties
        ///// 交易流水号
        ///// Varchar2(20)
        ///// 非空
        /////
        ///// </summary>
        //public string 交易流水号 { get; set; }

        ///// <summary>
        ///// 实际单价
        ///// Number(10,4)
        ///// 非空
        /////
        ///// </summary>
        //public string 项目单价 { get; set; }

        ///// <summary>
        ///// 审批标志
        ///// Varchar2(5)
        ///// 非空
        ///// 0、不需审批；1、高收费审批； 2、血液白蛋白审批
        ///// </summary>
        //public string 审批标记 { get; set; }

        ///// <summary>
        ///// 审批规则
        ///// Varchar2(5)
        /////
        ///// 暂不使用。
        ///// </summary>
        //public string 审批规则 { get; set; }

        ///// <summary>
        ///// 项目费用总额
        ///// Number(10,4)
        ///// 非空
        ///// 本次发生的总费用，应该与输入参数的“数 量*单价”及“金额”参数在数值上值相同
        ///// </summary>
        //public string 项目费用总额 { get; set; }

        ///// <summary>
        ///// 项目等级
        ///// Varchar2(3)
        ///// 非空
        ///// 1、甲类；2、乙类；3、自费
        ///// </summary>
        //public string 项目等级 { get; set; }

        ///// <summary>
        ///// 自付比例
        ///// Number(5,4)
        ///// 非空
        ///// 目录中项目对应的自付比例
        ///// </summary>
        //public string 自付比例 { get; set; }

        ///// <summary>
        ///// 标准单价
        ///// Number(10,4)
        /////
        ///// 目录中规定的各项目符合医保报销范围的 最高支付金额 为空表示中心不对此项目进行限价
        ///// </summary>
        //public string 标准单价 { get; set; }

        ///// <summary>
        ///// 自付金额
        ///// Number(10,4)
        ///// 非空
        ///// 由自付比例及标准单价共同计算出的本次 费用应自付的金额；包括封锁自付
        ///// </summary>
        //public string 自付金额 { get; set; }

        ///// <summary>
        ///// 自费金额
        ///// Number(10,4)
        ///// 非空
        ///// 自费项目的所有费用和超限价自付记为自 费金额
        ///// </summary>
        //public string 自费金额 { get; set; }

        //#endregion
    }

    public class 处方明细Item
    {
        /// <summary>
        ///     交易流水号
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 交易流水号 { get; set; }

        /// <summary>
        ///     实际单价
        ///     Number(10,4)
        ///     非空
        /// </summary>
        public string 项目单价 { get; set; }

        /// <summary>
        ///     审批标志
        ///     Varchar2(5)
        ///     非空
        ///     0、不需审批；1、高收费审批； 2、血液白蛋白审批
        /// </summary>
        public string 审批标记 { get; set; }

        /// <summary>
        ///     审批规则
        ///     Varchar2(5)
        ///     暂不使用。
        /// </summary>
        public string 审批规则 { get; set; }

        /// <summary>
        ///     项目费用总额
        ///     Number(10,4)
        ///     非空
        ///     本次发生的总费用，应该与输入参数的“数 量*单价”及“金额”参数在数值上值相同
        /// </summary>
        public string 项目费用总额 { get; set; }

        /// <summary>
        ///     项目等级
        ///     Varchar2(3)
        ///     非空
        ///     1、甲类；2、乙类；3、自费
        /// </summary>
        public string 项目等级 { get; set; }

        /// <summary>
        ///     自付比例
        ///     Number(5,4)
        ///     非空
        ///     目录中项目对应的自付比例
        /// </summary>
        public string 自付比例 { get; set; }

        /// <summary>
        ///     标准单价
        ///     Number(10,4)
        ///     目录中规定的各项目符合医保报销范围的 最高支付金额 为空表示中心不对此项目进行限价
        /// </summary>
        public string 标准单价 { get; set; }

        /// <summary>
        ///     自付金额
        ///     Number(10,4)
        ///     非空
        ///     由自付比例及标准单价共同计算出的本次 费用应自付的金额；包括封锁自付
        /// </summary>
        public string 自付金额 { get; set; }

        /// <summary>
        ///     自费金额
        ///     Number(10,4)
        ///     非空
        ///     自费项目的所有费用和超限价自付记为自 费金额
        /// </summary>
        public string 自费金额 { get; set; }

        public static 处方明细Item Parse(string[] list, int i)
        {
            var res = new 处方明细Item();
            res.交易流水号 = list[i + 0];
            res.项目单价 = list[i + 1];
            res.审批标记 = list[i + 2];
            res.审批规则 = list[i + 3];
            res.项目费用总额 = list[i + 4];
            res.项目等级 = list[i + 5];
            res.自付比例 = list[i + 6];
            res.标准单价 = list[i + 7];
            res.自付金额 = list[i + 8];
            res.自费金额 = list[i + 9];
            return res;
        }
    }

    #endregion 04_添加处方明细
}