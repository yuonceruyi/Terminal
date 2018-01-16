using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Gateway;
using YuanTu.YanTaiArea.Base;

namespace YuanTu.YanTaiArea
{
    public class ConfigWeiHai
    {
        /// <summary>
        /// M1卡本地调试
        /// </summary>
        public static bool M1Local { get; private set; }

        /// <summary>
        /// 高权限密码
        /// </summary>
        public static string HighPwd { get; set; } = "A";
        /// <summary>
        /// 医保科室对照
        /// </summary>
        public static Dictionary<string, string> DeptCompareDictionary { get; private set; }        
        public static void Init()
        {
            LoadBase();
            LoadDeptCompareAndInit();
        }

        public static void LoadDeptCompareAndInit()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var SiPay = config.GetValue("PayOut:医保:Visabled");
            var SiCard = config.GetValue("Card:社保卡:Visabled");

            if (SiPay == "1")
            {
                //LoadDeptCompare();                
            }
            if (SiPay=="1" || SiCard=="1")
            {
                //WeiHaiArea.QingDaoSiPay.Function.Init();
            }
        }

        public static void LoadBase()
        {                       
            M1Local = Environment.GetCommandLineArgs().Any(one => one == "M1Local");
        }        
                
        public static string[] UseConfigPath()
        {
            return new[] { "ConfigWeiHai.xml" };
        }
    }
}
