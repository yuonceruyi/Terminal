namespace YuanTu.ShenZhenArea.Nv200.ITLlib
{
    public class SSP_COMMAND
    {
        public int BaudRate = 9600;
        public byte[] CommandData = new byte[byte.MaxValue];
        public byte CommandDataLength;
        public string ComPort;
        public uint encPktCount;
        public bool EncryptionStatus;
        public SSP_FULL_KEY Key = new SSP_FULL_KEY();
        public byte[] ResponseData = new byte[byte.MaxValue];
        public byte ResponseDataLength;
        public PORT_STATUS ResponseStatus;
        public byte RetryLevel = 3;
        public byte SSPAddress;
        public byte sspSeq;
        public uint Timeout = 500;
    }
}