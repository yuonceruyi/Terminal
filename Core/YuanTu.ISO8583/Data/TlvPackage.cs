using YuanTu.ISO8583.Enums;

namespace YuanTu.ISO8583.Data
{
    public class TlvPackage
    {
        public int Tag { get; set; }

        public int Length { get; set; }

        public bool OmitLength { get; set; }

        public Format Format { get; set; }
    }
}