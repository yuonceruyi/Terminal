using System;
using System.IO;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Systems;
using YuanTu.Core.Systems.Ini;

namespace YuanTu.Core.Services.IniService
{
    public class DefaultBusinessConfigManager : IBusinessConfigManager
    {
        private static readonly string CommonIniFile = Path.Combine(Environment.CurrentDirectory, "Common.ini");
        private static readonly IniFile Config = new IniFile(CommonIniFile);
        private static readonly IniString _flowDate = new IniString(Config, "Main", "FlowDate");
        private static readonly IniInteger _flowNum = new IniInteger(Config, "Main", "FlowNum");

        private static string FlowDate
        {
            get { return _flowDate.Value; }
            set { _flowDate.Value = value; }
        }

        private static int FlowNum
        {
            get { return _flowNum.Value; }
            set { _flowNum.Value = value; }
        }

        public string GetFlowId(string reason)
        {
            lock (CommonIniFile)
            {
                var date = DateTimeCore.Now.ToString("yyyyMMdd");
                var dateString = FlowDate;
                if (string.IsNullOrEmpty(dateString) || dateString != date)
                {
                    FlowDate = date;
                    FlowNum = 0;
                }
                var num = ++FlowNum;
                var bts = new byte[2];
                var mac = NetworkManager.MacBytes;
                Array.Copy(mac, 4, bts, 0, 2);
                var macIndex = BitConverter.ToUInt16(bts, 0);
                return $"{FrameworkConst.OperatorId}{date}{macIndex:D5}{num:D6}";
            }
        }

        public string ServiceName => "获取业务配置";
    }
}