using System;
using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.IO;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IPOS
    {
        IContainerWrapper ContainerWrapper { get; set; }
        Func<byte[], byte[]> CalcMacFunc { get; set; }
        IConfig Config { get; set; }
        IConnection Connection { get; set; }

        byte[] CalcMAC(byte[] data);
        bool CheckKeys(byte[] data);
        Result<string> DoDownloadMasterKey(string sn);
        Result<byte[]> DoLogon();
        Result<Output> DoNotifyIC(Input input, List<CPUTlv> list);
        Result<Output> DoReverse();
        Result<Output> DoReverse(Input input);
        Result<Output> DoReverseIC();
        Result<Output> DoReverseIC(Input input, List<CPUTlv> list);
        Result<Output> DoSale(Input input);
        Result<Output> DoSaleIC(Input input, List<CPUTlv> list);
        Result<Output> DoUploadIC(Input input, List<CPUTlv> list);
        Result DownloadParams();
        Result DownloadPubKey();
        string GetPan(string cardNo);
        byte[] GetPin(string cardNo, string pass);
    }
}