using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Core.Log;
using YuanTu.ShengZhouHospital.ISO8583.CPUCard;
using YuanTu.ShengZhouHospital.ISO8583.Data;
using YuanTu.ShengZhouHospital.ISO8583.Enums;

namespace YuanTu.ShengZhouHospital.ISO8583
{
    public class Decoder
    {
        private byte[] _data;
        private string _s;

        public Message Message { get; protected set; }

        public BuildConfig BuildConfig { get; set; } = BuildConfig.DefaultConfig;


        public Message Decode(byte[] data)
        {
            _data = data;
            _s = data.Bytes2Hex();
            var pos = 0;
            Message = new Message();
            var sb = new StringBuilder();
            try
            {
                #region Length

                string lengthText;
                switch (BuildConfig.LengthFormat)
                {
                    case VarFormat.BCD:
                        lengthText = _s.Substring(pos, 4);
                        Message.Length = int.Parse(lengthText, System.Globalization.NumberStyles.HexNumber);//Convert.ToInt32(lengthText);
                        pos += 4;
                        break;

                    case VarFormat.ASCII:
                        lengthText = Encoding.ASCII.GetString(_data, pos/2, 4);
                        Message.Length = Convert.ToInt32(lengthText);
                        pos += 8;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                sb.AppendLine($"Length:{Message.Length}");

                #endregion Length

                #region TPDU

                if (!BuildConfig.OmitTPDU)
                {
                    Message.TPDU = _s.Substring(pos, 10);
                    pos += 10;
                    sb.AppendLine($"TPDU  :{Message.TPDU}");
                }

                #endregion TPDU

                #region Head

                if (!BuildConfig.OmitHead)
                {
                    Message.Head = _s.Substring(pos, 12);
                    pos += 12;
                    sb.AppendLine($"Head  :{Message.Head}");
                }

                #endregion Head

                #region MTI

                switch (BuildConfig.MTIFormat)
                {
                    case VarFormat.BCD:
                        Message.MessageType = _s.Substring(pos, 4);
                        pos += 4;
                        break;

                    case VarFormat.ASCII:
                        Message.MessageType = Encoding.ASCII.GetString(_data, pos/2, 4);
                        pos += 8;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                #endregion MTI

                var list = Bitmap(ref pos);
                sb.AppendLine(list.Aggregate("", (s, i) => s + " " + i));
                sb.AppendLine($"[  0][     MTI]=[   4][{Message.MessageType}]");
                sb.AppendLine($"[  1][  Bitmap]=[  { BuildConfig.DefaultConfig.BitmapLength}][{Message.Bitmap}]");
                foreach (var i in list.SkipWhile(i => i == 1))
                {
                    var d = Field(ref pos, i);
                    sb.AppendLine(
                        $"[{d.Id.ToString().PadLeft(3)}][{d.Name.PadLeft(8)}]=[{d.Length.ToString().PadLeft(4)}][{d.Text}]");
                    Message.Fields[i] = d;
                }

                #region Field 48

                var notAllow = new[] {"0800", "0810", "0420", "0430","0210"};
                if (Message.Fields.ContainsKey(48)&&!notAllow.Contains(Message.MessageType))
                {
                    sb.AppendLine("");
                    sb.AppendLine("[ 48]");
                    var d = Message.Fields[48];
                    var tlvPos = d.DataIndex;
                    var tlvs = Tlv(ref tlvPos, d.Length);
                    Message.Packages = tlvs.ToDictionary(one => one.Tag);
                    foreach (var tlv in tlvs)
                        sb.AppendLine(
                            $"\t[{tlv.Tag.ToString("X").PadRight(4)}]=[{tlv.Length.ToString().PadLeft(4)}][{tlv.Value}]");
                }

                #endregion Field 48

                #region Field 55

                if (Message.Fields.ContainsKey(55))
                {
                    sb.AppendLine();
                    sb.AppendLine("[ 55]");
                    var d = Message.Fields[55];
                    var datas = d.Text.Hex2Bytes();
                    var cpuDecoder = new CPUDecoder{OmitLog = true}.Decode(datas, 0, d.Length);
                    Message.ICPackages = cpuDecoder.Tlvs.ToDictionary(one => one.Tag);
                    sb.AppendLine(cpuDecoder.LogEntry.ToString());
                }

                #endregion Field 55

                #region Field 62

                if (Message.Fields.ContainsKey(62)
                    && (Message.MessageType == "0810" || Message.MessageType == "0830"))
                {
                    sb.AppendLine();
                    sb.AppendLine("[ 62]");
                    var d = Message.Fields[62];
                    var tlvPos = d.DataIndex;
                    var state = string.Empty;
                    switch (data[tlvPos/2])
                    {
                        case 0x30:
                            state = "No";
                            break;

                        case 0x31:
                            state = "In one";
                            break;

                        case 0x32:
                            state = "Has more";
                            Message.HasMore = true;
                            break;

                        case 0x33:
                            state = "No more";
                            break;
                    }
                    sb.AppendLine($"[State]=[{state}]");
                    tlvPos += 2;
                    var tlvs = Tlv(ref tlvPos, (d.Length - 1)*2);
                    Message.PackageList = tlvs.Select(one => new Tuple<int, TlvPackageData>(one.Tag, one)).ToList();
                    foreach (var tlv in tlvs)
                    {
                        var name = string.Empty;
                        if (CPUDecoder.TagNames.ContainsKey(tlv.Tag))
                            name = CPUDecoder.TagNames[tlv.Tag];
                        sb.AppendLine(
                            $"\t[{tlv.Tag.ToString("X").PadRight(4)}]=[{tlv.Length.ToString().PadLeft(4)}][{tlv.Value}][{name}]");
                    }
                }

                #endregion Field 62

                sb.AppendLine();
            }
            finally
            {
                Logger.POS.Debug(sb.ToString());
            }
            return Message;
        }

        public List<int> Bitmap(ref int pos)
        {
            var bPos = pos/2;
            var bLen = BuildConfig.BitmapLength/8;
            Message.Bitmap = _s.Substring(pos, bLen*2);

            var list = new List<int>();
            for (var i = 0; i < bLen; i++)
            {
                var b = _data[bPos + i];
                for (var j = 0; j < 8; j++)
                    if ((b >> (7 - j) & 0x01) == 1)
                        list.Add(8*i + j + 1);
            }
            pos += bLen*2;
            return list;
        }

        /// <summary>
        ///     解析域长度
        /// </summary>
        /// <param name="pos">当前位置</param>
        /// <param name="len">十进制宽度</param>
        /// <returns></returns>
        public int FieldLength(ref int pos, int len)
        {
            string lengthText;
            switch (BuildConfig.VarFormat)
            {
                case VarFormat.BCD:
                    var rLen = (len + 1)/2*2;
                    lengthText = _s.Substring(pos, rLen);
                    pos += rLen;
                    break;
                case VarFormat.ASCII:
                    lengthText = Encoding.ASCII.GetString(_data, pos/2, len);
                    pos += len*2;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Convert.ToInt32(lengthText);
        }

        public FieldData Field(ref int pos, int id)
        {
            var f = BuildConfig.Fields[id];
            var d = new FieldData
            {
                Id = id,
                Name = f.Name,
                BeginIndex = pos
            };
            int len;
            switch (f.VarType)
            {
                case VarType.Fixed:
                    d.Length = f.Length;
                    d.DataIndex = pos;
                    switch (f.Format)
                    {
                        case Format.BCD:
                            len = (d.Length + 1) / 2 * 2;
                            if (len % 2 == 1)
                                pos++;
                            d.Text = _s.Substring(pos, len).Replace('D', '=');
                            pos += len;
                            break;

                        case Format.ASCII:
                            d.Text = Encoding.Default.GetString(_data, pos/2, f.Length);
                            pos += f.Length*2;
                            break;

                        case Format.Binary:
                            len = f.Length/4;
                            d.Text = _s.Substring(pos, len);
                            pos += len;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case VarType.LLVar:
                    d.Length = FieldLength(ref pos, 2);
                    d.DataIndex = pos;
                    switch (f.Format)
                    {
                        case Format.BCD:
                            len = (d.Length + 1) / 2 * 2;
                            if (len % 2 == 1)
                                pos++;
                            d.Text = _s.Substring(pos, len).Replace('D', '=');
                            pos += len;
                            break;

                        case Format.ASCII:
                            len = d.Length;
                            d.Text = Encoding.Default.GetString(_data, pos/2, len);
                            pos += len*2;
                            break;

                        case Format.Binary:
                            len = d.Length/2;
                            d.Text = _s.Substring(pos, len);
                            pos += len;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case VarType.LLLVar:
                    d.Length = FieldLength(ref pos, 3);
                    d.DataIndex = pos;
                    switch (f.Format)
                    {
                        case Format.BCD:
                            len = (d.Length + 1)/2*2;
                            if (len%2 == 1)
                                pos++;
                            d.Text = _s.Substring(pos, len).Replace('D', '=');
                            pos += len;
                            break;

                        case Format.ASCII:
                            len = d.Length;
                            d.Text = Encoding.Default.GetString(_data, pos/2, len);
                            pos += len*2;
                            break;

                        case Format.Binary:
                            len = d.Length*2;
                            d.Text = _s.Substring(pos, len);
                            pos += len;
                            break;
                        case Format.Hex:
                            len = d.Length;
                            d.Text = _data.Skip(pos/2).Take(len).ToArray().Bytes2Hex();// Encoding.Default.GetString(_data, pos / 2, len); 
                             pos += len * 2;
                            break;
                        case Format.Raw:
                            len = d.Length;
                            d.Text = _s.Substring(pos, len);
                            pos += len;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            d.EndIndex = pos;
            return d;
        }

        public List<TlvPackageData> Tlv(ref int pos, int len)
        {
            var list = new List<TlvPackageData>();
            var max = pos + len;
            while (pos < max)
                list.Add(SingleTlv(ref pos));
            return list;
        }

        public TlvPackageData SingleTlv(ref int pos)
        {
            var package = new TlvPackageData();
            package.Tag = _data[pos/2];
            pos += 2;
            if ((package.Tag & 0x0F) == 0x0F) // 低4位为1111 'F' 表示Tag占2字节
            {
                package.Tag = package.Tag*0x100 + _data[pos/2];
                pos += 2;
            }
            int len;
            if (_data[pos/2] < 127)
            {
                package.Length = _data[pos/2];
                pos += 2;
            }
            else
            {
                var n = _data[pos/2] - 128;
                len = 0;
                for (var i = 0; i < n; i++)
                    len = len*0x100 + _data[pos/2 + 1 + i];
                package.Length = len;
                pos += 2*n + 2;
            }
            var format = Format.Binary;
            if (BuildConfig.Packages.ContainsKey(package.Tag))
                format = BuildConfig.Packages[package.Tag].Format;
            switch (format)
            {
                case Format.BCD:
                    len = package.Length + package.Length%2;
                    package.Value = _s.Substring(pos + package.Length%2, package.Length);
                    break;

                case Format.ASCII:
                    len = package.Length*2;
                    package.Value = Encoding.Default.GetString(_data, pos/2, package.Length);
                    break;

                case Format.Binary:
                    len = package.Length*2;
                    package.Value = _s.Substring(pos, len);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            pos += len;
            return package;
        }
    }
}