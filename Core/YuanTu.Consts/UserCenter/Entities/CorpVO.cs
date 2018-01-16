using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class CorpVO : CorpDO
    {
        public string validCode { get; set; }

        public long userId { get; set; }
        public List<int> funList { get; set; }

        /// <summary>
        ///     普通预约，专家预约；普通挂号，专家挂号
        /// </summary>
        public List<int> registerList { get; set; }

        /// <summary>
        ///     医院介绍
        /// </summary>
        public string corpIntro { get; set; }

        public List<CorpNewsDO> IntroInfo { get; set; }

        public PayTypesDO payTypes { get; set; }

        /// <summary>
        ///     子医院
        /// </summary>
        public List<CorpDO> leafList { get; set; }

        /// <summary>
        ///     医院简介
        /// </summary>
        public string corpDescription { get; set; }

        /// <summary>
        ///     预约收费模式：0 不收费，默认；1收费，2 可选收费
        /// </summary>
        public int appointMode { get; set; } = 0;

        /// <summary>
        ///     医院开通的线上服务功能 暂时只有预约挂号、挂号查询、排队叫号 在CorpFunctionEnums中定义
        /// </summary>
        public List<string> funcTagList { get; set; }

        /// <summary>
        ///     是否有分院
        /// </summary>
        public bool hasLeaf { get; set; } = false;
    }
}