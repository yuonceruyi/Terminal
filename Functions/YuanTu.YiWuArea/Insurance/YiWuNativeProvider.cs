using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.YiWuArea.Insurance
{
    public class YiWuNativeProvider
    {
        private const string SiPayDll = "External\\SiDll\\BargaingApplyV3_01022.dll";//
        private const string MwpdDll = "External\\SiDll\\mwpd_32.dll";//
        [DllImport(SiPayDll, EntryPoint = "f_UserBargaingInit", CharSet = CharSet.Ansi)]
        public static extern int f_UserBargaingInit(string data1, StringBuilder retMsg, string data2);
        [DllImport(SiPayDll, EntryPoint = "f_UserBargaingApply", CharSet = CharSet.Ansi)]
        public static extern int f_UserBargaingApply(int code, double no, string data, StringBuilder retMsg, string data2);
        [DllImport(SiPayDll, EntryPoint = "f_UserBargaingClose", CharSet = CharSet.Ansi)]
        public static extern int f_UserBargaingClose(string data1, StringBuilder retMsg, string data2);

        #region  设备操作函数及工具函数

        //[DllImport(MwpdDll, EntryPoint = "ic_init", CharSet = CharSet.Ansi)]
        //public static extern int ic_init(int port, int count);
        //[DllImport(MwpdDll, EntryPoint = "ic_exit", CharSet = CharSet.Ansi)]
        //public static extern int ic_exit(int handle);
        [DllImport(MwpdDll, EntryPoint = "readcardid", CharSet = CharSet.Ansi)]
        public static extern int readcardid(IntPtr handle, char flag, StringBuilder cardNo);

       
        /// <summary>
        /// 磁道数据结构体，用于存放读取到的磁卡的数据
        /// </summary>
        public struct MagCardData
        {
            /// <summary>
            /// 第一磁道数据长度
            /// </summary>
            public byte track1_len;
            /// <summary>
            /// 第二磁道数据长度
            /// </summary>
            public byte track2_len;
            /// <summary>
            /// 第三磁道数据长度
            /// </summary>
            public byte track3_len;

            /// <summary>
            /// 第一磁道数据 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] track1_data;

            /// <summary>
            /// 第二磁道数据
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
            public byte[] track2_data;

            /// <summary>
            /// 第三磁道数据
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 107)]
            public byte[] track3_data;
        };

        /// <summary>
        /// 初始化读写器，返回设备句柄
        /// </summary>
        /// <param name="port">串口号,0表示COM1，1表示COM2，以此类推。100表示USB口</param>
        /// <param name="baud">波特率,当port为100时，可默认为9600</param>
        /// <returns>读写器句柄</returns>
        [DllImport(MwpdDll)]
        public static extern IntPtr ic_init(Int16 port, Int32 baud);

        /// <summary>
        /// 断开设备
        /// </summary>
        /// <param name="devNo">ic_init返回的设备句柄</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        public static extern Int16 ic_exit(IntPtr icdev);

        /// <summary>
        /// 读取硬件版本号
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="len">版本号字符串长度，其值为18</param>
        /// <param name="data_buffer">存放读取的版本号字符串</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 srd_ver(IntPtr icdev, Int16 len, byte[] data_buffer);

        [DllImport(MwpdDll)]
        private static extern Int16 srd_snr(IntPtr icdev, Int16 len, byte[] data_buffer);

        [DllImport(MwpdDll)]
        private static extern Int16 swr_snr(IntPtr icdev, Int16 len, byte[] data_buffer);

        [DllImport(MwpdDll)]
        private static extern Int16 ic_decrypt(byte[] key, byte[] ptrSource, ushort msgLen, byte[] ptrDest);


        /// <summary>
        /// 读写器鸣响
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="time">蜂鸣时间，值范围0～255（单位10ms）</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        public static extern Int16 dv_beep(IntPtr icdev, Int16 time);

        /// <summary>
        /// 获取设备插卡状态
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="state">插卡状态state=1读写器插有卡；state=0读写器未插卡</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 get_status(IntPtr icdev, out Int16 state);

        /// <summary>
        /// 写设备备注区（总长384字节）
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">备注区偏移地址0～383</param>
        /// <param name="len">写入信息长度1～384</param>
        /// <param name="data_buffer">写入的信息</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 swr_eeprom(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 读取设备备注区信息（总长384字节）
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">备注区偏移地址0～383</param>
        /// <param name="len">读出信息长度1～384</param>
        /// <param name="data_buffer">存放读出的备注信息</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 srd_eeprom(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// LCD显示字符串，共4行16列，每行最多可显示16个字符或者8个汉字
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="row">行号，0~3</param>
        /// <param name="column">列号，0~15</param>
        /// <param name="len">字符串长度，大于0小于16</param>
        /// <param name="disp_buf">显示的字符串，每个汉字占两个字节</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 lcd_display_string(IntPtr icdev, Int16 row, Int16 column, Int16 len, byte[] disp_buf);

        /// <summary>
        /// LCD显示清屏
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 lcd_clear(IntPtr icdev);

        /// <summary>
        /// 获取密码键盘输入的密码数据，设定输入密码长度和等待输入的超时时间，读写器在指定超时时间内接收到指定长度的密码（并按确认键结束密码输入）后，将返回按键编码，按键编码有明文和密文两种传输方式。否则操作失败
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="pw_len">要求输入的密码长度，0~16</param>
        /// <param name="timeout">等待输入的超时时间，最好不要大于30秒</param>
        /// <param name="encrypt">传输模式，TRUE为密文传输，FALSE为明文传输</param>
        /// <param name="des_key">密文传输时的传输密钥,长度为8字节</param>
        /// <param name="data_buffer">返回的按键编码</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 get_keyboard_input(IntPtr icdev, Int16 pw_len, byte timeout, bool encrypt, byte[] des_key, byte[] data_buffer);

        /// <summary>
        /// 播放语音提示
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="number">语音编号，0:“请输入密码”，1:“请再次输入密码”</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 voice_display(IntPtr icdev, Int16 number);

        /// <summary>
        /// 读取磁条卡磁道数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="timeout">等待刷卡超时时间，单位为秒，0~255，建议设置的时间不要太长</param>
        /// <param name="mag_card">磁道数据结构体，用于存放读取到的磁卡的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 read_magnetic_card(IntPtr icdev, byte timeout, ref MagCardData mag_card);

        /// <summary>
        /// 将ASCII码转换为十六进制数据
        /// </summary>
        /// <param name="asc">输入要转换的字符串数组</param>
        /// <param name="hex">存放转换后的字符串数组</param>
        /// <param name="pair_len">为转换后的字符串长度</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 asc_hex(byte[] asc, byte[] hex, int pair_len);

        /// <summary>
        /// 将十六进制数据转换为ASCII码
        /// </summary>
        /// <param name="hex">输入要转换的字符串数组</param>
        /// <param name="asc">存放转换后的字符串数组</param>
        /// <param name="length">为要转换的字符串长度</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 hex_asc(byte[] hex, byte[] asc, int length);

        /// <summary>
        /// 进入获取键盘密码的状态,进入该状态后只接收IC_EPassGet和IC_PassCancel命令.该接口通知读卡器使用密文传输密码
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="ctime">是用户按键输入的超时时间，以second为单位；最大255s，最小1s</param>
        /// <param name="panlen">初始密钥长度</param>
        /// <param name="pandata">初始密钥</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_EPassIn(IntPtr icdev, byte ctime, byte panlen, byte[] pandata);

        /// <summary>
        /// 查询和获取输入的密码
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="rlen">密码位数（1-6之间，根据客户输入而定）</param>
        /// <param name="cpass">密码</param>
        /// <returns>type ＝ 0x0， 成功，返回加密后的密码,读卡器自动退出获取密码状态。
        ///			 type ＝ 0xa1，用户取消密码输入,读卡器自动退出获取密码状态。
        ///			 type ＝ 0xa2，用户密码输入操作超时,读卡器自动退出获取密码状态。
        ///			 type ＝ 0xa3，未处于密码输入状态
        ///			 type ＝ 0xa4，用户输入密码还未完成，返回按键个数、*号串。
        ///			 注意：0xa4这个返回值很重要,在开始查询中会一直遇到,表示输入还没有完成,可以再次执行mw_ic_EPassGet函数来获取密码</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_EPassGet(IntPtr icdev, out byte rlen, byte[] cpass);

        /// <summary>
        /// 退出获取密码状态
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_PassCancel(IntPtr icdev);

        /// <summary>
        /// 读取设备信息
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址</param>
        /// <param name="len">读取的长度</param>
        /// <param name="databuffer">读取到的数据</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_ReadDevice(IntPtr icdev, Int16 offset, Int16 len, byte[] databuffer);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_CreatMacData(IntPtr icdev, UInt16 DataLen, byte[] SourceData, byte[] MacData);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_LcdClrScrn(IntPtr idComDev, byte cLine);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_DispInfo(IntPtr idComDev, byte line, byte offset, byte[] data);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_Card(IntPtr idComDev, byte _Mode, byte[] _Snr);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_LoadKey(IntPtr idComDev, byte _bMode, byte _bSecNr, byte[] _bNKey);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_ReadMifare(IntPtr icdev, byte blocknr, byte[] data);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_WriteMifare(IntPtr icdev, byte blocknr, byte[] data);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_Authentication(IntPtr idComDev, byte _bMode, byte _bSecNr);

        [DllImport(MwpdDll)]
        private static extern Int16 mw_ic_ShowInfo(IntPtr idComDev, byte flag);

        //[DllImport(MwpdDll)]
        //public static extern Int16 mw_ic_CreatMacData(IntPtr icdev, UInt16 dataLen, byte[] data, byte[] MacData);

        #endregion

        #region  M1卡及非接CPU卡操作函数
        /// <summary>
        /// 寻卡,并激活卡片.Mifare标准卡只有激活后才可以执行认证,读/写数据,值操作以及命令交换等操作.双界面卡激活后可以执行卡片COS指令.在调用rf_card()函数前,对于双界面卡必须确保支持ISO14443-4协议,如果取消了对ISO14443-4协议的支持,
        /// 必须调用rf_enable_AutoATS()函数重新设置支持.对于兼容Mifare标准的卡片,则需要取消ISO14443-4协议的支持,同样调用rf_enable_AutoATS()函数进行设置.
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="mode">寻卡模式
        ///0:只能寻到空闲状态下的卡片，被激活的卡片不会响应
        ///1:所有卡片都能寻到，已经被激活的卡片将回到空闲状态后并重新被激活。
        ///</param>
        /// <param name="_Snr">返回的卡片序列号</param>
        /// <param name="_SnrLen">返回的卡片序列号的长度</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_card(IntPtr icdev, byte mode, byte[] _Snr, out short SnrLen);

        [DllImport(MwpdDll)]
        private static extern Int16 close_card(IntPtr icdev);

        /// <summary>
        /// 卡片数据块认证
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="mode">密码认证模式，0:使用密码A认证，4:使用密码B认证</param>
        /// <param name="blocknr">卡片操作块地址</param>
        /// <param name="key">认证密码，长度为6字节</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_authentication_key(IntPtr icdev, byte mode, byte blocknr, byte[] key);

        /// <summary>
        /// 读卡片数据块，每次读取16字节数据。Mifare S50和S70返回一个块的数据，UltraLight返回指定块地址开始的4个块的数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="blocknr">卡片数据块地址</param>
        /// <param name="data">读出的卡片数据块的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_read(IntPtr icdev, byte blocknr, byte[] data);

        /// <summary>
        /// 卡片数据块写数据操作，每次写入16字节的数据。针对每个数据块长度为16字节的卡片。
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="blocknr">卡片数据块地址</param>
        /// <param name="data">将要写入的数据，长度为16字节</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_write(IntPtr icdev, byte blocknr, byte[] data);

        /// <summary>
        /// 卡片数据块加值操作
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="blocknr">卡片数据块地址</param>
        /// <param name="_val">将要增加的值</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_increment(IntPtr icdev, byte blocknr, UInt32 _val);

        /// <summary>
        /// 卡片数据块做减值操作
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="blocknr">卡片数据块地址</param>
        /// <param name="_val">将要减去的值</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_decrement(IntPtr icdev, byte blocknr, UInt32 _val);

        /// <summary>
        /// 卡片数据块值操作初始化，卡片数据块用于值操作功能时，必须先对数据块进行值操作初始化。只有进行该初始化操作后的数据块可以做加值、减值操作
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="_Adr">卡片数据块地址</param>
        /// <param name="_Value">将要设置的初始值</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_initval(IntPtr icdev, byte _Adr, UInt32 _Value);

        /// <summary>
        /// 卡片数据块读当前值
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="_Adr">卡片数据块地址</param>
        /// <param name="_Value">读出的卡片数据块当前值</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_readval(IntPtr icdev, byte _Adr, out UInt32 _Value);

        /// <summary>
        /// 设置取消/支持ISO14443-4协议。双界面卡需要设置支持ISO14443-4协议，兼容Mifare标准卡需取消ISO14443-4协议支持
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="bEnableState">TRUE:设置支持ISO14443-4协议  FALSE:取消ISO14443-4协议支持</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_enable_AutoATS(IntPtr icdev, bool bEnableState);

        /// <summary>
        /// 寻卡成功后，调用此函数取卡片的复位信息
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="_CardProperty">复位信息</param>
        /// <param name="_CardPropertyLen">复信信息的长度</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_CardProperty(IntPtr icdev, byte[] _CardProperty, out byte _CardPropertyLen);

        /// <summary>
        /// 发送COS指令给卡片，并返回卡片执行指令后的应答信息
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="sendlen">发送的指令长度</param>
        /// <param name="sendblock">发送的指令信息</param>
        /// <param name="recvlen">接收到的应答信息长度</param>
        /// <param name="recvblock">应答信息</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rf_pro_trn(IntPtr icdev, byte sendlen, byte[] sendblock, out byte recvlen, byte[] recvblock);
        #endregion

        #region SLE4442卡、24c16卡及接触CPU卡操作

        /// <summary>
        /// 检查卡型是否正确 
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 chk_4442(IntPtr icdev);

        /// <summary>
        /// 读出密码错误计数器值
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="counter">密码错误记数值</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rsct_4442(IntPtr icdev, out Int16 counter);

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="keyLen">密码个数，其值为3</param>
        /// <param name="key">密码</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 csc_4442(IntPtr icdev, Int16 keyLen, byte[] key);

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="keyLen">密码个数，其值为3</param>
        /// <param name="key">密码</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 wsc_4442(IntPtr icdev, Int16 keyLen, byte[] key);

        /// <summary>
        /// 从指定地址读数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址，其值范围0～255</param>
        /// <param name="len">字符串长度，其值范围1～256</param>
        /// <param name="data_buffer">读出的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 srd_4442(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 向指定地址写数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址，其值范围0～255</param>
        /// <param name="len">字符串长度，其值范围1～256</param>
        /// <param name="data_buffer">写入的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 swr_4442(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 检查卡型是否正确 
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 chk_4428(IntPtr icdev);

        /// <summary>
        /// 读出密码错误计数器值
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="counter">密码错误记数值</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 rsct_4428(IntPtr icdev, out Int16 counter);

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="keyLen">密码个数，其值为2</param>
        /// <param name="key">密码</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 csc_4428(IntPtr icdev, Int16 keyLen, byte[] key);

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="keyLen">密码个数，其值为2</param>
        /// <param name="key">密码</param>
        /// <returns></returns>
        [DllImport(MwpdDll)]
        private static extern Int16 wsc_4428(IntPtr icdev, Int16 keyLen, byte[] key);

        /// <summary>
        /// 从指定地址读数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址，其值范围0～1023</param>
        /// <param name="len">字符串长度，其值范围1～1024 </param>
        /// <param name="data_buffer">读出的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 srd_4428(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 向指定地址写数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址，其值范围0～1023</param>
        /// <param name="len">字符串长度，其值范围1～1024</param>
        /// <param name="data_buffer">写入的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 swr_4428(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 检查卡型是否正确 
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 chk_24c16(IntPtr icdev);

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址，范围0～2047</param>
        /// <param name="len">字符串长度，范围1～2048</param>
        /// <param name="data_buffer">写入的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 swr_24c16(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="offset">偏移地址，范围0～2047</param>
        /// <param name="len">字符串长度，范围1～2048</param>
        /// <param name="data_buffer">读出的数据</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 srd_24c16(IntPtr icdev, Int16 offset, Int16 len, byte[] data_buffer);

        /// <summary>
        /// 接触CPU卡复位
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="CardType">卡座：
        ///                         0--上卡座
        ///                         1-	下大卡座(sam1)
        ///                         2-	小卡座(sam2)
        ///                         3-	小卡座(sam3)</param>
        /// <param name="len">复位信息长度</param>
        /// <param name="receive_data">复位信息</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 sam_slt_reset(IntPtr icdev, byte CardType, out Int16 len, byte[] receive_data);

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="CardType">卡座：
        ///                         0--上卡座
        ///                         1-	下大卡座(sam1)
        ///                         2-	小卡座(sam2)
        ///                         3-	小卡座(sam3)</param>
        /// <param name="sLen">发送的命令实际长度</param>
        /// <param name="send_cmd">发送的命令</param>
        /// <param name="rLen">从receive_data[3]开始CPU卡应答信息的长度</param>
        /// <param name="receive_data">CPU卡应答信息</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 sam_slt_protocol(IntPtr icdev, byte CardType, Int16 sLen, byte[] send_cmd, out Int16 rLen, byte[] receive_data);

        /// <summary>
        /// 卡片下电
        /// </summary>
        /// <param name="icdev">ic_init返回的设备句柄</param>
        /// <param name="CardType">卡座：
        ///                         0--上卡座
        ///                         1-	下大卡座(sam1)
        ///                         2-	小卡座(sam2)
        ///                         3-	小卡座(sam3)</param>
        /// <returns>=0成功，非0失败</returns>
        [DllImport(MwpdDll)]
        private static extern Int16 sam_slt_power_down(IntPtr icdev, byte CardType);
        #endregion

        private static IntPtr _icdev = IntPtr.Zero;

        public static Result<YiBaoCardContent> Read()
        {
            try
            {
                _icdev = ic_init(100 /*USB*/, 115200);
                if (_icdev.ToInt32() <= 0)
                {
                    return Result<YiBaoCardContent>.Fail("医保读卡器打开失败，请联系医护人员检查读卡器连接线");
                }
                dv_beep(_icdev, 30); //蜂鸣
                string resultMsg = null;
                var cardContent = new YiBaoCardContent();

                #region 读社保卡

                //选择主文件
                var restData = CpuApdu(0, "00a404000f7378312e73682ec9e7bbe1b1a3d5cf", false).Trim();
                if (restData == "")
                {
                    resultMsg = "您插入的不是社保卡，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //选择EF05
                restData = CpuApdu(0, "00a4000002EF05", false).Trim();
                if (restData == "")
                {
                    resultMsg = "您插入的不是社保卡，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读卡识别码
                cardContent.CardId = CpuApdu(0, "00b2010010", false).Trim();
                if (cardContent.CardId == "")
                {
                    resultMsg = "读卡出错(未能读取社保识别码)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读卡类别
                cardContent.CardType = CpuApdu(0, "00b2020001", true).Trim();
                if (cardContent.CardType == "")
                {
                    resultMsg = "读卡出错(未能读取社保卡类别)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读规范版本
                cardContent.GFVer = CpuApdu(0, "00b2030004", true).Trim();
                if (cardContent.GFVer == "")
                {
                    resultMsg = "读卡出错(未能读取社保版本)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取发卡机构代码
                cardContent.FKJG = CpuApdu(0, "00b204000c", false).Trim();
                if (cardContent.FKJG == "")
                {
                    resultMsg = "读卡出错(未能读取社保发卡机构代码)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取发卡日期
                cardContent.CardData = CpuApdu(0, "00b2050004", false).Trim();
                if (cardContent.CardData == "")
                {
                    resultMsg = "读卡出错(未能读取社保卡发卡日期)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取卡有效期
                cardContent.CardValidData = CpuApdu(0, "00b2060004", false).Trim();
                if (cardContent.CardValidData == "")
                {
                    resultMsg = "读卡出错(未能读取社保卡有效期)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                //读取卡号
                cardContent.CardNumber = CpuApdu(0, "00b2070009", true).Trim();
                if (cardContent.CardNumber == "" && cardContent.CardNumber.Length > 2)
                {
                    resultMsg = "读卡出错(未能读取社保卡号)，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //选择EF06
                restData = CpuApdu(0, "00a4000002ef06", true).Trim();
                if (restData == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取身份证号
                cardContent.IDNumber = CpuApdu(0, "00b2080018", true).Trim();
                if (cardContent.IDNumber == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                //else
                //{
                //    cardContent.IDNumber = cardContent.IDNumber.Substring(2);
                //}

                //读取姓名
                cardContent.Name = CpuApdu(0, "00b209001e", true).Trim(new[] { ' ', '\0' });
                if (cardContent.Name == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                //else
                //{
                //    cardContent.Name = cardContent.Name.Substring(1).TrimEnd('\0');
                //}

                //读取性别
                cardContent.Sex = CpuApdu(0, "00b20a0001", true).Trim();
                if (cardContent.Sex == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取民族
                cardContent.Nation = CpuApdu(0, "00b20b0001", false).Trim();
                if (cardContent.Nation == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取出生地
                cardContent.Birthplace = CpuApdu(0, "00b20c0003", false).Trim();
                if (cardContent.Birthplace == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取出生日期
                cardContent.Birthday = CpuApdu(0, "00b20d0004", false).Trim();
                if (cardContent.Birthday == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }


                #endregion


                if (string.IsNullOrEmpty(cardContent.CardNumber))
                {
                    return Result<YiBaoCardContent>.Fail("读卡失败，请重新插卡");
                }
                Logger.Device.Info($"[社保读卡]读卡信息:" + cardContent.ToJsonString());
                return Result<YiBaoCardContent>.Success(cardContent);
            }
            catch (Exception ex)
            {

                Logger.Main.Error($"[社保读卡]读取医保设备时抛出异常,内容:{ex.Message} 堆栈:{ex.StackTrace}");
                return Result<YiBaoCardContent>.Fail("读社保卡时发生异常，请联系医护人员处理", ex);
            }
            finally
            {
                ic_exit(_icdev);
            }
        }
        public static Result<string> ReadId()
        {
            _icdev = ic_init(100 /*USB*/, 115200);
            try
            {
                if (_icdev.ToInt32() <= 0)
                {
                    return Result<string>.Fail("医保读卡器打开失败，请联系医护人员检查读卡器连接线");
                }
                var builder = new StringBuilder();
                builder.Append(' ', 20480);
                var ret = readcardid(_icdev, '1', builder);
                if (ret == 0)
                {
                    return Result<string>.Success(builder.ToString());

                }
                return Result<string>.Fail($"返回{ret}");

            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
            finally
            {
                ic_exit(_icdev);
            }


        }

        private static string CpuApdu(byte cardSet, string cmdData, bool dataType)
        {

            var str = "";
            var return_data = new byte[256];
            var send_cmd = new byte[256];
            short rLen = 0;
            var strcmd = Encoding.Default.GetBytes(cmdData);
            var len = (Int16)(cmdData.Length / 2); //输入的命令长度
            var cmd = new byte[len];
            asc_hex(strcmd, cmd, len);
            Array.Copy(cmd, 0, send_cmd, 3, len);  //把命令复制到send_cmd中第四个字节开始处
            send_cmd[1] = 0x40;
            send_cmd[2] = (byte)len;
            //异或
            for (var i = 0; i < len + 3; i++)
            {
                send_cmd[len + 3] = (byte)(send_cmd[len + 3] ^ send_cmd[i]);
            }

            var st = sam_slt_protocol(_icdev, cardSet, (Int16)(len + 4), send_cmd, out rLen, return_data);
            if (st == 0)
            {
                if (return_data[rLen - 3] != 0x90 && return_data[rLen - 2] != 0)
                {
                    return str;
                }

                if (return_data[2] == 0)
                {
                    str = "9000";
                }
                else
                {
                    var rec_data = new byte[500];
                    if (dataType == true)
                    {
                        Array.Copy(return_data, 3, rec_data, 0, return_data[2]);
                        str = Encoding.Default.GetString(rec_data, 2, return_data[2] - 2);
                    }
                    else
                    {
                        hex_asc(return_data, rec_data, rLen);
                        str = Encoding.Default.GetString(rec_data, 6, return_data[2] * 2);
                    }
                }
            }
            Logger.Device.Info($"[社保读卡]APDU 入参:{cmdData} 返回:{Encoding.Default.GetString(return_data)}");
            return str;
        }

        /*

        /*
    public static Result<YiBaoCardContent> Read()
    {
        if (!OpenReader())
        {
            return Result<YiBaoCardContent>.Fail("读卡器打开失败");
        }
        var rest = readData();
        CloseReader();
        if (rest.Item1)
        {
            return Result<YiBaoCardContent>.Success(rest.Item2);
        }
        return Result<YiBaoCardContent>.Fail(rest.Item3);
    }


    static IntPtr _icdev = IntPtr.Zero;
    static short _st, _mSt, _pSt, _cSt;
    static byte psamCardSet = 1;

    private static string cpuApdu(byte cardSet, string cmdData, bool dataType)
    {

        string str = "";
        byte[] return_data = new byte[256];
        byte[] send_cmd = new byte[256];
        Int16 rLen = 0;
        byte[] strcmd = Encoding.Default.GetBytes(cmdData);
        Int16 len = (Int16)(cmdData.Length / 2); //输入的命令长度
        byte[] cmd = new byte[len];
        asc_hex(strcmd, cmd, len);
        Array.Copy(cmd, 0, send_cmd, 3, len);  //把命令复制到send_cmd中第四个字节开始处
        send_cmd[1] = 0x40;
        send_cmd[2] = (byte)len;

        for (int i = 0; i < len + 3; i++)
        {
            send_cmd[len + 3] = (byte)(send_cmd[len + 3] ^ send_cmd[i]);
        }

        _st = sam_slt_protocol(_icdev, cardSet, (Int16)(len + 4), send_cmd, out rLen, return_data);
        if (_st == 0)
        {
            if (return_data[rLen - 3] != 0x90 && return_data[rLen - 2] != 0)
            {
                return str;
            }

            if (return_data[2] == 0)
            {
                str = "9000";
            }
            else
            {
                byte[] rec_data = new byte[500];
                if (dataType == true)
                {
                    Array.Copy(return_data, 3, rec_data, 0, return_data[2]);
                    str = Encoding.Default.GetString(rec_data, 0, return_data[2]);
                }
                else
                {
                    hex_asc(return_data, rec_data, rLen);
                    str = Encoding.Default.GetString(rec_data, 6, return_data[2] * 2);
                }
            }
        }

        return str;
    }

    private static bool OpenReader()
    {
        _icdev = ic_init(100, 115200);
        if (_icdev.ToInt32() > 0)
        {
            dv_beep(_icdev, 30);//蜂鸣
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool CloseReader()
    {
        try
        {
            mw_ic_DispInfo(_icdev, 0, 0, Encoding.Default.GetBytes("            "));
            mw_ic_DispInfo(_icdev, 1, 4, Encoding.Default.GetBytes("欢迎使用"));
            mw_ic_DispInfo(_icdev, 2, 4, Encoding.Default.GetBytes("智慧医疗"));

        }

        finally
        {
            ic_exit(_icdev);

        }
        return true;
    }


    private static Tuple<bool, YiBaoCardContent, string> readData()
    {
        try
        {
            string resultMsg = null;
            var cardContent = new YiBaoCardContent();
            string cardData = "";
            byte[] resetData = new byte[50];
            short len = 0;

            #region 读M1卡
            byte[] cardSnr = new byte[7];
            _mSt = rf_card(_icdev, 0, cardSnr, out len);
            if (_mSt == 0)
            {
                //cardType = true;
                byte[] Snr = new byte[15];
                hex_asc(cardSnr, Snr, len);
                // textBoxSnrHex.Text = Encoding.Default.GetString(Snr);
                //textBoxSnr.Text = BitConverter.ToUInt32(cardSnr, 0).ToString();
            }
            #endregion
            //延时，以便可以读到接触卡片
            DateTime dateBegin = DateTime.Now;
            while (true)
            {
                //Thread.Sleep(100);
                DateTime endTime = DateTime.Now;
                double timeDate = (endTime - dateBegin).TotalMilliseconds;
                if (timeDate > 500)
                {
                    goto PSAMCARD;
                    //break; 
                }

                _pSt = sam_slt_reset(_icdev, 0, out len, resetData);
                if (_pSt == 0)
                {
                    break;
                }
                //读取4428卡
                _cSt = chk_4428(_icdev);
                if (_cSt != 0)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    byte[] SData = new byte[17];
                    _cSt = srd_4428(_icdev, 64, 16, SData);  //从卡内读出的IC卡卡号
                    if (_cSt == 0)
                    {
                        if (SData != null || SData.Length != 0)
                        {
                            byte[] key = Encoding.Default.GetBytes("13572468"); //密钥
                            byte[] ptrDest = new byte[50];
                            _st = ic_decrypt(key, SData, 16, ptrDest);
                            if (_st == 0)
                            {
                                ptrDest[16] = 0;
                                //textBoxOldNumber.Text = Encoding.Default.GetString(ptrDest);
                            }
                        }
                    }
                    break;
                }
            }

            #region 读社保卡
            //选择主文件
            cardData = cpuApdu(0, "00a404000f7378312e73682ec9e7bbe1b1a3d5cf", false).Trim();
            if (cardData == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //选择EF05
            cardData = cpuApdu(0, "00a4000002EF05", false).Trim();
            if (cardData == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读卡识别码
            cardContent.CardId = cpuApdu(0, "00b2010010", false).Trim();
            if (cardContent.CardId == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读卡类别
            cardContent.CardType = cpuApdu(0, "00b2020001", true).Trim();
            if (cardContent.CardType == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读规范版本
            cardContent.GFVer = cpuApdu(0, "00b2030004", true).Trim();
            if (cardContent.GFVer == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取发卡机构代码
            cardContent.FKJG = cpuApdu(0, "00b204000c", false).Trim();
            if (cardContent.FKJG == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取发卡日期
            cardContent.CardData = cpuApdu(0, "00b2050004", false).Trim();
            if (cardContent.CardData == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取卡有效期
            cardContent.CardValidData = cpuApdu(0, "00b2060004", false).Trim();
            if (cardContent.CardValidData == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            //读取卡号
            cardContent.CardNumber = cpuApdu(0, "00b2070009", true).Trim();
            if (cardContent.CardNumber == "" && cardContent.CardNumber.Length > 2)
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            else
            {
                cardContent.CardNumber = cardContent.CardNumber.Substring(2);
            }

            //选择EF06
            cardData = cpuApdu(0, "00a4000002ef06", true).Trim();
            if (cardData == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取身份证号
            cardContent.IDNumber = cpuApdu(0, "00b2080018", true).Trim();
            if (cardContent.IDNumber == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            else
            {
                cardContent.IDNumber = cardContent.IDNumber.Substring(2);
            }

            //读取姓名
            cardContent.Name = cpuApdu(0, "00b209001e", true).Trim();
            if (cardContent.Name == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            else
            {
                cardContent.Name = cardContent.Name.Substring(1).TrimEnd('\0');
            }

            //读取性别
            cardContent.Sex = cpuApdu(0, "00b20a0001", true).Trim();
            if (cardContent.Sex == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取民族
            cardContent.Nation = cpuApdu(0, "00b20b0001", false).Trim();
            if (cardContent.Nation == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取出生地
            cardContent.Birthplace = cpuApdu(0, "00b20c0003", false).Trim();
            if (cardContent.Birthplace == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读取出生日期
            cardContent.Birthday = cpuApdu(0, "00b20d0004", false).Trim();
            if (cardContent.Birthday == "")
            {
                resultMsg = "读卡出错，请重新插卡";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }


            #endregion

            PSAMCARD:
            if (_mSt != 0 && _pSt != 0 && _cSt != 0)
            {
                resultMsg = "请插卡";
                mw_ic_LcdClrScrn(_icdev, 4);  //清空LCD
                mw_ic_DispInfo(_icdev, 0, 0, Encoding.Default.GetBytes("请插卡"));
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            #region 读PSAM卡
            Array.Clear(resetData, 0, 50);
            _st = sam_slt_reset(_icdev, psamCardSet, out len, resetData);
            if (_st != 0)
            {
                resultMsg = "Psam卡复位失败";
                mw_ic_LcdClrScrn(_icdev, 4);  //清空LCD
                mw_ic_DispInfo(_icdev, 0, 0, Encoding.Default.GetBytes("Psam卡复位失败"));
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            //读Psam卡序列号
            cardData = "";
            cardData = cpuApdu(psamCardSet, "00a40200020015", false);
            if (cardData == "")
            {
                resultMsg = "读Psam卡出错";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            cardData = "";
            cardData = cpuApdu(psamCardSet, "00b095010a", false);
            if (cardData == "")
            {
                resultMsg = "读Psam卡出错";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            else
            {
                //textBoxPSAMSnr.Text = cardData;
            }

            //读Psam终端编号
            cardData = "";
            cardData = cpuApdu(psamCardSet, "00a40200020016", false);
            if (cardData == "")
            {
                resultMsg = "读Psam卡出错";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }

            cardData = "";
            cardData = cpuApdu(psamCardSet, "00b0960006", false);
            if (cardData == "")
            {
                resultMsg = "读Psam卡出错";
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, resultMsg);
            }
            else
            {
                //textBoxPSAMNumber.Text = cardData;
            }

            #endregion

            resultMsg = "读卡成功";
            //dv_beep(icdev, 30);
            mw_ic_LcdClrScrn(_icdev, 4);  //清空LCD
            mw_ic_DispInfo(_icdev, 0, 0, Encoding.Default.GetBytes("读卡成功"));

            if (string.IsNullOrEmpty(cardContent.CardNumber))
            {
                return new Tuple<bool, YiBaoCardContent, string>(false, cardContent, "读卡失败，请重新插卡");
            }
            Logger.Device.Info($"社保卡读卡信息:" + cardContent.ToJsonString());
            return new Tuple<bool, YiBaoCardContent, string>(true, cardContent, resultMsg);
        }
        catch (Exception ex)
        {

            Logger.Main.Error($"读取医保设备时抛出异常,内容:{ex.Message} 堆栈:{ex.StackTrace}");
            return new Tuple<bool, YiBaoCardContent, string>(false, null, ex.Message);
        }

    }
   // */
    }

    public class YiBaoCardContent

    {
        public string Birthday { get; set; }
        public string Birthplace { get; set; }
        public string CardData { get; set; }
        /// <summary>
        /// 卡识别码
        /// </summary>
        public string CardId { get; set; }
        /// <summary>
        /// 社保卡号
        /// </summary>
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string CardValidData { get; set; }
        public string FKJG { get; set; }
        public string GFVer { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IDNumber { get; set; }
        public string Name { get; set; }
        public string Nation { get; set; }
        public string Sex { get; set; }
    }
}
