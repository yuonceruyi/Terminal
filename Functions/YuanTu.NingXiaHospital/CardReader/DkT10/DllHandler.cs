using System;
using System.Runtime.InteropServices;

namespace YuanTu.NingXiaHospital.CardReader.DkT10
{
    public class DllHandler
    {
        private const string DllPath = "External\\DK_T10\\dcrf32.dll";

        [DllImport(DllPath)]
        public static extern int dc_init(Int16 port, Int32 baud);  //初试化
        [DllImport(DllPath)]
        public static extern short dc_exit(int icdev);
        [DllImport(DllPath)]
        public static extern short dc_beep(int icdev, ushort misc);
        [DllImport(DllPath)]
        public static extern short dc_reset(int icdev, uint sec);
        [DllImport(DllPath)]
        public static extern short dc_config_card(int icdev, byte cardType);
        [DllImport(DllPath)]

        public static extern short dc_card(int icdev, byte model, ref ulong snr);

        [DllImport(DllPath)]
        public static extern short dc_card_double(int icdev, byte model, [Out] byte[] snr);

        [DllImport(DllPath)]
        public static extern short dc_card_double_hex(int icdev, byte model, [Out]char[] snr);

        [DllImport(DllPath)]
        public static extern short dc_pro_reset(int icdev, ref byte rlen, [Out] byte[] recvbuff);
        [DllImport(DllPath)]
        public static extern short dc_pro_command(int icdev, byte slen, byte[] sendbuff, ref byte rlen,
            [Out]byte[] recvbuff, byte timeout);
        [DllImport(DllPath)]
        public static extern short dc_card_b(int icdev, [Out] byte[] atqb);


        [DllImport(DllPath)]
        public static extern short dc_setcpu(int icdev, byte address);
        [DllImport(DllPath)]
        public static extern short dc_cpureset(int icdev, ref byte rlen, byte[] rdata);
        [DllImport(DllPath)]
        public static extern short dc_cpuapdu(int icdev, byte slen, byte[] sendbuff, ref byte rlen,
            [Out]byte[] recvbuff);

        [DllImport(DllPath)]
        public static extern short dc_readpincount_4442(int icdev);
        [DllImport(DllPath)]
        public static extern short dc_read_4442(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
        [DllImport(DllPath)]
        public static extern short dc_write_4442(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
        [DllImport(DllPath)]
        public static extern short dc_verifypin_4442(int icdev, byte[] password);
        [DllImport(DllPath)]
        public static extern short dc_readpin_4442(int icdev, byte[] password);
        [DllImport(DllPath)]
        public static extern short dc_changepin_4442(int icdev, byte[] password);

        [DllImport(DllPath)]
        public static extern short dc_readpincount_4428(int icdev);
        [DllImport(DllPath)]
        public static extern short dc_read_4428(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
        [DllImport(DllPath)]
        public static extern short dc_write_4428(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
        [DllImport(DllPath)]
        public static extern short dc_verifypin_4428(int icdev, byte[] password);
        [DllImport(DllPath)]
        public static extern short dc_readpin_4428(int icdev, byte[] password);
        [DllImport(DllPath)]
        public static extern short dc_changepin_4428(int icdev, byte[] password);

        [DllImport(DllPath)]
        public static extern int dc_authentication(int icdev, int _Mode, int _SecNr);

        [DllImport(DllPath)]
        public static extern int dc_authentication_pass(int icdev, int _Mode, int _SecNr, byte[] nkey);

        [DllImport(DllPath)]
        public static extern int dc_authentication_pass_hex(int icdev, int _Mode, int _SecNr, string nkey);

        [DllImport(DllPath)]
        public static extern int dc_load_key(int icdev, int mode, int secnr, byte[] nkey);  //密码装载到读写模块中


        [DllImport(DllPath)]
        public static extern int dc_write(int icdev, int adr, [In] byte[] sdata);  //向卡中写入数据

        [DllImport(DllPath)]
        public static extern int dc_write_hex(int icdev, int adr, [In] string sdata);  //向卡中写入数据


        [DllImport(DllPath)]
        public static extern int dc_read(int icdev, int adr, [Out] byte[] sdata);  //从卡中读数据

        [DllImport(DllPath)]
        public static extern short dc_read_24c(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
        [DllImport(DllPath)]
        public static extern short dc_write_24c(int icdev, Int16 offset, Int16 lenth, byte[] buffer);

        [DllImport(DllPath)]
        public static extern short dc_read_24c64(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
        [DllImport(DllPath)]
        public static extern short dc_write_24c64(int icdev, Int16 offset, Int16 lenth, byte[] buffer);
    }
}
