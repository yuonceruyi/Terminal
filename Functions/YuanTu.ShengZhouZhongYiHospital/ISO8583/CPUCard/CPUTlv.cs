using System.Collections.Generic;
using System.Linq;
using YuanTu.ShengZhouZhongYiHospital.ISO8583.Data;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583.CPUCard
{
    public class CPUTlv
    {
        public int Tag { get; set; }

        public int Length { get; set; }

        public byte[] Value { get; set; }

        public bool Constructed
        {
            get
            {
                byte t0 = (byte) (Tag > 0xFF ? Tag >> 16 : Tag);
                return (t0 & 0x20) == 0x20;
            }
        }

        public List<CPUTlv> InnerTlvs { get; set; }

        public CPUTlv this[int tag]
        {
            get { return InnerTlvs.FirstOrDefault(one => one.Tag == tag); }
        }

        public int BeginIndex { get; set; }

        public int LengthIndex { get; set; }

        public int ValueIndex { get; set; }

        public int EndIndex { get; set; }

        public CPUDecoder Decoder { get; set; }

        public static CPUTlv Convert(TlvPackageData data)
        {
            return new CPUTlv()
            {
                Tag = data.Tag,
                Length = data.Length,
                Value = data.Value.Hex2Bytes(),
            };
        }
    }
}