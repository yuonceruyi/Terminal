using YuanTu.Consts.FrameworkBase;
using YuanTu.Devices.CardReader;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.IO;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IManager
    {
        IPOS POS { get; set; }
        IICPOS IcPos { get; set; }
        IMagCardReader MagCardReader { get; set; }
        IIcCardReader IcCardReader { get; set; }
        Result<string> DoDownloadMasterKey(string sn);
        Result<byte[]> DoLogon();
        Result<Output> DoReverse();
        Result<Output> DoSale(Input input);
        Result Initialize();
        Result ReadCard(Input input);
    }
}