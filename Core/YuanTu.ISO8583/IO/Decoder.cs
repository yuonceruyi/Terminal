using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using log4net;
using YuanTu.ISO8583.Data;
using YuanTu.ISO8583.Enums;
using YuanTu.ISO8583.Interfaces;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.IO
{
    public class Decoder : IDecoder
    {
        public delegate void OnLogdelegate(Message message, ref byte[] data, StringBuilder sb);

        protected byte[] _data;
        protected string _s;

        public Message Message { get; protected set; }

        public IBuildConfig BuildConfig { get; set; }

        public ILog Log { get; set; } = POSLogger.Main;

        public virtual Message Decode(byte[] data)
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
                        Message.Length = int.Parse(lengthText, NumberStyles.HexNumber);
                        pos += 4;
                        break;

                    case VarFormat.ASCII:
                        lengthText = Encoding.ASCII.GetString(_data, pos/2, 8);
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
                sb.AppendLine($"[  1][  Bitmap]=[  32][{Message.Bitmap}]");
                foreach (var i in list.SkipWhile(i => i == 1))
                {
                    var d = Field(ref pos, i);
                    sb.AppendLine(
                        $"[{d.Id.ToString().PadLeft(3)}][{d.Name.PadLeft(8)}]=[{d.Length.ToString().PadLeft(4)}][{d.Text}]");
                    Message.Fields[i] = d;
                }

                OnLog?.Invoke(Message, ref data, sb);

                sb.AppendLine();
            }
            finally
            {
                Log.Debug(sb.ToString());
            }
            return Message;
        }

        public virtual List<int> Bitmap(ref int pos)
        {
            var bPos = pos/2;
            var bLen = BuildConfig.BitmapLength/8;
            Message.Bitmap = _s.Substring(pos, bLen*2);

            var list = new List<int>();
            for (var i = 0; i < bLen; i++)
            {
                var b = _data[bPos + i];
                for (var j = 0; j < 8; j++)
                    if (((b >> (7 - j)) & 0x01) == 1)
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
        public virtual int FieldLength(ref int pos, int len)
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

        public virtual FieldData Field(ref int pos, int id)
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
                            len = f.Length;
                            if (len%2 == 1)
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
                            len = (d.Length + 1)/2*2;
                            d.Text = _s.Substring(pos, len).Replace('D', '=');
                            pos += len;
                            if (len%2 == 1)
                                pos++;
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
                        case Format.Hex:
                            len = d.Length;
                            d.Text = _data.Skip(pos / 2).Take(len).ToArray().Bytes2Hex();// Encoding.Default.GetString(_data, pos / 2, len); 
                            pos += len * 2;
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
                            d.Text = _s.Substring(pos, len).Replace('D', '=');
                            pos += len;
                            if (len%2 == 1)
                                pos++;
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
                            d.Text = _data.Skip(pos / 2).Take(len).ToArray().Bytes2Hex();// Encoding.Default.GetString(_data, pos / 2, len); 
                            pos += len * 2;
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

        public virtual List<TlvPackageData> Tlv(ref int pos, int len)
        {
            var list = new List<TlvPackageData>();
            var max = pos + len;
            while (pos < max)
                list.Add(SingleTlv(ref pos));
            return list;
        }

        public virtual TlvPackageData SingleTlv(ref int pos)
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

        public event OnLogdelegate OnLog;
    }
}