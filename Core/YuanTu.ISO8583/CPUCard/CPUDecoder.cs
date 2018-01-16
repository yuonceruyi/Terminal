using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.CPUCard
{
    public class CPUDecoder
    {
        private byte[] _data;
        public bool OmitLog { get; set; }
        public StringBuilder LogEntry { get; set; }

        public ILog Log { get; set; } = POSLogger.Main;

        public List<CPUTlv> Tlvs { get; private set; }

        public Dictionary<int, CPUTlv> Dictionary { get; set; }

        public static Dictionary<int, string> TagNames { get; set; }

        public CPUDecoder Decode(byte[] data, int index, int length, Dictionary<int, CPUTlv> dictionary = null)
        {
            var bytes = new byte[length];
            Array.Copy(data, index, bytes, 0, length);
            return Decode(bytes, dictionary);
        }

        public CPUDecoder Decode(byte[] data, Dictionary<int, CPUTlv> dictionary = null)
        {
            LogEntry = new StringBuilder();
            if (!OmitLog)
                LogEntry.AppendLine();
            _data = data;
            Tlvs = new List<CPUTlv>();
            Dictionary = dictionary ?? new Dictionary<int, CPUTlv>(8);
            var pos = 0;
            try
            {
                while (pos < data.Length)
                    Tlvs.Add(Decode(ref pos));
            }
            finally
            {
                if (!OmitLog)
                    Log.Debug(LogEntry.ToString());
            }
            return this;
        }

        public CPUTlv Decode(ref int pos, int level = 1)
        {
            var package = new CPUTlv();
            package.Decoder = this;

            package.BeginIndex = pos;
            package.Tag = ParseTag(ref pos, ref _data);
            package.LengthIndex = pos;
            var len = ParseLength(ref pos, ref _data);
            package.Length = len;

            package.ValueIndex = pos;
            package.Value = new byte[len];
            Array.Copy(_data, pos, package.Value, 0, len);
            pos += len;
            package.EndIndex = pos;

            var name = string.Empty;
            var tag = package.Tag;
            if (TagNames.ContainsKey(tag))
                name = TagNames[tag];

            LogEntry.AppendLine(
                $"{"".PadLeft(level, '\t')}[{tag.ToString("X").PadRight(4)}]=[{package.Length.ToString().PadLeft(4)}][{(package.Constructed ? "+" : package.Value.Bytes2Hex())}][{name}]");

            if (package.Constructed)
            {
                var list = new List<CPUTlv>();
                var inPos = package.ValueIndex;
                while (inPos < package.EndIndex)
                    list.Add(Decode(ref inPos, level + 1));
                package.InnerTlvs = list;
            }
            Dictionary[package.Tag] = package;
            return package;
        }

        public static int ParseTag(ref int pos, ref byte[] data)
        {
            var tag = 0;
            if ((data[pos] & 0x1F) == 0x1F) //Tag长度为2
            {
                tag = data[pos]*0x100 + data[pos + 1];
                pos += 2;
            }
            else
            {
                tag = data[pos];
                pos += 1;
            }
            return tag;
        }

        public static int ParseLength(ref int pos, ref byte[] data)
        {
            var len = 0;
            if (data[pos] > 0x80) //首位为1 后续字节为长度
            {
                var n = data[pos] - 0x80;
                for (var i = 0; i < n; i++)
                    len = len*0x100 + data[pos + i + 1];
                pos += n + 1;
            }
            else
            {
                len = data[pos];
                pos += 1;
            }
            return len;
        }
    }
}