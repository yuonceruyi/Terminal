using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace YuanTu.Core.Systems
{
    public static class NetworkManager
    {
        private static string _ip;
        private static string _mac;
        private static byte[] _macbts;
        public static string IP => string.IsNullOrEmpty(_ip) ? (_ip = LocalIpAddress()) : _ip;
        public static string MAC => string.IsNullOrEmpty(_mac) ? (_mac = PhysicalAddress()) : _mac;

        public static byte[] MacBytes => _macbts?.Length>=6? _macbts:(_macbts = PhysicalAddressBytes());

        public static string LocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork&&!Equals(ip, IPAddress.Loopback))
                    return ip.ToString();
            return "";
        }

        public static string PhysicalAddress()
        {
            var bytes = PhysicalAddressBytes();
            return string.Join("-", bytes.Select(one => one.ToString("X2")));
        }
        public static byte[] PhysicalAddressBytes()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            if (nics.Length < 1)
                return new byte[0];
            var address = nics[0].GetPhysicalAddress();
            var bytes = address.GetAddressBytes();
            return bytes;
        }
    }
}
