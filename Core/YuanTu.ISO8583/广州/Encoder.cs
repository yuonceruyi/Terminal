using System;
using System.Text;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.广州
{
    internal class Encoder : IO.Encoder
    {
        public override void Field(StringBuilder sb, int id, string data)
        {
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

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}