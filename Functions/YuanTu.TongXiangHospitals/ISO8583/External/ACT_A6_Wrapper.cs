using System;

namespace YuanTu.TongXiangHospitals.ISO8583.External
{
    public class ACT_A6_Wrapper : ICardDevice
    {
        public bool Init(int port, int baud)
        {
            ACT_A6_V2.Port = port;
            ACT_A6_V2.Baud = baud;
            return ACT_A6_V2.Init();
        }

        public bool Uninit()
        {
            return ACT_A6_V2.Uninit();
        }

        public bool EnterCard(bool allow)
        {
            return ACT_A6_V2.EnterCard(allow ? ACT_A6_V2.FCI.允许 : ACT_A6_V2.FCI.禁止);
        }

        public bool CheckCard(out CardPos pos)
        {
            pos = CardPos.未知;
            ACT_A6_V2.CardPos aPos;
            var ret = ACT_A6_V2.CheckCardPos(out aPos);
            if (!ret)
                return false;
            switch (aPos)
            {
                case ACT_A6_V2.CardPos.未知:
                    pos = CardPos.未知;
                    break;

                case ACT_A6_V2.CardPos.长卡:
                    pos = CardPos.未知;
                    break;

                case ACT_A6_V2.CardPos.短卡:
                    pos = CardPos.未知;
                    break;

                case ACT_A6_V2.CardPos.不持卡位:
                    pos = CardPos.不持卡位;
                    break;

                case ACT_A6_V2.CardPos.持卡位:
                    pos = CardPos.持卡位;
                    break;

                case ACT_A6_V2.CardPos.停卡位:
                    pos = CardPos.停卡位;
                    break;

                case ACT_A6_V2.CardPos.IC位:
                    pos = CardPos.IC位;
                    break;

                case ACT_A6_V2.CardPos.后端持卡位:
                    pos = CardPos.未知;
                    break;

                case ACT_A6_V2.CardPos.后端不持卡位:
                    pos = CardPos.未知;
                    break;

                case ACT_A6_V2.CardPos.无卡:
                    pos = CardPos.无卡;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        public bool MoveCard()
        {
            return ACT_A6_V2.MoveCard(ACT_A6_V2.MOVE_TO.IC);
        }

        public bool MoveCardTo非接()
        {
            return ACT_A6_V2.MoveCard(ACT_A6_V2.MOVE_TO.非接);
        }

        public bool ReadTracks(out string track2, out string track3)
        {
            string[] tracks;
            var ret = ACT_A6_V2.ReadTracks(new[] {2, 3}, out tracks);
            if (!ret)
            {
                track2 = track3 = string.Empty;
                return false;
            }
            track2 = tracks[0];
            track3 = tracks[1];
            return true;
        }

        public bool EjectCard()
        {
            return ACT_A6_V2.MoveCard(ACT_A6_V2.MOVE_TO.前端不持卡);
        }

        public bool CPUChipIO(bool t0, byte[] input, out byte[] output)
        {
            return ACT_A6_V2.CpuTransmit(
                t0 ? ACT_A6_V2.ICC_PROTOCOL.T0 : ACT_A6_V2.ICC_PROTOCOL.T1,
                input, out output);
        }

        public bool CPUCodeReset(out byte[] apdu)
        {
            return ACT_A6_V2.CpuColdReset(out apdu);
        }
    }
}