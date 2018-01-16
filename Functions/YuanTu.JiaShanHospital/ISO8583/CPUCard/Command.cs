using System.Collections.Generic;

namespace YuanTu.JiaShanHospital.ISO8583.CPUCard
{
    public abstract class Command
    {
        protected virtual byte CLA { get; }

        protected virtual byte INS { get; }

        protected virtual byte P1 { get; }

        protected virtual byte P2 { get; }

        protected virtual byte[] Data { get; set; }

        protected virtual byte Le { get; }

        protected virtual bool HasData => false;

        protected virtual bool HasLe => false;

        public virtual byte[] Make()
        {
            var list = new List<byte> {CLA, INS, P1, P2};
            if (HasData)
            {
                list.Add((byte) Data.Length);
                list.AddRange(Data);
            }
            if (HasLe)
                list.Add(Le);

            return list.ToArray();
        }
    }

    public class READ_RECORD : Command
    {
        protected override byte CLA => 0x00;

        protected override byte INS => 0xB2;

        protected override byte P1 => Id;

        protected override byte P2 => (byte) ((SF1 << 3) | 0x04);

        protected override bool HasData => false;

        protected override bool HasLe => true;

        public byte Id { get; set; }

        public byte SF1 { get; set; }
    }

    public class SELECT : Command
    {
        protected override byte CLA => 0x00;

        protected override byte INS => 0xA4;

        protected override byte P1 => (byte) (ByName ? 0x04 : 0x00);

        protected override byte P2 => (byte) (Next ? 0x02 : 0x00);

        protected override byte[] Data => Name;

        protected override bool HasData => true;

        protected override bool HasLe => false;

        public bool ByName { get; set; } = true;

        public bool Next { get; set; }

        public byte[] Name { get; set; }
    }

    public class GET_PROCESS_OPTIONS : Command
    {
        protected override byte CLA => 0x80;

        protected override byte INS => 0xA8;

        protected override byte P1 => 0x00;

        protected override byte P2 => 0x00;

        protected override byte[] Data => PDOL;

        protected override byte Le => 0x00;

        protected override bool HasData => true;

        protected override bool HasLe => true;

        public byte[] PDOL { get; set; }
    }

    public class GET_DATA : Command
    {
        protected override byte CLA => 0x80;

        protected override byte INS => 0xCA;

        protected override byte P1 => Tag[0];

        protected override byte P2 => (byte) (Tag.Length > 1 ? Tag[1] : 0x00);

        protected override byte Le => 0x00;

        protected override bool HasData => false;

        protected override bool HasLe => true;

        public byte[] Tag { get; set; }
    }

    public class GENERATE_AC : Command
    {
        protected override byte CLA => 0x80;

        protected override byte INS => 0xAE;

        protected override byte P1
        {
            get
            {
                var b = (byte) AcType;
                if (Req)
                    b |= 0x10;
                return b;
            }
        }

        protected override byte P2 => 0x00;

        protected override byte[] Data => Bytes;

        protected override byte Le => 0x00;

        protected override bool HasData => true;

        protected override bool HasLe => true;

        public bool Req { get; set; }

        public ACType AcType { get; set; }

        public byte[] Bytes { get; set; }
    }

    public enum ACType : byte
    {
        AAC = 0x00,
        TC = 0x40,
        ARQC = 0x80,
        RFU = 0xC0
    }

    public class EXTERNAL_AUTHENTICATE : Command
    {
        protected override byte CLA => 0x00;

        protected override byte INS => 0x82;

        protected override byte P1 => 0x00;

        protected override byte P2 => 0x00;

        protected override byte[] Data => Auth;

        protected override bool HasData => true;

        protected override bool HasLe => false;

        public byte[] Auth { get; set; }
    }

    public class INTERNAL_AUTHENTICATE : Command
    {
        protected override byte CLA => 0x00;

        protected override byte INS => 0x88;

        protected override byte P1 => 0x00;

        protected override byte P2 => 0x00;

        protected override byte[] Data => Auth;

        protected override bool HasData => true;

        protected override bool HasLe => true;

        public byte[] Auth { get; set; }
    }
}