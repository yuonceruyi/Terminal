using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;

namespace YuanTu.WeiHaiZXYY.CardReader
{
    public class MyHuaDaXzxIdCardReader : IIdCardReader
    {
        public string DeviceName => "MyHuaDa";
        public string DeviceId => DeviceName + "_900";

        private static int nPort = -1;

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            if (nPort > 0)
            {
                return Result.Success();
            }
            var cfg = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var port = cfg.GetValueInt("身份证:端口");
            if (port > 0)
            {
                nPort = port;
            }
            else
            {
                nPort = UnSafeMethods.Syn_FindReader();
            }

            if (nPort <= 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{nPort}");
                return Result.Fail("身份证读卡器连接异常");
            }
            var ret = UnSafeMethods.Syn_OpenPort(nPort);
            if (ret != 0)
            {
                nPort = -1;
                Logger.Device.Error($"[读卡器{DeviceId}]端口打开异常，返回值:{nPort}");
                return Result.Fail("身份证读卡器连接异常");
            }
            return Result.Success();
        }

        public Result Initialize()
        {
            if (nPort > 0)
            {
                var bytes = Encoding.Default.GetBytes(FrameworkConst.RootDirectory);
                var cPath = new byte[255];
                Array.Copy(bytes, cPath, bytes.Length);
                UnSafeMethods.Syn_SetPhotoPath(2, ref cPath[0]); //设置照片路径	iOption 路径选项	0=C:	1=当前路径	2=指定路径
                //cPhotoPath	绝对路径,仅在iOption=2时有效
                UnSafeMethods.Syn_SetPhotoType(0); //0 = bmp ,1 = jpg , 2 = base64 , 3 = WLT ,4 = 不生成
                UnSafeMethods.Syn_SetPhotoName(0); // 生成照片文件名 0=tmp 1=姓名 2=身份证号 3=姓名_身份证号

                UnSafeMethods.Syn_SetSexType(1); // 0=卡中存储的数据	1=解释之后的数据,男、女、未知
                UnSafeMethods.Syn_SetNationType(1); // 0=卡中存储的数据	1=解释之后的数据 2=解释之后加"族"
                UnSafeMethods.Syn_SetBornType(3); // 0=YYYYMMDD,1=YYYY年MM月DD日,2=YYYY.MM.DD,3=YYYY-MM-DD,4=YYYY/MM/DD
                UnSafeMethods.Syn_SetUserLifeBType(3);
                // 0=YYYYMMDD,1=YYYY年MM月DD日,2=YYYY.MM.DD,3=YYYY-MM-DD,4=YYYY/MM/DD
                UnSafeMethods.Syn_SetUserLifeEType(3, 1);
                // 0=YYYYMMDD(不转换),1=YYYY年MM月DD日,2=YYYY.MM.DD,3=YYYY-MM-DD,4=YYYY/MM/DD,
                // 0=长期 不转换,	1=长期转换为 有效期开始+50年
                return Result.Success();
            }
            return Result.Fail("身份证读卡器连接异常");
        }

        public Result UnInitialize()
        {
            if (nPort > 0)
            {
                return Result.Success();
            }
            return Result.Fail("身份证读卡器连接异常");
        }

        public Result DisConnect()
        {
            if (nPort > 0)
            {
                UnSafeMethods.Syn_ClosePort(nPort);
                nPort = -1;
                return Result.Success();
            }
            return Result.Fail("身份证读卡器连接异常");
        }

        public Result HasIdCard()
        {
            return GetIdDetail().Convert();
            //byte pucIin = 0;
            //var nRet = UnSafeMethods.Syn_StartFindIDCard(nPort, ref pucIin, 0);
            //if (nRet == 0)
            //    return Result.Success();
            //return Result.Fail("", "");
        }

        public Result<IdCardDetail> GetIdDetail()
        {
            byte pucIin = 0;
            var nRet = UnSafeMethods.Syn_StartFindIDCard(nPort, ref pucIin, 0);
            //if (nRet<0)
            //{
            //    return Result<IdCardDetail>.Fail("寻卡失败，请重新放置身份证");
            //}
            var cardMsg = new UnSafeMethods.IdCardData();
            nRet = UnSafeMethods.Syn_SelectIDCard(nPort, ref pucIin, 0);
            nRet = UnSafeMethods.Syn_ReadMsg(nPort, 0, ref cardMsg);
            if (!cardMsg.Name.IsNullOrWhiteSpace())
            {
                try
                {
                    //File.Delete(cardMsg.PhotoFileName);
                }
                catch (Exception)
                {
                }
                Logger.Device.Info($"{cardMsg.Name},{cardMsg.Nation},{cardMsg.Born},{cardMsg.UserLifeBegin},{cardMsg.UserLifeEnd}");
                return Result<IdCardDetail>.Success(new IdCardDetail()
                {
                    Name = cardMsg.Name.Trim(),
                    Sex = (cardMsg.Sex == "未知") ? Sex.未知 : (cardMsg.Sex == "男" ? Sex.男 : Sex.女),
                    Nation = cardMsg.Nation,
                    Birthday = DateTime.ParseExact(cardMsg.Born, "yyyy-MM-dd", null),
                    Address = cardMsg.Address.Trim(),
                    IdCardNo = cardMsg.IDCardNo,
                    GrantDept = cardMsg.GrantDept,
                    EffectiveDate = DateTime.ParseExact(cardMsg.UserLifeBegin, "yyyy-MM-dd", null),
                    ExpireDate = DateTime.ParseExact(cardMsg.UserLifeEnd, "yyyy-MM-dd", null),
                    PortraitPath = cardMsg.PhotoFileName
                });
            }
            return Result<IdCardDetail>.Fail("读卡失败，请重新放置身份证");
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }
    }

}
