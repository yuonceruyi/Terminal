using System;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.NingXiaHospital.CardReader.DkT10
{
    public class Dkt10
    {
        public Result<IdCardModel> Read()
        {
            var idCardModel = new IdCardModel();
            int icdev;
            int st;
            byte[] snr = new byte[128];
            byte rlen = 0;
            byte[] rbuff = new byte[128];
            icdev = DllHandler.dc_init(100, 115200);
            if (icdev < 0)
            {
                return Result<IdCardModel>.Fail("德卡读卡器连接失败");
            }

            st = DllHandler.dc_setcpu(icdev, 0x0c);
            if (st != 0)
            {
                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("德卡读卡器设置命令失败");
            }

            st = DllHandler.dc_cpureset(icdev, ref rlen, snr);
            if (st != 0)
            {

                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("德卡读卡器重置命令失败");
            }

            var reqByes = StrToToHexByte("00a404000f7378312e73682ec9e7bbe1b1a3d5cf");
            st = DllHandler.dc_cpuapdu(icdev, Convert.ToByte(reqByes.Length), reqByes, ref rlen, rbuff);
            if (st != 0)
            {
                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("设置医保卡主文件命令失败");
            }
            reqByes = StrToToHexByte("00a4000002ef06");
            st = DllHandler.dc_cpuapdu(icdev, Convert.ToByte(reqByes.Length), reqByes, ref rlen, rbuff);
            if (st != 0)
            {
                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("设置医保卡次文件命令失败");
            }
            reqByes = StrToToHexByte("00b2080014");
            st = DllHandler.dc_cpuapdu(icdev, Convert.ToByte(reqByes.Length), reqByes, ref rlen, rbuff);
            if (st != 0)
            {
                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("医保卡读取身份证命令失败");
            }
            var resStr = Encoding.Default.GetString(rbuff,2, Convert.ToInt32(rlen)).Split('?')[0];
            idCardModel.IdCardNo = resStr;
            idCardModel.Birthday = DateTime.Parse(resStr.Substring(6, 8).Insert(4, "-").Insert(7, "-"));
            reqByes = StrToToHexByte("00b2090020");
            st = DllHandler.dc_cpuapdu(icdev, Convert.ToByte(reqByes.Length), reqByes, ref rlen, rbuff);
            if (st != 0)
            {
                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("医保卡读取姓名命令失败");
            }
            idCardModel.Name = Encoding.Default.GetString(rbuff, 2, Convert.ToInt32(rlen)).Split('?')[0].Replace("\u0000","");
            reqByes = StrToToHexByte("00b20a0003");
            st = DllHandler.dc_cpuapdu(icdev, Convert.ToByte(reqByes.Length), reqByes, ref rlen, rbuff);
            if (st != 0)
            {
                DllHandler.dc_exit(icdev);
                return Result<IdCardModel>.Fail("医保卡读取性别命令失败");
            }
            idCardModel.Sex = Encoding.Default.GetString(rbuff, 2, Convert.ToInt32(rlen)).Split('?')[0] == "1" ? Sex.男 : Sex.女;
            DllHandler.dc_beep(icdev, 10);
            DllHandler.dc_exit(icdev);

            Logger.Device.Info($"社保卡读卡信息:{idCardModel.ToJsonString()}");
            return Result<IdCardModel>.Success(idCardModel);
        }

        public static String ByteToChar(int length, byte[] data)
        {
            StringBuilder stringbuiler = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                String temp = data[i].ToString("x");
                if (temp.Length == 1)
                {
                    stringbuiler.Append("0" + temp);
                }
                else
                {
                    stringbuiler.Append(temp);
                }
            }
            return (stringbuiler.ToString());
        }

        private static byte[] StrToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
}
