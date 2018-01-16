using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    /// <summary>
    ///     提供所有与实际业务有关的跨终端配置服务
    /// </summary>
    public interface IBusinessConfigManager : IService
    {
        /// <summary>
        ///     获取相对流水号(OperId+Mac地址+年月日+6位流水)
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        string GetFlowId(string reason);
    }
}