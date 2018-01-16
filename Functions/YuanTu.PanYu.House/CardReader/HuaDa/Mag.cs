using System;
using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.PanYu.House.CardReader.HuaDa
{
    public class Mag
    {
        public static int readerHandle => PanYu.House.CardReader.HuaDa.Common.readerHandle;
        private const string DllPathHuaDa = "External\\HuaDa\\SSSE32.dll";

        [DllImport(DllPathHuaDa, EntryPoint = "Rcard", CharSet = CharSet.Ansi)]
        public static extern int Rcard(int ReaderHandle, byte ctime, int track, ref byte rlen, byte[] getdata);

        [DllImport(DllPathHuaDa, EntryPoint = "Rcard", CharSet = CharSet.Ansi)]
        public static extern int Rcard(int ReaderHandle, byte ctime, int track, ref byte rlen, StringBuilder getdata);

        /// <summary>
        ///     读磁条卡磁道数据
        /// </summary>
        /// <param name="trackNo">磁道</param>
        /// <param name="data">返回磁道数据</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public static bool ReadMagCard(int trackNo, out string data, int timeOut = 3)
        {
            byte len = 0;
            var sb = new StringBuilder(256);
            var ret = Rcard(readerHandle, (byte)timeOut, trackNo, ref len, sb);
            Console.WriteLine(len);
            data = sb.ToString(0, len);
            if (data.StartsWith(";"))
            {
                data = data.Substring(1, data.Length - 3);
            }
            return ret == 0;
        }

        public static bool ReadAllTracks(out string Track1, out string Track2, out string Track3, int timeOut = 20)
        {
            byte len = 0;
            byte[] sb = new byte[1024];

            var ret = Rcard(readerHandle, (byte)timeOut, 4, ref len, sb);
            Console.WriteLine(len);

            var track1len = sb[2 - 1];
            var track2len = sb[2 + 2 + track1len - 1];
            var track3len = sb[2 + 2 + 2 + track1len + track2len - 1];

            if (track1len > 3)
            {
                var track1 = new byte[track1len - 3];
                Array.Copy(sb, 2 + 1, track1, 0, track1len - 3);
                Track1 = Encoding.ASCII.GetString(track1);
            }
            else
            {
                Track1 = null;
            }

            if (track2len > 3)
            {
                var track2 = new byte[track2len - 3];
                Array.Copy(sb, 2 + 2 + track1len + 1, track2, 0, track2len - 3);
                Track2 = Encoding.ASCII.GetString(track2);
            }
            else { Track2 = null; }

            if (track3len > 3)
            {
                var track3 = new byte[track3len - 3];
                Array.Copy(sb, 2 + 2 + 2 + track1len + track2len + 1, track3, 0, track3len - 3);
                Track3 = Encoding.ASCII.GetString(track3);
            }
            else { Track3 = null; }
            return ret == 0;
        }
    }
}