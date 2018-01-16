using System;
using System.Linq;
using System.Threading;
using YuanTu.Consts.Enums;

namespace YuanTu.Consts
{
    public static class FrameworkConst
    {
        /// <summary>
        ///     命令行参数
        /// </summary>
        public static string LocalArgs
        {
            set
            {
                var args = value;
                if (args == null || !args.Contains("LOCAL=")) return;
                var localparams = args.Split(' ')?.FirstOrDefault(p => p.StartsWith("LOCAL="));
                if (string.IsNullOrWhiteSpace(localparams))
                    return;
                var param = localparams.Split('=')[1].Split(new[] {'|', '-', ','});
                FakeServer = param.Any(one => one == "GATEWAY");
                DoubleClick = param.Any(one => one == "DOUBLECLICK");
                VirtualHardWare = param.Any(one => one == "VIRTUALHARDWARE");
                VirtualThridPay = param.Any(one => one == "VIRTUALTHRIDPAY");
                IsDemo = param.Any(one => one == "DEMO");
                Local = FakeServer || DoubleClick || VirtualHardWare || VirtualThridPay;
            }
        }

        /// <summary>
        ///     标志是否有Local运行(任何一个Local运行都会导致为True)
        /// </summary>
        public static bool Local { get; set; }

        /// <summary>
        ///     标志是否运行在本地FakeServer环境中
        /// </summary>
        public static bool FakeServer { get; set; }

        /// <summary>
        ///     标志是否启用双击模拟读卡，塞钱等操作
        /// </summary>
        public static bool DoubleClick { get; set; }

        /// <summary>
        /// 标志是否运行在虚拟硬件环境中
        /// </summary>
        public static bool VirtualHardWare { get; set; }

        /// <summary>
        /// 标志是否模拟第三方交易
        /// </summary>
        public static bool VirtualThridPay { get; set; }

        /// <summary>
        /// 是否处于演示模式，在演示模式中不显示“测试环境”等Logo
        /// </summary>
        public static bool IsDemo { get; set; }

        /// <summary>
        /// 数据文件根目录 (暂未实现)
        /// </summary>
        public static string RootDirectory { get; set; }

        /// <summary>
        ///     医院程序集
        /// </summary>
        public static string HospitalAssembly { get; set; }

        /// <summary>
        ///     医院Id，医院名称
        /// </summary>
        public static string HospitalName { get; set; } = "青岛市口腔医院";


        /// <summary>
        ///     医院Id，该Id仅用于与前置网关数据交互
        /// </summary>
        public static string HospitalId { get; set; }

        /// <summary>
        ///     院区Id，用于区分多个院区
        /// </summary>
        public static string HospitalAreaCode { get; set; }

        /// <summary>
        ///     操作员Id，该Id是医院内部HIS分配的用户名
        /// </summary>
        public static string OperatorId { get; set; }

        /// <summary>
        ///     操作员名称，该Id是医院内部HIS分配的用户名称
        /// </summary>
        public static string OperatorName { get; set; }

        /// <summary>
        ///     右下角提示信息、通常为客服电话
        /// </summary>
        public static string NotificMessage { get; set; }

        /// <summary>
        ///     前置网关访问Url
        /// </summary>
        public static string GatewayUrl { get; set; }

        /// <summary>
        ///     机器型号，与自助机铭牌上一致
        /// </summary>
        public static string DeviceType { get; set; }

        /// <summary>
        ///     本地数据存储路径
        /// </summary>
        public static string DatabasePath { get; set; }

        public static string DataMiningDataBasePath { get; set; } = "Data\\DataMining.db";

        /// <summary>
        ///     是否在单元测试环境中
        /// </summary>
        public static bool UnitTest { get; set; }

        /// <summary>
        /// 是否开启语音提示
        /// </summary>
        public static bool AudioGuideEnabled { get; set; }

        /// <summary>
        /// 是否开启转场动画
        /// </summary>
        public static bool AnimationEnabled { get; set; }

        /// <summary>
        ///     组播的IP地址
        /// </summary>
        public static string MultiCastIP { get; set; }

        /// <summary>
        ///     组播的端口号
        /// </summary>
        public static string MultiCastPort { get; set; }

        /// <summary>
        ///     默认的每个界面的倒计时
        /// </summary>
        public static int DefaultTimeOut { get; set; }

        /// <summary>
        /// 是否允许自动关机
        /// </summary>
        public static bool AutoShutdownEnable { get; set; }

        /// <summary>
        /// 自动关机时间
        /// </summary>
        public static DateTime AutoShutdownTime { get; set; }

        /// <summary>
        /// 是否允许自动更新
        /// </summary>
        public static bool AutoUpdateEnable { get; set; }

        /// <summary>
        ///监控平台地址 
        /// </summary>
        public static string MonitorUrl { get; set; }

        /// <summary>
        /// 本机IP地址
        /// </summary>
      //  public static string LocalIp { get; set; }

        /// <summary>
        /// 本机Mac地址
        /// </summary>
     //   public static string LocalMac { get; set; }

        /// <summary>
        /// 当前项目的资源策略
        /// </summary>
        public static string[] Strategies { get; set; }

      
        /// <summary>
        /// 用户管理接口地址
        /// </summary>
        public static string UserCenterUrl { get; set; }

        /// <summary>
        /// 预检分诊接口地址
        /// </summary>
        public static string TriageUrl { get; set; }

        /// <summary>
        /// 分诊地址
        /// </summary>
        public static string DeviceUrl { get; set; }

        /// <summary>
        /// 当前主题
        /// </summary>
        public static Theme CurrentTheme { get; set; } = Theme.Default;

        /// <summary>
        /// 医联体ID
        /// </summary>
        public static string UnionId { get; set; }

        /// <summary>
        /// 用户管理Token
        /// </summary>
        public static string Token { get; set; }

        /// <summary>
        /// 第三方支付多账号区分Code(具体值由平台分配)
        /// </summary>
        public static string FundCustodian { get; set; }
        /// <summary>
        /// 是否上传交易流水
        /// </summary>
        public static bool EnableUploadTradeInfo { get; set; }

        /// <summary>
        /// 防止Terminal多开的互斥锁
        /// </summary>
        public static Mutex MutexLock;
    }
}