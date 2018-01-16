using System;
using System.Collections.Generic;
using YuanTu.NingXiaHospital.CardReader.BankCard.IO;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.NingXiaHospital.CardReader.BankCard.CPUCard
{
    public interface IICPOS
    {
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