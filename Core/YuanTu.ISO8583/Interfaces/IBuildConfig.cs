using System.Collections.Generic;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IBuildConfig
    {
        int BitmapLength { get; set; }
        Dictionary<int, Field> Fields { get; set; }
        Format LengthBytesFormat { get; set; }
        int LengthBytesLength { get; set; }
        VarFormat LengthFormat { get; set; }
        int MACFieldId { get; set; }
        VarFormat MTIFormat { get; set; }
        bool OmitHead { get; set; }
        bool OmitTPDU { get; set; }
        Dictionary<int, TlvPackage> Packages { get; set; }
        VarFormat VarFormat { get; set; }
    }
}