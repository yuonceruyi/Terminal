using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.JiaShanHospital.HealthInsurance
{
	public static class MtReader
	{

	    public static int icdev;
	    public static short st;
	    public static string Card;
        private static readonly Dictionary<int, string> ErrCode = new Dictionary<int, string>
		{
			{0, "正常"},
			{-200001, "打开端口错误"},
			{-200002, "关闭端口错误"},
			{-200013, "CPU卡复位错误"},
			{-200014, "CPU卡通讯错误,"},
			{-200023, "CPU卡片尚未插入"},
			{-200024, "CPU卡片尚未取出"},
			{-200025, "CPU卡片无应答"},
			{-200026, "CPU卡不支持该命令"},
			{-200027, "CPU卡命令长度错误"},
			{-200028, "CPU卡命令参数错误"},
			{-200029, "CPU卡访问权限不满足"},
			{-200030, "CPU卡信息校验和出错"},
			{-200060, "密码键盘输入错误"},
			{-200061, "密码键盘输入超时"},
			{-200062, "密码键盘输入取消"},
			{-200069, "个人密码认证失败，只允许一次出错机会"},
			{-200070, "个人密码认证失败，只允许两次出错机会"},
			{-200071, "个人密码认证失败，只允许三次出错机会"},
			{-200072, "个人密码认证失败，只允许四次出错机会"},
			{-200073, "个人密码认证失败，只允许五次出错机会"},
			{-200074, "个人密码已经锁定"},
			{-200075, "输入密码长度错误"},
			{-200076, "社保PSAM卡复位错误"},
			{-200077, "内部认证错误"},
			{-200078, "外部认证错误"},
			{-200100, "动态库加载错误"}
		};
        public static Result<string> Read()
		{
            try
            {
                var sb = new StringBuilder(4096);
                var ret = UnSafeMethods.IC_ReadCardInfo_NoPin(sb);
                if (ret != 0)
                {
                    string message;
                    ErrCode.TryGetValue(ret, out message);
                    Logger.Device.Error($"[嘉善社保]读卡失败 返回信息:{ret} {message}");
                    //Logger.Main.Error($"读取社保卡失败 返回信息:{ret} {message}");
                    return Result<string>.Fail(ret, message);
                }
                Logger.Device.Info("[嘉善社保]读取社保卡成功 返回信息:" + sb);
                return Result<string>.Success(sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[嘉善社保]读卡失败 {ex.Message} {ex.StackTrace}");
                return Result<string>.Fail(ex.Message);
            }
			
		}

	    [DllImport("mt_32.dll", EntryPoint = "open_device", SetLastError = true,
	        CharSet = CharSet.Auto, ExactSpelling = false,
	        CallingConvention = CallingConvention.StdCall)]
	    //说明：打开通讯接口
	    public static extern int open_device(byte nPort, int ulBaud);

	    [DllImport("mt_32.dll", EntryPoint = "close_device", SetLastError = true,
	        CharSet = CharSet.Auto, ExactSpelling = false,
	        CallingConvention = CallingConvention.StdCall)]
	    //说明：    关闭通讯口
	    public static extern Int16 close_device(int icdev);

	    [DllImport("mt_32.dll", EntryPoint = "OpenCard", SetLastError = true,
	        CharSet = CharSet.Auto, ExactSpelling = false,
	        CallingConvention = CallingConvention.StdCall)]
	    public static extern Int16 OpenCard(int icdev, byte mode, byte[] snr, byte[] sCardInfo, ref byte nAtrLen);

	    [DllImport("mt_32.dll", EntryPoint = "CloseCard", SetLastError = true,
	        CharSet = CharSet.Auto, ExactSpelling = false,
	        CallingConvention = CallingConvention.StdCall)]
	    public static extern Int16 CloseCard(int icdev);

	    [DllImport("mt_32.dll", EntryPoint = "ExchangePro", SetLastError = true,
	        CharSet = CharSet.Auto, ExactSpelling = false,
	        CallingConvention = CallingConvention.StdCall)]
	    public static extern Int16 ExchangePro(int icdev, byte[] cmd, ushort cmdLen, byte[] ret, ref ushort retLen);

	    public static string Transmit(string text)
	    {
	        var sendbuff = new byte[256];
	        ushort sendlen = 0;
	        var recvbuff = new byte[256];
	        ushort recvlen = 256;

	        for (var i = 0; i < text.Length; i += 2)
	        {
	            sendbuff[i / 2] = Convert.ToByte(16 * Char2Byte(text[i]) + Char2Byte(text[i + 1]));
	            sendlen++;
	        }

	        st = ExchangePro(icdev, sendbuff, sendlen, recvbuff, ref recvlen);
	        return Bytes2String(recvbuff, recvlen);
	    }

	    public static string JiaXinCardNo()
	    {
	        var CardNo = "";
	        string text;
	        text = Transmit("00A404000F7378312E73682EC9E7BBE1B1A3D5CF");
	        Logger.Net.Debug($"1:{text}");
	        if (st != 0 || !text.EndsWith("9000"))
	        {
	            return null;
	        }
	        text = Transmit("00A4020002EF05");
	        Logger.Net.Debug($"2:{text}");
	        if (st != 0 || !text.EndsWith("9000"))
	            return null;
	        //CardNo += text.Substring(0, text.Length - 4);
	        text = Transmit("00B2070000");
	        Logger.Net.Debug($"3:{text}");
	        if (st != 0 || !text.EndsWith("9000"))
	            return null;
	        CardNo = text; //+= text.Substring(0, text.Length - 4);
	        return CardNo;
	    }

	    public static string Bytes2String(byte[] buff, int len)
	    {
	        var text = "";
	        for (var i = 0; i < len; i++)
	            text += String.Format("{0:X2}", buff[i]);
	        return text;
	    }

	    private static byte Char2Byte(char c)
	    {
	        if (c >= '0' && c <= '9')
	            return Convert.ToByte(c - '0');
	        if (c >= 'a' && c <= 'f')
	            return Convert.ToByte(c - 'a' + 10);
	        if (c >= 'A' && c <= 'F')
	            return Convert.ToByte(c - 'A' + 10);
	        return Convert.ToByte(-1);
	    }

	    public static string ReadSMK()
	    {
	        //return null;
	        byte recLen = 0;
	        if (icdev == 0)
	            icdev = open_device(0, 9600);
	        if (icdev < 0)
	        {
	            var log = "市民卡读卡器初始化失败";
	            st = close_device(icdev);
	            icdev = 0;
	            Logger.Main.Error(log);
	        }

	        var snr = new byte[10];
	        var recData = new byte[40];
	        st = OpenCard(icdev, 0, snr, recData, ref recLen);
	        Logger.Main.Info($"Read:{BitConverter.ToString(recData)} {recLen}");
	        if (st < 0)
	        {
	            Logger.Main.Info("打开市民卡失败");
	            st = CloseCard(icdev);
	            st = close_device(icdev);
	            icdev = 0;
	            return null;
	        }
	        var card = JiaXinCardNo();
	        if (!string.IsNullOrEmpty(card))
	        {
	            Card = card;
	            Logger.Main.Info("读取市民卡: " + card);
	        }
	        else
	        {
	            Logger.Main.Info("读取市民卡为空");
	        }

	        st = CloseCard(icdev);
	        st = close_device(icdev);
	        icdev = 0;

	        return card;
	    }

    }
}