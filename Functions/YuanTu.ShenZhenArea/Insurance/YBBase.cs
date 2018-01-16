using System;
using YuanTu.Consts.Services;

namespace YuanTu.ShenZhenArea.Insurance
{
    public partial class YBBase
    {
        /// <summary>
        /// 医疗机构编码
        /// </summary>
        public static string yljgbm { get; set; }
        /// <summary>
        /// 操作员姓名
        /// </summary>
        public static string czyxm { get; set; }
        /// <summary>
        /// 操作员编码
        /// </summary>
        public static string czybm { get; set; }
        /// <summary>
        /// 操作员密码,社保文档没涉及到，应该不要
        /// </summary>
        [Obsolete]
        public static string czymm { get; set; }
        /// <summary>
        /// 医保前置URI
        /// </summary>
        public static Uri Uri { get; set; }

        /// <summary>
        /// 初始化社保配置
        /// </summary>
        /// <param name="shebaoConfig"></param>
        public static void InitSheBaoBase(IConfigurationManager shebaoConfig)
        {
            czybm = shebaoConfig.GetValue("SheBao:czybm");
            czyxm = shebaoConfig.GetValue("SheBao:czyxm");
            yljgbm = shebaoConfig.GetValue("SheBao:yljgbm");
            yljgbm = shebaoConfig.GetValue("SheBao:yljgbm");
            czymm = shebaoConfig.GetValue("SheBao:mm");
            string url = shebaoConfig.GetValue("SheBao:url");
            if (!string.IsNullOrEmpty(url))
                Uri = new Uri(url);
        }
    }
}