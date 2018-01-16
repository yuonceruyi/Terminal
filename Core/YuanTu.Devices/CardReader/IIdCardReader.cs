using System;
using System.IO;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Devices.CardReader
{
    public interface IIdCardReader : IDevice
    {
        /// <summary>
        /// 判断是读卡器上是否已经放置身份证
        /// </summary>
        /// <returns></returns>
        Result HasIdCard();

        /// <summary>
        /// 获取身份证具体内容
        /// </summary>
        /// <returns></returns>
        Result<IdCardDetail> GetIdDetail();
        /// <summary>
        /// 手动设置handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result SetHandle(int handle);
        /// <summary>
        /// 手动获取handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result<int> GetHandle();


    }

    public class XzxIdCardReader : IIdCardReader
    {
        public string DeviceName => "Xzx";
        public string DeviceId => DeviceName + "_XZX";

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
            nPort = UnSafeMethods.Syn_FindUSBReader();
            if (nPort <= 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{nPort}");
                return Result.Fail("身份证读卡器连接异常");
            }
            var ret = UnSafeMethods.Syn_OpenPort(nPort);
            if (ret < 0)
            {
                nPort = -1;
                Logger.Device.Error($"[读卡器{DeviceId}]端口打开异常，返回值:{nPort}");
                return Result.Fail("身份证读卡器连接异常");
            }
            return Result.Success();
        }

        public Result Initialize()
        {
            if (nPort <= 0)
                return Result.Fail("身份证读卡器连接异常");
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

        public Result UnInitialize()
        {
            if (nPort <= 0)
                return Result.Fail("身份证读卡器连接异常");
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (nPort <= 0)
                return Result.Fail("身份证读卡器连接异常");
            UnSafeMethods.Syn_ClosePort(nPort);
            nPort = -1;
            return Result.Success();
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
            if (cardMsg.Name.IsNullOrWhiteSpace())
            {
                Logger.Device.Error($"[读卡器{DeviceId}]读取失败，接口返回值:{nRet}");
                return Result<IdCardDetail>.Fail("读卡失败，请重新放置身份证");
            }
                
            try
            {
                Logger.Device.Info($"[读卡器{DeviceId}]获得身份结果：{cardMsg.ToJsonString()}");
                File.Delete(cardMsg.PhotoFileName);
            }
            catch (Exception)
            {
                //
            }
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
            });
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

    public class HuaDaXzxIdCardReader : IIdCardReader
    {
        public string DeviceName => "HuaDa";
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
            if (port>0)
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
                    File.Delete(cardMsg.PhotoFileName);
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

    public class HuaDaIdCardReader : IIdCardReader
    {
        public string DeviceName => "HuaDa";
        public string DeviceId => DeviceName + "_HUADA";

        private int _handle;
        private int _ret;
        private string _deviceName = "USB1";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            _handle = UnSafeMethods.ICC_Reader_Open(_deviceName);
            if (_handle > 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{_handle}");
            return Result.Fail("身份证读卡器连接异常");
        }

        public Result Initialize()
        {
            _ret = UnSafeMethods.ICC_PosBeep(_handle, 30);
            if (_ret >= 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]初始化（蜂鸣）异常，接口返回值:{_ret}");
            return Result.Fail("身份证读卡器初始化（蜂鸣）异常");
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            _ret = UnSafeMethods.ICC_Reader_Close(_handle);
            if (_ret == 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]断开连接异常，接口返回值:{_ret}");
            return Result.Fail("身份证读卡器断开连接异常");
        }

        public Result HasIdCard()
        {
            return GetIdDetail().Convert();
        }

        public Result<IdCardDetail> GetIdDetail()
        {
            var pBmpFile = new StringBuilder(null);
            var pName = new byte[10];
            var pSex = new byte[2];
            var pBirth = new byte[8];
            var pNation = new byte[10];

            //var pName = new StringBuilder();
            //var pSex = new StringBuilder();
            //var pBirth = new StringBuilder();
            //var pNation = new StringBuilder();
            var pAddress = new StringBuilder();
            var pCertNo = new StringBuilder();
            var pDepartment = new StringBuilder();
            var pEffectData = new StringBuilder();
            var pExpire = new StringBuilder();
            var pErrMsg = new StringBuilder();

            _ret= UnSafeMethods.PICC_Reader_ReadIDMsg(_handle, pBmpFile, pName, pSex, pNation, pBirth, pAddress, pCertNo, pDepartment, pEffectData, pExpire, pErrMsg);

            if (_ret==0)
            {
                return Result<IdCardDetail>.Success(new IdCardDetail()
                {
                    Name = Encoding.Default.GetString(pName).Trim('\0'),
                    Sex = Encoding.Default.GetString(pSex) == "男" ? Sex.男 : Sex.女,
                    Nation = Encoding.Default.GetString(pNation).Trim('\0'),
                    Birthday = DateTime.ParseExact(Encoding.Default.GetString(pBirth).Insert(4, "-").Insert(7, "-"), "yyyy-MM-dd", null),
                    Address = pAddress.ToString().Trim(),
                    IdCardNo = pCertNo.ToString().Trim(),
                    GrantDept = pDepartment.ToString().Trim(),
                    EffectiveDate = DateTime.ParseExact(pEffectData.ToString().Trim().Insert(4, "-").Insert(7, "-"), "yyyy-MM-dd", null),
                    ExpireDate = DateTime.ParseExact(pExpire.ToString().Trim().Insert(4, "-").Insert(7, "-"), "yyyy-MM-dd", null),
                });
            }
            return Result<IdCardDetail>.Fail("读卡失败，请重新放置身份证");
        }

        public Result SetHandle(int handle)
        {
            _handle = handle;
            return Result.Success();
        }

        public Result<int> GetHandle()
        {
            return Result<int>.Success(_handle);
        }
    }

    public class HuaDa900IdCardReader : IIdCardReader
    {
        public string DeviceName => "HuaDa900";
        public string DeviceId => DeviceName + "_Id";
        private static int handle = -1;
        private static int port = -1;
        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            if (handle > 0)
            {
                return Result.Success();
            }
            var cfg = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            port = cfg.GetValueInt("身份证:端口");
            if (port <= 0)
            {
                return Result.Fail("请正确配置读卡器端口");
            }

            var ret = UnSafeMethods.HD_InitComm(port);
            if (ret != 0)
            {
                handle = -1;
                Logger.Device.Error($"[读卡器{DeviceId}]端口打开异常，返回值:{ret} {GetErrorDetail(ret)}");
                return Result.Fail($"身份证读卡器连接异常，错误原因:{GetErrorDetail(ret)}");
            }
            handle = 100;
            return Result.Success();
        }

        public Result Initialize()
        {
            if (handle > 0)
            {
                return Result.Success();
            }
            return Result.Fail("接口未初始化");
        }

        public Result UnInitialize()
        {
            if (handle > 0)
            {
                return Result.Success();
            }
            return Result.Fail("接口未初始化");
        }

        public Result DisConnect()
        {
            if (handle>0)
            {
                UnSafeMethods.HD_CloseComm(port);
                handle = -1;
            }
            return Result.Success();
        }

        public Result HasIdCard()
        {
            if (handle > 0)
            {
                var ret = UnSafeMethods.HD_Authenticate();
                if (ret==0)
                {
                    return Result.Success();
                }
                return Result.Fail(ret,GetErrorDetail(ret));
            }
            return Result.Fail("接口未初始化");
        }

        public Result<IdCardDetail> GetIdDetail()
        {
            if (handle > 0)
            {
                var pname=new StringBuilder();
                var psex=new StringBuilder();
                var pNation = new StringBuilder();
                var pBirth = new StringBuilder();
                var pAddress = new StringBuilder();
                var pCertNo = new StringBuilder();//公民身份证号码 
                var pDepartment = new StringBuilder();//签发机关 
                var pEffectData = new StringBuilder();//有效期开始日期 
                var pExpire = new StringBuilder();//有效期结束日期 
                var ret = UnSafeMethods.HD_Read_BaseInfo(null,null,pname,psex,pNation,pBirth,pAddress,pCertNo,pDepartment,pEffectData,pExpire);
                Logger.Device.Info($"{DeviceId}读取身份信息:{pname} { psex} { pNation} { pBirth} { pAddress} { pCertNo} { pDepartment} { pEffectData} { pExpire}");
                if (ret == 0)
                {
                    return Result<IdCardDetail>.Success(new IdCardDetail()
                    {
                        Name = pname.ToString().Trim(),
                        Sex = (psex.ToString().Trim() == "未知") ? Sex.未知 : (psex.ToString().Trim() == "男" ? Sex.男 : Sex.女),
                        Nation = pNation.ToString().Trim(),
                        Birthday = DateTime.ParseExact(pBirth.ToString().Trim(), "yyyyMMdd", null),
                        Address = pAddress.ToString().Trim(),
                        IdCardNo = pCertNo.ToString().Trim(),
                        GrantDept = pDepartment.ToString().Trim(),
                        EffectiveDate = DateTime.ParseExact(pEffectData.ToString().Trim(), "yyyyMMdd", null),
                        ExpireDate = DateTime.ParseExact(pExpire.ToString().Trim(), "yyyyMMdd", null),
                    });
                }
                return Result<IdCardDetail>.Fail(ret, GetErrorDetail(ret));
            }
            return Result<IdCardDetail>.Fail("接口未初始化");
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        private string GetErrorDetail(int ret)
        {
            if (UnSafeMethods.HuaDa900_ErrorDictionary.ContainsKey(ret))
            {
                return UnSafeMethods.HuaDa900_ErrorDictionary[ret];
            }
            else
            {
                return "未知异常，状态码:" + ret;
            }
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }
    }

    public class IdCardDetail
    {
        /// <summary>
        /// 身份证姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// 民族
        /// </summary>
        public string Nation { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// 家庭住址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdCardNo { get; set; }

        /// <summary>
        /// 发证机关
        /// </summary>
        public string GrantDept { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// 肖像地址
        /// </summary>
        public string PortraitPath { get; set; }
    }
}