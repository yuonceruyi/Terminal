using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;

namespace YuanTu.PanYu.House.PanYuService
{
    public  class ServiceBase
    {
        public string GetFlowId()
        {
            var bis = ServiceLocator.Current.GetInstance<IBusinessConfigManager>();
            return bis.GetFlowId("");
        }

        public string GetChnlSsn()
        {
            var bis = ServiceLocator.Current.GetInstance<IBusinessConfigManager>();
            var chnlSsn = bis.GetFlowId("");
            chnlSsn = chnlSsn.Substring(chnlSsn.Length - 6, 6);
            return $"{FrameworkConst.OperatorId}{DateTimeCore.Now.ToString("yyMMdd")}{chnlSsn}";
        }

       
    }
}
