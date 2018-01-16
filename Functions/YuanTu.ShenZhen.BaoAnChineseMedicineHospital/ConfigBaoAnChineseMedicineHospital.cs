using System.Net;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;
using YuanTu.ISO8583;
using YuanTu.ShenZhenArea.Enums;
using YuanTu.Consts.Models.Print;
using System.Drawing;
using YuanTu.ShenZhenArea.Services;
using YuanTu.Core.Log;
using System.Collections.Generic;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital
{
    public class ConfigBaoAnChineseMedicineHospital
    {
        /// <summary>
        ///退出系统的密码
        /// </summary>
        public const string ExitPass = "123123123";

        /// <summary>
        ///清钱箱的密码
        /// </summary>
        public const string ClearCashboxPass = "774651";

        /// <summary>
        ///是否开启社保卡输入密码
        /// </summary>
        public static bool OpenSheBaoCardPasswordEnter { set; get; }

        /// <summary>
        /// 是否关闭打印机检测
        /// </summary>
        public static bool ClosePrintStatusCheck { set; get; }

        public static string 上午挂号开始时间 { get; set; }
        public static string 上午挂号停止时间 { get; set; }
        public static string 下午挂号开始时间 { get; set; }
        public static string 下午挂号停止时间 { get; set; }


        public static string 上午取号开始时间 { get; set; }
        public static string 上午取号停止时间 { get; set; }
        public static string 下午取号开始时间 { get; set; }
        public static string 下午取号停止时间 { get; set; }

        public static Dictionary<string, string> SheKangBianMa;
        public static string BenBuBianMa;

        public static bool 康复分院的机器 { get; set; }

        /// <summary>
        ///银联配置
        /// </summary>
        public static Config PosConfig { get; set; }

        public static void Init()
        {
            LoadPrinterConfig();
            LoadPosConfig();
            LoadSheBaoConfig();
            LoadPrintConfig();
            LoadAccountingConfig();
            LoadRegisterTime();
            LoadSheKangList();
            LoadOthersConfigs();
        }


        /// <summary>
        /// 记账地址配置
        /// </summary>
        private static void LoadAccountingConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            AccountingBase.JiaoYiAccountingUrl = config.GetValue("AccountingAddress:jyjz");
            AccountingBase.InsuranceAccountingUrl = config.GetValue("AccountingAddress:ybjz");

            HttpPost.Log += Logger.Net.Info;
        }

        /// <summary>
        /// 社保配置
        /// </summary>
        private static void LoadSheBaoConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            OpenSheBaoCardPasswordEnter = config.GetValue("SheBao:OpenSheBaoPassword") == "1";
            ShenZhenArea.Insurance.YBBase.InitSheBaoBase(config);
        }
        /// <summary>
        /// 打印机配置
        /// </summary>
        public static void LoadPrinterConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            ClosePrintStatusCheck = config.GetValue("Print:CloseChecked")?.Trim() == "1";
        }
        /// <summary>
        /// 打印配置
        /// </summary>
        public static void LoadPrintConfig()
        {
            PrintConfig.HeaderFont = new Font("宋体", 14, FontStyle.Bold);
            PrintConfig.HeaderFont2 = new Font("宋体", 12, FontStyle.Regular);
            PrintConfig.DefaultFont = new Font("宋体", 10, FontStyle.Regular);
        }
        /// <summary>
        /// POS配置
        /// </summary>
        public static void LoadPosConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            PosConfig = new Config
            {
                Address = IPAddress.Parse(config.GetValue("Pos:IP")),
                Port = config.GetValueInt("Pos:Port"),
                TPDU = config.GetValue("Pos:TPDU"),
                Head = config.GetValue("Pos:Head"),
                TerminalId = config.GetValue("Pos:TerminalId"),
                MerchantId = config.GetValue("Pos:MerchantId"),
            };
        }



        /// <summary>
        /// 挂号、取号时间读取
        /// </summary>
        public static void LoadRegisterTime()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            上午挂号开始时间 = (config.GetValue("挂号时间:上午:开始时间") ?? "0730").PadLeft(4, '0').Insert(2, ":");
            上午挂号停止时间 = (config.GetValue("挂号时间:上午:结束时间") ?? "1200").PadLeft(4, '0').Insert(2, ":");
            下午挂号开始时间 = (config.GetValue("挂号时间:下午:开始时间") ?? "1330").PadLeft(4, '0').Insert(2, ":");
            下午挂号停止时间 = (config.GetValue("挂号时间:下午:结束时间") ?? "1700").PadLeft(4, '0').Insert(2, ":");

            上午取号开始时间 = (config.GetValue("取号时间:上午:开始时间") ?? "0730").PadLeft(4, '0').Insert(2, ":");
            上午取号停止时间 = (config.GetValue("取号时间:上午:结束时间") ?? "1200").PadLeft(4, '0').Insert(2, ":");
            下午取号开始时间 = (config.GetValue("取号时间:下午:开始时间") ?? "1300").PadLeft(4, '0').Insert(2, ":");
            下午取号停止时间 = (config.GetValue("取号时间:下午:结束时间") ?? "1700").PadLeft(4, '0').Insert(2, ":");
        }
        /// <summary>
        /// 社康的名称列表
        /// </summary>
        public static void LoadSheKangList()
        {
            SheKangBianMa = new Dictionary<string, string>();
            SheKangBianMa.Add("hbb70", "宝文社康");
            SheKangBianMa.Add("hb140", "上合社康");
            SheKangBianMa.Add("hbz50", "翻身社康");
            SheKangBianMa.Add("h5w40", "海旺社康");
            SheKangBianMa.Add("h6a50", "灵芝社康");
            SheKangBianMa.Add("h6a60", "凯旋社康");
            SheKangBianMa.Add("hec60", "中州社康");
            BenBuBianMa = "hbz90";
        }

        public static void LoadOthersConfigs()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            string 分院机器String = config.GetValue("分院机器");
            康复分院的机器 = (!string.IsNullOrEmpty(分院机器String)) && (分院机器String == "1");
        }
    }
}