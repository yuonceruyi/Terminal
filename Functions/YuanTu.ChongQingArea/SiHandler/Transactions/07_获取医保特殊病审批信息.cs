using System.Collections.Generic;

namespace YuanTu.ChongQingArea.SiHandler
{

    #region 07_获取医保特殊病审批信息

    /// <summary>
    ///     功能描述: 根据患者社会保障号，从中心获取该患者在本定点机构申报的特殊病审批信息。
    /// </summary>
    public class Req获取医保特殊病审批信息 : Req
    {
        public override string 交易类别代码 => "07";
        public override string 交易类别 => "获取医保特殊病审批信息";

        public override string ToQuery()
        {
            return $"{交易类别代码}|{(string.IsNullOrEmpty(社保卡卡号) ? 老医保卡卡号 : 社保卡卡号)}";
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

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 根据患者社会保障号，从中心获取该患者在本定点机构申报的特殊病审批信息。
    /// </summary>
    public class Res获取医保特殊病审批信息 : Res
    {
        public List<审批信息Item> Items { get; set; }

        /// <summary>
        ///     {执行代码}
        ///     |{病种编码1}|{病种名称1}|{并发症1}
        ///     ${病种编码2}|{病种名称2}|{并发症2}
        ///     $...
        ///     若患者无审批信息，返回结果为：1|##
        /// </summary>
        /// <returns></returns>
        //public string ToQuery()
        //{
        //    return $"{执行代码}|{病种编码}|{病种名称}|{并发症}";
        //}
        public static Res获取医保特殊病审批信息 Parse(string s)
        {
            var list = s.Split('$');
            var res = new Res获取医保特殊病审批信息();
            var items = new List<审批信息Item>();
            for (var i = 0; i < list.Length; i++)
            {
                if(string.IsNullOrEmpty(list[i]))
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
                    if (subList[1] == "##")
                        break;
                    items.Add(审批信息Item.Parse(subList, 1));
                }
                else
                {
                    items.Add(审批信息Item.Parse(subList, 0));
                }
            }
            res.Items = items;
            // 执行代码|病种编码1|病种名称1|并发症1$病种编码2|病种名称2|并发症2$...若患者无审批信息，返回结果为：1|##
            return res;
        }

        ///// <summary>
        //#region Properties
        ///// 病种编码
        ///// Varchar2(20)
        ///// 非空
        /////
        ///// </summary>
        //public string 病种编码 { get; set; }

        ///// <summary>
        ///// 病种名称
        ///// Varchar2(40)
        ///// 非空
        /////
        ///// </summary>
        //public string 病种名称 { get; set; }

        ///// <summary>
        ///// 并发症
        ///// Varchar2(200)
        /////
        ///// 为空表示无并发症
        ///// </summary>
        //public string 并发症 { get; set; }

        //#endregion
    }

    public class 审批信息Item
    {
        /// <summary>
        ///     病种编码
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 病种编码 { get; set; }

        /// <summary>
        ///     病种名称
        ///     Varchar2(40)
        ///     非空
        /// </summary>
        public string 病种名称 { get; set; }

        /// <summary>
        ///     并发症
        ///     Varchar2(200)
        ///     为空表示无并发症
        /// </summary>
        public string 并发症 { get; set; }

        public static 审批信息Item Parse(string[] list, int i)
        {
            var res = new 审批信息Item();
            res.病种编码 = list[i + 0];
            res.病种名称 = list[i + 1];
            res.并发症 = list[i + 2];
            return res;
        }
    }

    #endregion 07_获取医保特殊病审批信息
}