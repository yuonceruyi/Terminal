using System.Collections.Generic;
using log4net;
using YuanTu.ISO8583.Data;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IDecoder
    {
        IBuildConfig BuildConfig { get; set; }
        ILog Log { get; set; }
        Message Message { get; }

        List<int> Bitmap(ref int pos);
        Message Decode(byte[] data);
        FieldData Field(ref int pos, int id);
        int FieldLength(ref int pos, int len);
        TlvPackageData SingleTlv(ref int pos);
        List<TlvPackageData> Tlv(ref int pos, int len);
    }
}