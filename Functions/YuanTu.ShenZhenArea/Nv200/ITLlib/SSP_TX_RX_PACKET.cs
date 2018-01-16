namespace YuanTu.ShenZhenArea.Nv200.ITLlib
{
    internal class SSP_TX_RX_PACKET
    {
        public byte CheckStuff;
        public bool NewResponse;
        public byte rxBufferLength;
        public byte[] rxData = new byte[byte.MaxValue];
        public byte rxPtr;
        public byte SSPAddress;
        public byte txBufferLength;
        public byte[] txData = new byte[byte.MaxValue];
        public byte txPtr;
    }
}