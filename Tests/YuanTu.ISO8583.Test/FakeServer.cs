using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.Test
{
    internal class FakeServer
    {
        private readonly TcpListener listener;

        public FakeServer(IPAddress address, int port)
        {
            listener = new TcpListener(address, port);
        }

        public void Start()
        {
            Task.Factory.StartNew(async () =>
            {
                listener.Start(10);
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    await Task.Factory.StartNew(Handle, client);
                }
            });
        }

        private void Handle(object o)
        {
            var client = o as TcpClient;
            if (o == null)
                return;
            var lenlen = 2;
            var len = new byte[lenlen];
            var s = client.GetStream();
            s.Read(len, 0, 2);

            var length = int.Parse(len.Bytes2Hex(), NumberStyles.HexNumber);

            var recv = new byte[length + 2];
            Array.Copy(len, recv, lenlen);
            s.Read(recv, lenlen, length);
            byte[] send;
            switch (recv[13])
            {
                case 0x08:
                    send = "007960000005106031000000000810003800010AC00014000133091942042808000958403039313934323135303036373030353834303230303133303234343033383036323131303000110000000700300040CDF4F5FDFC4FB62E3533A16D1555B7DD342571BFA0F1F1B1213B2DE2000000000000000081D7E011".Hex2Bytes();
                    break;

                case 0x02:
                    send = "00A360000005106031000000000210703E02810AD08213166225000000000014000000000000000001000134091947042815120427000100080009584030393139343731343530363030303538343032303031333032343430333830363231313030223930333130303030202020303330323538343020202031353600179F3602031E910A42FE066E2918B088303000132200000700050000034355503531443837343145".Hex2Bytes();
                    break;

                case 0x04:
                    send = "009060000005106031000000000410703A02810AD0821116622500000000001400000000000000000100013409194704280427000100080009584030393139343831363030353830303538343032303031333032343430333830363231313030223930333130303030202020303330323538343020202031353600059F3602031E0013220000070005003346433531374642".Hex2Bytes();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            s.Write(send, 0, send.Length);
        }
    }
}