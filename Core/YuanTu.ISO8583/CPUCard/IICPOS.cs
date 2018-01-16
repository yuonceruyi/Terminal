using System;
using System.Collections.Generic;
using log4net;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ISO8583.IO;

namespace YuanTu.ISO8583.CPUCard
{
    public interface IICPOS
    {
        ILog Log { get; set; }

        bool CompareTVR2IAC(byte[] tvr, byte[] iac);
        byte[] Int2BCD(int value, int len);

        void LoadDevice(DateTime now, int amount);
        bool OnlineFail();
        Result ReadCard(Input input, Func<byte[], Result<byte[]>> ioFunc);
        List<CPUTlv> FirstHalf();
        bool SecondHalf(Output output);

        List<Tuple<int, int>> ParseDOL(byte[] dol);
        byte[] MakeDOLData(List<Tuple<int, int>> list);

        void PrintDic(Dictionary<int, CPUTlv> dic);
        List<CPUTlv> MakeList(int[] indexes);
        List<CPUTlv> MakeNotifyList();
        void MakePrintList(Output output);
        List<CPUTlv> MakeUploadList();
    }
}