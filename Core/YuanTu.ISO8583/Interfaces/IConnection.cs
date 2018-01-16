using log4net;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IConnection
    {
        IBuildConfig BuildConfig { get; set; }
        IConfig Config { get; set; }
        ILog Log { get; set; }
        int TimeOut { get; set; }

        Result<byte[]> Handle(byte[] send);
    }
}