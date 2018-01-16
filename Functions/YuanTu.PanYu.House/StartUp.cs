using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.PanYu.House.CardReader;
using YuanTu.PanYu.House.PanYuGateway;
using YuanTu.PanYu.House.PanYuService;

namespace YuanTu.PanYu.House
{
    public class Startup:YuanTu.Default.House.Startup
    {
        public override string[] UseConfigPath()
        {
            var file = Path.Combine(FrameworkConst.RootDirectory, "CurrentResource", "PanYuHouse", "PanYuHouseConfig.xml");
            return new[] { file };
        }
        public override void AfterStartup()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            ACT_A6_V2.Port = config.GetValueInt("Act_A6:Port");
            ACT_A6_V2.Baud = config.GetValueInt("Act_A6:Baud");

            InnerConfig.终端号 = config.GetValue("睿民:终端号");
            InnerConfig.商户代号 = config.GetValue("睿民:商户号");
            InnerConfig.柜员号 = config.GetValue("睿民:柜员号");

            DataHandler.HospCode = config.GetValue("HospCode");
            DataHandler.OperId = config.GetValue("OperId");
            DataHandler.HospitalId = "549";
            DataHandler.terminalNo = FrameworkConst.OperatorId;
            DataHandler.Uri = new Uri(FrameworkConst.GatewayUrl);

        }
    }
}
