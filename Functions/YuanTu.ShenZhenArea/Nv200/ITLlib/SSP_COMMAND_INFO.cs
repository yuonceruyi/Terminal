namespace YuanTu.ShenZhenArea.Nv200.ITLlib
{
    public class SSP_COMMAND_INFO
    {
        public SSP_PACKET Transmit = new SSP_PACKET();
        public SSP_PACKET Receive = new SSP_PACKET();
        public SSP_PACKET PreEncryptedTransmit = new SSP_PACKET();
        public SSP_PACKET PreEncryptedRecieve = new SSP_PACKET();
        public bool Encrypted;
    }
}