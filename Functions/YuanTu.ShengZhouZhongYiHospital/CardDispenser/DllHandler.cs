using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShengZhouZhongYiHospital.CardDispenser
{
    public class DllHandler
    {
        private const string DllPathDk = "External\\Zbr\\dcic32.dll";
        //申明API位置.
        [DllImport(DllPathDk)]
        //初始化端口
        public static extern int IC_InitComm(int Port);

        [DllImport(DllPathDk)]
        //关闭端口
        public static extern short IC_ExitComm(int icdev);

        [DllImport(DllPathDk)]
        //卡下电(要重新审核密码才能继续操作)
        public static extern short IC_Down(int icdev);

        [DllImport(DllPathDk)]
        //初始化卡型
        public static extern short IC_InitType(int icdev, int cardType);

        [DllImport(DllPathDk)]
        //判断连接是否成功,<0 ,连接不成功.0可以读写,1连接成功,但是没插卡.
        public static extern short IC_Status(int icdev);
        [DllImport(DllPathDk)]
        //检查原始密码(小于0为不正确)
        public static extern short IC_CheckPass_4442hex(int icdev, string Password);
        [DllImport(DllPathDk)]
        //更改密码(0为更改成功)
        public static extern short IC_ChangePass_4442hex(int icdev, string Password);
        [DllImport(DllPathDk)]
        //在固定的位置写入固定长度的数据
        public static extern short IC_Write(int icdev, int offset, int Length, string Databuffer);
        [DllImport(DllPathDk)]
        //在固定的位置读出固定长度的数据
        public static extern short IC_Read(int icdev, int offset, int l, byte[] Databuffer);
        [DllImport(DllPathDk)]
        //发出声音
        public static extern short IC_DevBeep(int icdev, int intime);
        [DllImport(DllPathDk)]
        //4428读卡
        public static extern short IC_ReadWithProtection(int icdev, int offset, int l, byte[] Databuffer);

        [DllImport(DllPathDk)]
        //4428读卡
        public static extern short IC_WriteWithProtection(int icdev, int offset, int l, byte[] Databuffer);
    }
}
