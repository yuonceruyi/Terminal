using System.Collections.Generic;


namespace YuanTu.JiaShanHospital.ISO8583.CPUCard
{
    public class CPUEncoder
    {
        public List<CPUTlv> Tlvs { get; set; }

        public byte[] Encode()
        {
            var list = new List<byte>();
            foreach (var tlv in Tlvs)
                Encode(list, tlv);
            return list.ToArray();
        }

        public void Encode(List<byte> list, CPUTlv package)
        {
            if (package.Constructed)
            {
                var innerList = new List<byte>();
                foreach (var tlv in package.InnerTlvs)
                    Encode(innerList, tlv);
                package.Value = innerList.ToArray();
            }
            int tag = package.Tag;
            if (tag < 0x100)
                list.Add((byte) tag);
            else
            {
                list.Add((byte)(tag >> 8));
                list.Add((byte)(tag & 0xFF));
            }
            var len = package.Value.Length;
            package.Length = len;
            if (len < 128)
            {
                list.Add((byte) len);
            }
            else
            {
                var n = 0;
                var bytes = new byte[4];
                while (len > 0)
                {
                    bytes[n] = (byte) (len%0x100);
                    len = len/0x100;
                    n++;
                }
                list.Add((byte) (0x80 + n)); //最高位为1 后7位为后面长度的字节数
                for (var i = 0; i < n; i++)
                    list.Add(bytes[i]);
            }
            list.AddRange(package.Value);
        }
    }
}