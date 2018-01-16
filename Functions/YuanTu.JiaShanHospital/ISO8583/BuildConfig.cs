using System.Collections.Generic;
using YuanTu.JiaShanHospital.ISO8583.Data;
using YuanTu.JiaShanHospital.ISO8583.Enums;

namespace YuanTu.JiaShanHospital.ISO8583
{
    public class BuildConfig
    {
        public int BitmapLength { get; set; } = 64;

        public VarFormat VarFormat { get; set; }

        public VarFormat LengthFormat { get; set; }

        public VarFormat MTIFormat { get; set; }

        public bool OmitTPDU { get; set; }

        public bool OmitHead { get; set; }

        public Dictionary<int, Field> Fields { get; set; }

        public Dictionary<int, TlvPackage> Packages { get; set; }

        public static BuildConfig DefaultConfig { get; set; }

        public int MACFieldId { get; set; }
    }
}