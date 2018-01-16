namespace YuanTu.JiaShanHospital.ISO8583.External
{
    public interface ICardDevice
    {
        bool Init(int port, int baud);

        bool Uninit();

        bool EnterCard(bool allow);

        bool CheckCard(out CardPos pos);

        bool MoveCard();

        bool MoveCardTo非接();
        bool ReadTracks(out string track2, out string track3);

        bool EjectCard();

        bool CPUChipIO(bool t0, byte[] input, out byte[] output);

        bool CPUCodeReset(out byte[] apdu);
    }

    public enum CardPos
    {
        未知,
        不持卡位,
        持卡位,
        停卡位,
        IC位,
        无卡
    }
}