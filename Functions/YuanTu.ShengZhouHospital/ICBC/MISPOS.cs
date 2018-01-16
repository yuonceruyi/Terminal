using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts;
using YuanTu.Core.Log;

namespace YuanTu.ShengZhouHospital.ICBC
{
	public static class Mispos
	{
	    public const string IcbcDllPath  = @"C:\Windows\icbc\MposCore.dll";
        [DllImport(IcbcDllPath, EntryPoint = "DoICBCZJMisTranSTD", CharSet = CharSet.Ansi,
			CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.BStr)]
		private static extern string DoICBCZJMisTranSTD(string id, string preinput, string rsv1, string rsv2, string rsv3);

		public static bool DoSale(int count, out Output output)
		{
            try
            {
                var da = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory=FrameworkConst.RootDirectory,
                        FileName = "icbc.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Arguments ="Sale "+count.ToString()
                    }
                };
                da.Start();
                var outputStr=da.StandardOutput.ReadToEnd();
                Logger.POS.Info($"交易返回信息{outputStr}");
                if (!string.IsNullOrEmpty(outputStr) &&outputStr.Contains("Ret="))
                {
                    var ret = Decipher(outputStr.Replace("Ret=",""), out output);
                    return ret;
                }
                output = new Output();
                output.错误信息 ="非正常输出";
                return false;
            }
            catch (System.Exception ex)
            {
                Logger.POS.Error($"[消费]res:{ex.Message}");
                output = new Output();
                output.错误信息 = ex.Message;
                return false;
            }
		   
		}

		public static bool DoCancel(string centerSeq, int count, out Output output)
		{
            Logger.POS.Info($"[撤销]req:centerSeq={centerSeq},count={count}");
            try
            {
                var da = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory=FrameworkConst.RootDirectory,
                        FileName = "icbc.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Arguments =centerSeq+" "+count.ToString()
                    }
                };
                da.Start();
                var outputStr = da.StandardOutput.ReadToEnd();
                Logger.POS.Info($"撤销返回信息{outputStr}");
                if (!string.IsNullOrEmpty(outputStr) && outputStr.Contains("Ret="))
                {
                    var ret = Decipher(outputStr.Replace("Ret=", ""), out output);
                    return ret;
                }
                output = new Output();
                output.错误信息 = "非正常输出";
                Logger.POS.Info($"[撤销]res:{output}");
                return false;
            }
            catch (System.Exception ex)
            {
                Logger.POS.Error($"[消费]res:{ex.Message}");
                output = new Output();
                output.错误信息 = ex.Message;
                return false;
            }
		}

		private static bool Decipher(string text, out Output output)
		{
			output = Output.Decipher(text);
			return output.成功;
		}

		public class Output
		{
			public string Text;
			public string 备注信息;
			public string 错误信息;
			public string 发卡行名称;
			public string 返回码;
			public string 分期期数;
			public string 检索参考号;
			public string 交易金额;
			public string 交易日期;
			public string 交易时间;
			public string 卡号;
			public string 卡片类型;
			public string 卡片有效期;
			public string 批次号;
			public string 清算日期;
			public string 授权号;
			public string 终端编号;
			public string 终端流水号;

			public bool 成功
			{
				get { return 返回码 == "00"; }
			}

			public static Output Decipher(string text)
			{
				if (!text.StartsWith("00"))
				{
					return new Output
					{
						Text = text,
						返回码 = text.Substring(0, 2),
						错误信息 = text.Substring(2)
					};
				}
				return new Output
				{
					Text = text,
					返回码 = text.Substring(0, 2),
					卡号 = text.Substring(2, 19),
					交易日期 = text.Substring(21, 8),
					交易时间 = text.Substring(29, 6),
					终端流水号 = text.Substring(35, 6),
					批次号 = text.Substring(41, 6),
					清算日期 = text.Substring(47, 6),
					检索参考号 = text.Substring(53, 8),
					卡片有效期 = text.Substring(61, 4),
					交易金额 = text.Substring(65, 12),
					终端编号 = text.Substring(77, 15),
					授权号 = text.Substring(92, 6),
					分期期数 = text.Substring(98, 2),
					发卡行名称 = text.Substring(100, 20),
					卡片类型 =text.Substring(120, 20),
					备注信息 =text.Substring(140)
				};
			}

			public override string ToString()
			{
				var text = new StringBuilder();
				if (!成功)
				{
					text.Append("返回码      :" + 返回码 + "\n");
					text.Append("错误信息    :" + 错误信息 + "\n");
					return text.ToString();
				}
				text.Append("返回码      :" + 返回码 + "\n");
				text.Append("卡号        :" + 卡号 + "\n");
				text.Append("交易日期    :" + 交易日期 + "\n");
				text.Append("交易时间    :" + 交易时间 + "\n");
				text.Append("终端流水号  :" + 终端流水号 + "\n");
				text.Append("批次号      :" + 批次号 + "\n");
				text.Append("清算日期    :" + 清算日期 + "\n");
				text.Append("检索参考号  :" + 检索参考号 + "\n");
				text.Append("卡片有效期  :" + 卡片有效期 + "\n");
				text.Append("交易金额    :" + 交易金额 + "\n");
				text.Append("终端编号    :" + 终端编号 + "\n");
				text.Append("授权号      :" + 授权号 + "\n");
				text.Append("分期期数    :" + 分期期数 + "\n");
				text.Append("发卡行名称  :" + 发卡行名称 + "\n");
				text.Append("卡片类型    :" + 卡片类型 + "\n");
				text.Append("备注信息    :" + 备注信息 + "\n");
				return text.ToString();
			}
		}
	}
}