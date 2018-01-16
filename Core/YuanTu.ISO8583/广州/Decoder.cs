using System;
using System.Linq;
using System.Text;
using YuanTu.ISO8583.CPUCard;
using YuanTu.ISO8583.Data;

namespace YuanTu.ISO8583.广州
{
    public class Decoder : IO.Decoder
    {
        public Decoder()
        {
            OnLog += On48;
            OnLog += On55;
            OnLog += On62;
        }

        protected void On48(Message Message, ref byte[] data, StringBuilder sb)
        {
            if (Message.Fields.ContainsKey(48)
                && (Message.MessageType != "0800")
                && (Message.MessageType != "0810"))
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
        }

        protected void On55(Message Message, ref byte[] data, StringBuilder sb)
        {
            if (Message.Fields.ContainsKey(55))
            {
                sb.AppendLine();
                sb.AppendLine("[ 55]");
                var d = Message.Fields[55];
                var cpuDecoder = new CPUDecoder {OmitLog = true}.Decode(data, d.DataIndex/2, d.Length);
                Message.ICPackages = cpuDecoder.Tlvs.ToDictionary(one => one.Tag);
                sb.AppendLine(cpuDecoder.LogEntry.ToString());
            }
        }

        protected void On62(Message Message, ref byte[] data, StringBuilder sb)
        {
            if (Message.Fields.ContainsKey(62)
                && ((Message.MessageType == "0810") || (Message.MessageType == "0830")))
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
        }
    }
}