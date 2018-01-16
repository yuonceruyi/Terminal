using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.ShengZhouHospital.ISO8583.CPUCard;
using YuanTu.ShengZhouHospital.ISO8583.Data;
using YuanTu.ShengZhouHospital.ISO8583.Enums;

namespace YuanTu.ShengZhouHospital.ISO8583
{
    public class Encoder
    {
        public string MessageType { get; set; }

        public Dictionary<int, string> Values { get; set; }

        public List<TlvPackageData> Packages { get; set; }

        public List<CPUTlv> ICPackages { get; set; } 

        public Config Config { get; set; } = Config.DefaultConfig;

        public BuildConfig BuildConfig { get; set; } = BuildConfig.DefaultConfig;

        public Func<byte[],byte[]> CalcMacFunc { get; set; } 

        public byte[] Encode()
        {
            var sb = new StringBuilder();
            TPDU(sb);
            Head(sb);
            MTI(sb);
            Bitmap(sb);
            foreach (var value in Values.OrderBy(kvp=>kvp.Key))
                if (value.Key == 48 && string.IsNullOrEmpty(value.Value))
                    Tlv(sb);
                else if (value.Key == 55 && string.IsNullOrEmpty(value.Value))
                    IC(sb);
                else
                    Field(sb, value.Key, value.Value);

            Length(sb);
            var data = sb.ToString().Hex2Bytes();
            if (Values.ContainsKey(BuildConfig.MACFieldId))
            {
                int lengthLen;
                switch (BuildConfig.LengthFormat)
                {
                    case VarFormat.BCD:
                        lengthLen = 2;
                        break;
                    case VarFormat.ASCII:
                        lengthLen = 4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var skipLen = lengthLen + (Config.TPDU?.Length ?? 0)/2;//Mac长度 + TPDU长度
                var totalLen = data.Length - skipLen - 8;//总长-忽略头-Mac长度
                
                var bytes = new byte[totalLen];
                Array.Copy(data, skipLen, bytes,0, totalLen);
                var mac = CalcMacFunc(bytes);
                Array.Copy(mac, 0, data, totalLen+skipLen, 8);
            }
            return data;
        }

        public void TPDU(StringBuilder sb)
        {
            if (BuildConfig.OmitTPDU)
                return;
            sb.Append(Config.TPDU.PadLeft(10, ' '));
        }

        public void Head(StringBuilder sb)
        {
            if (BuildConfig.OmitHead)
                return;
            sb.Append(Config.Head.PadLeft(12, ' '));
        }

        public void MTI(StringBuilder sb)
        {
            switch (BuildConfig.MTIFormat)
            {
                case VarFormat.BCD:
                    sb.Append(MessageType);
                    break;

                case VarFormat.ASCII:
                    sb.Append(Encoding.ASCII.GetBytes(MessageType).Bytes2Hex());
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Bitmap(StringBuilder sb)
        {
            var bLen = BuildConfig.BitmapLength/8;
            var bytes = new byte[bLen];
            foreach (var value in Values.OrderBy(one => one.Key))
            {
                var i = value.Key - 1;
                bytes[i/8] |= (byte) (1 << (7 - i%8));
            }
            sb.Append(bytes.Bytes2Hex());
        }

        public int Length(StringBuilder sb)
        {
            var length = sb.Length/2;
            switch (BuildConfig.LengthFormat)
            {
                case VarFormat.BCD:
                    sb.Insert(0, length.ToString("X4"));
                    break;

                case VarFormat.ASCII:
                    sb.Insert(0, Encoding.ASCII.GetBytes(length.ToString("D4")).Bytes2Hex());
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return length;
        }

        public string FieldLength(int len, VarType type)
        {
            switch (BuildConfig.VarFormat)
            {
                case VarFormat.BCD:
                    switch (type)
                    {
                        case VarType.Fixed:
                            return "";

                        case VarType.LLVar:
                            return len.ToString("D2");

                        case VarType.LLLVar:
                            return len.ToString("D4");

                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }

                case VarFormat.ASCII:
                    switch (type)
                    {
                        case VarType.Fixed:
                            return "";

                        case VarType.LLVar:
                            return Encoding.ASCII.GetBytes(len.ToString("D2")).Bytes2Hex();

                        case VarType.LLLVar:
                            return Encoding.ASCII.GetBytes(len.ToString("D3")).Bytes2Hex();

                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Field(StringBuilder sb, int id, string data,bool ignoreNotExist=false)
        {
            if (ignoreNotExist)
            {
                if (!BuildConfig.Fields.ContainsKey(id))
                {
                    sb.Append(data);
                    return;
                }
            }
            var f = BuildConfig.Fields[id];
            switch (f.Format)
            {
                case Format.BCD:
                    sb.Append(FieldLength(data.Length, f.VarType));
                    switch (f.VarType)
                    {
                        case VarType.Fixed:
                            sb.Append(data);
                            break;

                        case VarType.LLVar:
                            sb.Append(data);
                            break;

                        case VarType.LLLVar:
                            sb.Append(data);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (data.Length%2 == 1)
                        sb.Append("0");
                    break;

                case Format.ASCII:
                    sb.Append(FieldLength(data.Length, f.VarType));
                    switch (f.VarType)
                    {
                        case VarType.Fixed:
                            var bytes = Encoding.Default.GetBytes(data);
                            sb.Append(bytes.Bytes2Hex());
                            for (var i = 0; i < f.Length - bytes.Length; i++)
                                sb.Append("20");
                            break;

                        case VarType.LLVar:
                            sb.Append(Encoding.Default.GetBytes(data).Bytes2Hex());
                            break;

                        case VarType.LLLVar:
                            sb.Append(Encoding.Default.GetBytes(data).Bytes2Hex());
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case Format.Binary:
                    sb.Append(FieldLength(data.Length/2, f.VarType));
                    switch (f.VarType)
                    {
                        case VarType.Fixed:
                            sb.Append(data);
                            break;

                        case VarType.LLVar:
                            sb.Append(data);
                            break;

                        case VarType.LLLVar:
                            sb.Append(data);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case Format.Hex:
                    sb.Append(FieldLength(data.Length / 2, f.VarType));
                    switch (f.VarType)
                    {
                        case VarType.Fixed:
                            var bytes = Encoding.Default.GetBytes(data);
                            sb.Append(bytes.Bytes2Hex());
                            for (var i = 0; i < f.Length - bytes.Length; i++)
                                sb.Append("20");
                            break;

                        case VarType.LLVar:
                            sb.Append(data);
                            break;

                        case VarType.LLLVar:
                            sb.Append(data);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Tlv(StringBuilder sb)
        {
            var index = sb.Length;
            foreach (var data in Packages)
                SingleTlv(sb, data);
            // LLLVAR
            var len = ((sb.Length - index)/2);
            switch (BuildConfig.VarFormat)
            {
                case VarFormat.BCD:
                    sb.Insert(index, len.ToString("D4"));
                    break;
                case VarFormat.ASCII:
                    sb.Insert(index, Encoding.Default.GetBytes(len.ToString("D3")).Bytes2Hex());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SingleTlv(StringBuilder sb, TlvPackageData data)
        {
            var f = BuildConfig.Packages[data.Tag];
            sb.Append(data.Tag.ToString("X"));
            if (!f.OmitLength)
                sb.Append(data.Length.ToString("X2"));
            switch (f.Format)
            {
                case Format.BCD:
                    // BCD 奇数长度左补0
                    if (data.Length%2 == 1)
                        sb.Append("0");
                    sb.Append(data.Value);
                    break;

                case Format.ASCII:
                    sb.Append(Encoding.Default.GetBytes(data.Value).Bytes2Hex());
                    break;

                case Format.Binary:
                    sb.Append(data.Value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void IC(StringBuilder sb)
        {
            var encoder = new CPUEncoder {Tlvs = ICPackages};
            var data = encoder.Encode().Bytes2Hex();
            switch (BuildConfig.VarFormat)
            {
                case VarFormat.BCD:
                    sb.Append((data.Length/2).ToString("D4"));
                    break;
                case VarFormat.ASCII:
                    sb.Append(Encoding.Default.GetBytes((data.Length/2).ToString("D3")).Bytes2Hex());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            sb.Append(data);
        }
    }
}