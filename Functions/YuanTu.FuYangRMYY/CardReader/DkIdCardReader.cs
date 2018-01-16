using System;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;

namespace YuanTu.FuYangRMYY.CardReader
{
    public class DkIdCardReader : IIdCardReader
    {
        private static int nPort = -1;
        public string DeviceName => "DK";
        public string DeviceId => DeviceName + "_dk";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            var errMsg = new StringBuilder(1024);
            nPort = UnSafeMethods.iOpenPort(errMsg);
            if (nPort != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{nPort},错误内容:{errMsg}");
                return Result.Fail("身份证读卡器连接异常");
            }
            return Result.Success();
        }

        public Result Initialize()
        {
            return Result.Success();
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            var errMsg = new StringBuilder(1024);
            nPort = UnSafeMethods.iClosePort(errMsg);
            if (nPort != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]关闭异常，接口返回值:{nPort},错误内容:{errMsg}");
                return Result.Fail("身份证读卡器关闭异常");
            }
            return Result.Success();
        }

        public Result HasIdCard()
        {
            return GetIdDetail().Convert();
        }

        public Result<IdCardDetail> GetIdDetail()
        {
            var pBmpFile = string.Empty;
            var pname = new StringBuilder(1024);
            var pSex = new StringBuilder(1024);
            var pNation = new StringBuilder(1024);
            var pBirth = new StringBuilder(1024);
            var pAddress = new StringBuilder(1024);
            var pCertNo = new StringBuilder(1024);
            var pDepartment = new StringBuilder(1024);
            var pExpire = new StringBuilder(1024);
            var pErrMsg = new StringBuilder(1024);
            nPort = UnSafeMethods.LPub_IC_CertCardInfos(pBmpFile, pname, pSex, pNation, pBirth, pAddress, pCertNo,
                pDepartment, pExpire, pErrMsg);
            if (nPort != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]读取失败，接口返回值:{nPort},错误内容:{pErrMsg}");
                return Result<IdCardDetail>.Fail("读卡失败，请重新放置身份证");
            }
            Logger.Device.Info($"读取身份证成功 返回值:{nPort} 返回数据:[姓名:{pname}\n" +
                               $"性别:{pSex}\n" +
                               $"民族:{pNation}\n" +
                               $"出生日期:{pBirth}\n" +
                               $"户籍地址:{pAddress}\n" +
                               $"身份证号:{pCertNo}\n" +
                               $"签发机关:{pDepartment}\n" +
                               $"截止有效期:{pExpire}\n]");
            return Result<IdCardDetail>.Success(new IdCardDetail
            {
                Name = pname.ToString().Trim(),
                Sex = pSex.ToString().Trim() == "未知" ? Sex.未知 : (pSex.ToString().Trim() == "男" ? Sex.男 : Sex.女),
                Nation = pNation.ToString().Trim(),
                Birthday = DateTime.ParseExact(pBirth.ToString().Trim(), "yyyy-MM-dd", null),
                Address = pAddress.ToString().Trim(),
                IdCardNo = pCertNo.ToString().Trim(),
                GrantDept = pDepartment.ToString().Trim(),
                EffectiveDate = new DateTime(),
                ExpireDate = DateTime.ParseExact(pExpire.ToString().Trim(), "yyyy-MM-dd", null)
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
}