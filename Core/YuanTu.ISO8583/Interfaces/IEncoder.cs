using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;

namespace YuanTu.ISO8583.Interfaces
{
    public interface IEncoder
    {
        IBuildConfig BuildConfig { get; set; }
        Func<byte[], byte[]> CalcMacFunc { get; set; }
        IConfig Config { get; set; }
        List<CPUTlv> ICPackages { get; set; }
        string MessageType { get; set; }
        List<TlvPackageData> Packages { get; set; }
        Dictionary<int, string> Values { get; set; }

        void Bitmap(StringBuilder sb);
        byte[] Encode();
        void Field(StringBuilder sb, int id, string data);
        string FieldLength(int len, VarType type);
        void Head(StringBuilder sb);
        int Length(StringBuilder sb);
        void MTI(StringBuilder sb);
        void SingleTlv(StringBuilder sb, TlvPackageData data);
        void Tlv(StringBuilder sb);
        void TPDU(StringBuilder sb);
        void IC(StringBuilder sb);
    }
}