using YuanTu.Consts.FrameworkBase;

namespace YuanTu.ISO8583.Interfaces
{
    public interface ILoader : IService
    {
        IManager Initialize(IConfig config);
    }
}