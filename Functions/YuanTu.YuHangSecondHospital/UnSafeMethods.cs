using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.YuHangSecondHospital
{
    public class UnSafeMethods
    {
        #region T6接口函数

        private const string DllPathT6 = "External\\Z9\\dcic32.dll";

        [DllImport(DllPathT6)] //初始化端口
        public static extern int IC_InitComm(int port);

        [DllImport(DllPathT6)] //关闭端口
        public static extern short IC_ExitComm(int icdev);

        [DllImport(DllPathT6)] //卡下电(要重新审核密码才能继续操作)
        public static extern short IC_Down(int icdev);

        [DllImport(DllPathT6)] //初始化卡型
        public static extern short IC_InitType(int icdev, int cardType);

        [DllImport(DllPathT6)] //判断连接是否成功,<0 ,连接不成功.0可以读写,1连接成功,但是没插卡.
        public static extern short IC_Status(int icdev);

        [DllImport(DllPathT6)] //检查原始密码(小于0为不正确)
        public static extern short IC_CheckPass_4442hex(int icdev, string password);

        [DllImport(DllPathT6)] //更改密码(0为更改成功)
        public static extern short IC_ChangePass_4442hex(int icdev, string password);

        [DllImport(DllPathT6)] //在固定的位置写入固定长度的数据
        public static extern short IC_Write(int icdev, int offset, int length, string databuffer);

        [DllImport(DllPathT6)] //在固定的位置读出固定长度的数据
        public static extern short IC_Read(int icdev, int offset, int l, byte[] databuffer);

        [DllImport(DllPathT6)] //发出声音
        public static extern short IC_DevBeep(int icdev, int intime);

        //cpu卡函数
        [DllImport(DllPathT6)]
        public static extern short IC_CpuReset(int icdev, ref byte rlen, byte[] rbuff);

        [DllImport(DllPathT6)]
        public static extern short IC_CpuReset_Hex(int icdev, ref byte rlen, byte[] rbuff);

        [DllImport(DllPathT6)] //设置cpu参数
        public static extern short IC_SetCpuPara(int icdev, short cputype, short cpupro, short cpuetu);

        [DllImport(DllPathT6)] //cpu卡信息交换协议
        public static extern short IC_CpuApdu(int icdev, byte slen, byte[] sbuff, ref byte rlen, byte[] rbuff);

        [DllImport(DllPathT6)] //cpu卡信息交换协议
        public static extern short IC_CpuApdu_Hex(int icdev, short slen, string sbuff, ref short rlen,
            StringBuilder rbuff);

        #endregion T6接口函数
    }
}