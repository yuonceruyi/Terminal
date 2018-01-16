using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Gateway;
using YuanTu.BJArea.Base;

namespace YuanTu.BJArea
{
    public class ConfigBJ
    {
        /// <summary>
        /// pos本地调试
        /// </summary>
        public static bool PosLocal { get; private set; }
        /// <summary>
        /// M1卡本地调试
        /// </summary>
        public static bool M1Local { get; private set; }

        /// <summary>
        /// 高权限密码
        /// </summary>
        public static string HighPwd { get; set; } = "A";

        /// <summary>
        /// 排班版本，默认0
        /// 0：默认，原模式
        /// 1：预约收费模式
        /// </summary>
        public static string ScheduleVersion { get; set; } = "0";
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
            var SiPay = config.GetValue("PayOut:社保:Visabled");
            var SiCard = config.GetValue("Card:社保卡:Visabled");

            if (SiPay == "1")
            {
                LoadDeptCompare();                
            }
            if (SiPay=="1" || SiCard=="1")
            {
                //BJArea.BeiJingSiPay.SiFunction.Init();
            }
        }

        public static void LoadBase()
        {           
            PosLocal = Environment.GetCommandLineArgs().Any(one => one == "PosLocal");
            M1Local = Environment.GetCommandLineArgs().Any(one => one == "M1Local");
        }        
        public static void LoadDeptCompare()
        {           
            var req = new req医保科室
            {
                //operId = FrameworkConst.HospitalAreaCode,
                //flowId = FrameworkConst.FlowId()
            };

            var res = DataHandlerEx.医保科室(req);
            if (res?.success ?? false)
            {
                if (res != null)
                {
                    DeptCompareDictionary = new Dictionary<string, string>();
                    foreach (var info in res.data)
                    {
                        DeptCompareDictionary.Add(info.HisDeptCode, info.SiDeptCode);
                    }
                    //BJArea.BeiJingSiPay.Common.SiSet.DeptCompare = DeptCompareDictionary;
                }
            }
        }
        public static string[] UseConfigPath()
        {
            return new[] { "ConfigBJ.xml","ConfigBankPos.xml" };
        }
    }
}
