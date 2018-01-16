using System.Collections.Generic;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.Interfaces;

namespace YuanTu.ISO8583
{
    public class BuildConfig : IBuildConfig
    {
        public int LengthBytesLength { get; set; }

        public Format LengthBytesFormat { get; set; }

        public int BitmapLength { get; set; } = 64;

        public VarFormat VarFormat { get; set; }

        public VarFormat LengthFormat { get; set; }

        public VarFormat MTIFormat { get; set; }

        public bool OmitTPDU { get; set; }

        public bool OmitHead { get; set; }

        public Dictionary<int, Field> Fields { get; set; }

        public Dictionary<int, TlvPackage> Packages { get; set; }

        //public static BuildConfig DefaultConfig { get; set; }

        public int MACFieldId { get; set; }
    }
}