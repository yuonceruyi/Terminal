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

namespace YuanTu.ShenZhen.PekingUniversityHospital
{
    public class ConfigPekingUniversityHospital
    {
        /// <summary>
        ///退出系统的密码
        /// </summary>
        public const string ExitPass = "123123123";

        /// <summary>
        ///清钱箱的密码
        /// </summary>
        public const string ClearCashboxPass = "123698745";

        /// <summary>
        ///是否开启社保卡输入密码
        /// </summary>
        public static bool OpenSheBaoCardPasswordEnter { set; get; }

        /// <summary>
        /// 是否关闭打印机检测
        /// </summary>
        public static bool ClosePrintStatusCheck { set; get; }


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
    }
}