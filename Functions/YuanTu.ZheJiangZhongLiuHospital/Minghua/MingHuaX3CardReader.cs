using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.ZheJiangZhongLiuHospital.Minghua
{
    public class MingHuaX3CardReader
    {
        private const string DllPath = "MisPosDll.dll";
        [DllImport(DllPath)]
        public static extern int MisPos_Handle(int loprType, StringBuilder dest, int amt, string src);


        public bool IsEnable { get; }

        public Result<YiBaoCardContent> Read()
        {

            if (!OpenReader())
            {
                return Result<YiBaoCardContent>.Fail(-1, "读卡器打开失败");
            }
            var rest = ReadData();
            CloseReader();
            return rest;
        }


        IntPtr icdev = IntPtr.Zero;
        Int16 st, mSt, pSt, cSt;
        byte psamCardSet = 1;

        private string cpuApdu(byte cardSet, string cmdData, bool dataType)
        {

            string str = "";
            byte[] return_data = new byte[256];
            byte[] send_cmd = new byte[256];
            Int16 rLen = 0;
            byte[] strcmd = Encoding.Default.GetBytes(cmdData);
            Int16 len = (Int16)(cmdData.Length / 2); //输入的命令长度
            byte[] cmd = new byte[len];
            MingHuaDevice.asc_hex(strcmd, cmd, len);
            Array.Copy(cmd, 0, send_cmd, 3, len);  //把命令复制到send_cmd中第四个字节开始处
            send_cmd[1] = 0x40;
            send_cmd[2] = (byte)len;
            //异或
            for (int i = 0; i < len + 3; i++)
            {
                send_cmd[len + 3] = (byte)(send_cmd[len + 3] ^ send_cmd[i]);
            }

            st = MingHuaDevice.sam_slt_protocol(icdev, cardSet, (Int16)(len + 4), send_cmd, out rLen, return_data);
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
                    byte[] rec_data = new byte[500];
                    if (dataType == true)
                    {
                        Array.Copy(return_data, 3, rec_data, 0, return_data[2]);
                        str = Encoding.Default.GetString(rec_data, 0, return_data[2]);
                    }
                    else
                    {
                        MingHuaDevice.hex_asc(return_data, rec_data, rLen);
                        str = Encoding.Default.GetString(rec_data, 6, return_data[2] * 2);
                    }
                }
            }

            return str;
        }

        private bool OpenReader()
        {
            icdev = MingHuaDevice.ic_init(100/*USB*/, 115200);
            if (icdev.ToInt32() > 0)
            {
                MingHuaDevice.dv_beep(icdev, 30);//蜂鸣
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CloseReader()
        {
            try
            {
                MingHuaDevice.mw_ic_DispInfo(icdev, 0, 0, Encoding.Default.GetBytes("            "));
                MingHuaDevice.mw_ic_DispInfo(icdev, 1, 4, Encoding.Default.GetBytes("欢迎使用"));
                MingHuaDevice.mw_ic_DispInfo(icdev, 2, 4, Encoding.Default.GetBytes("智慧医疗"));

            }

            finally
            {
                MingHuaDevice.ic_exit(icdev);

            }
            return true;
        }


        private Result<YiBaoCardContent> ReadData()
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
                mSt = MingHuaDevice.rf_card(icdev, 0, cardSnr, out len);
                if (mSt == 0)
                {
                    //cardType = true;
                    byte[] Snr = new byte[15];
                    MingHuaDevice.hex_asc(cardSnr, Snr, len);
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

                    pSt = MingHuaDevice.sam_slt_reset(icdev, 0, out len, resetData);
                    if (pSt == 0)
                    {
                        break;
                    }
                    //读取4428卡
                    cSt = MingHuaDevice.chk_4428(icdev);
                    if (cSt != 0)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        byte[] SData = new byte[17];
                        cSt = MingHuaDevice.srd_4428(icdev, 64, 16, SData);  //从卡内读出的IC卡卡号
                        if (cSt == 0)
                        {
                            if (SData != null || SData.Length != 0)
                            {
                                byte[] key = Encoding.Default.GetBytes("13572468"); //密钥
                                byte[] ptrDest = new byte[50];
                                st = MingHuaDevice.ic_decrypt(key, SData, 16, ptrDest);
                                if (st == 0)
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
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //选择EF05
                cardData = cpuApdu(0, "00a4000002EF05", false).Trim();
                if (cardData == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读卡识别码
                cardContent.CardId = cpuApdu(0, "00b2010010", false).Trim().Substring(4);
                if (cardContent.CardId == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读卡类别
                cardContent.CardType = cpuApdu(0, "00b2020001", true).Trim();
                if (cardContent.CardType == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读规范版本
                cardContent.GfVer = cpuApdu(0, "00b2030004", true).Trim();
                if (cardContent.GfVer == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取发卡机构代码
                cardContent.Fkjg = cpuApdu(0, "00b204000c", false).Trim();
                if (cardContent.Fkjg == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取发卡日期
                cardContent.CardData = cpuApdu(0, "00b2050004", false).Trim();
                if (cardContent.CardData == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取卡有效期
                cardContent.CardValidData = cpuApdu(0, "00b2060004", false).Trim();
                if (cardContent.CardValidData == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                //读取卡号
                cardContent.CardNume = cpuApdu(0, "00b2070009", true).Trim();
                if (cardContent.CardNume == "" && cardContent.CardNume.Length > 2)
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                else
                {
                    cardContent.CardNume = cardContent.CardNume.Substring(2);
                }

                //选择EF06
                cardData = cpuApdu(0, "00a4000002ef06", true).Trim();
                if (cardData == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取身份证号
                cardContent.PId = cpuApdu(0, "00b2080018", true).Trim();
                if (cardContent.PId == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                else
                {
                    cardContent.PId = cardContent.PId.Substring(2);
                }

                //读取姓名
                cardContent.Name = cpuApdu(0, "00b209001e", true).Trim();
                if (cardContent.Name == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
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
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取民族
                cardContent.Nation = cpuApdu(0, "00b20b0001", false).Trim();
                if (cardContent.Nation == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取出生地
                cardContent.Birthplace = cpuApdu(0, "00b20c0003", false).Trim();
                if (cardContent.Birthplace == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读取出生日期
                cardContent.Birthday = cpuApdu(0, "00b20d0004", false).Trim();
                if (cardContent.Birthday == "")
                {
                    resultMsg = "读卡出错，请重新插卡";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }


                #endregion

                PSAMCARD:
                if (mSt != 0 && pSt != 0 && cSt != 0)
                {
                    resultMsg = "请插卡";
                    MingHuaDevice.mw_ic_LcdClrScrn(icdev, 4);  //清空LCD
                    MingHuaDevice.mw_ic_DispInfo(icdev, 0, 0, Encoding.Default.GetBytes("请插卡"));
                    return Result<YiBaoCardContent>.Fail(-1, resultMsg);
                    //return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                #region 读PSAM卡
                Array.Clear(resetData, 0, 50);
                st = MingHuaDevice.sam_slt_reset(icdev, psamCardSet, out len, resetData);
                if (st != 0)
                {
                    resultMsg = "Psam卡复位失败";
                    MingHuaDevice.mw_ic_LcdClrScrn(icdev, 4);  //清空LCD
                    MingHuaDevice.mw_ic_DispInfo(icdev, 0, 0, Encoding.Default.GetBytes("Psam卡复位失败"));
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                //读Psam卡序列号
                cardData = "";
                cardData = cpuApdu(psamCardSet, "00a40200020015", false);
                if (cardData == "")
                {
                    resultMsg = "读Psam卡出错";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                cardData = "";
                cardData = cpuApdu(psamCardSet, "00b095010a", false);
                if (cardData == "")
                {
                    resultMsg = "读Psam卡出错";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
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
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }

                cardData = "";
                cardData = cpuApdu(psamCardSet, "00b0960006", false);
                if (cardData == "")
                {
                    resultMsg = "读Psam卡出错";
                    return Result<YiBaoCardContent>.Fail(resultMsg);
                }
                else
                {
                    //textBoxPSAMNumber.Text = cardData;
                }

                #endregion

                resultMsg = "读卡成功";
                //MingHuaDevice.dv_beep(icdev, 30);
                MingHuaDevice.mw_ic_LcdClrScrn(icdev, 4);  //清空LCD
                MingHuaDevice.mw_ic_DispInfo(icdev, 0, 0, Encoding.Default.GetBytes("读卡成功"));

                if (string.IsNullOrEmpty(cardContent.CardNume))
                {
                    return Result<YiBaoCardContent>.Fail(-1, "读卡失败，请重新插卡");
                    //return new Tuple<bool, YiBaoCardContent, string>(false,cardContent, "读卡失败，请重新插卡");
                }
                Logger.Device.Info($"社保卡读卡信息:" + JsonConvert.SerializeObject(cardContent));
                return Result<YiBaoCardContent>.Success(cardContent);
                //return new Tuple<bool, YiBaoCardContent, string>(true, cardContent, resultMsg);
            }
            catch (Exception ex)
            {

                Logger.Main.Error($"读取医保设备时抛出异常,内容:{ex.Message} 堆栈:{ex.StackTrace}");
                // return new Tuple<bool, YiBaoCardContent, string>(false, null, ex.Message);
                return Result<YiBaoCardContent>.Fail(-1, ex.Message);
            }

        }
        public Result<string> ReadRfCard()
        {
            var str = new StringBuilder(1024);
            var val = MisPos_Handle(1001, str, 0, "");
            if (val == 0 && str.Length > 8)
            {
                var no = "8019" + str.ToString().Substring(str.Length - 8);
                return Result<string>.Success(no);
            }
            return Result<string>.Fail("读卡失败");
        }

        public Result<YiBaoCardContent> ReadCard()
        {
            var str = new StringBuilder(1024);
            var val = MisPos_Handle(1003, str, 0, "");
            Logger.Device.Info($"[德卡读卡]读卡结果:{val} 内容:{str}");
            if (val == 0 && str.Length > 8)
            {
                var arr = str.ToString().Split('|');
                if (arr.Length > 4)
                {
                    //卡识别码32位|卡类别1位|卡号9位|身份证号18位|姓名30位|性别1位
                    return Result<YiBaoCardContent>.Success(new YiBaoCardContent()
                    {
                        CardId = arr[0],
                        CardType = arr[1],
                        CardNume = arr[2],
                        PId = arr[4],
                        Name = arr[5].TrimEnd(),
                        Sex = arr[6]
                    });
                }

            }
            return Result<YiBaoCardContent>.Fail("读卡失败");
        }


    }
}
