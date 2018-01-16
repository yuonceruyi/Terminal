using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.QDArea.QueueSignIn
{
    public class NetHelper
    {
        private static string _ip;
        private static string _mac;
        public static string IP => string.IsNullOrEmpty(_ip) ? (_ip = LocalIPAddress()) : _ip;
        public static string MAC => string.IsNullOrEmpty(_mac) ? (_mac = GetLocalMac()) : _mac;

        public static string LocalIPAddress()
        {
            string ip = CommandLine.ParseCmd("IP", string.Empty);
            if (!string.IsNullOrEmpty(ip))
                return ip;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in host.AddressList)
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    return ipAddress.ToString();
            return string.Empty;
        }

        public static string GetLocalMac()
        {
            string mac = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                    mac = mo["MacAddress"].ToString();
            }
            return (mac);
        }
        public static string PhysicalAddress()
        {
            string mac = CommandLine.ParseCmd("MAC", string.Empty);
            if (!string.IsNullOrEmpty(mac))
                return mac;

            var nics = NetworkInterface.GetAllNetworkInterfaces();
            if (nics.Length < 1)
                return null;
            var address = nics[0].GetPhysicalAddress();
            var bytes = address.GetAddressBytes();
            return string.Join(":", bytes.Select(one => one.ToString("X2")));
        }
    }
}
