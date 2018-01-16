using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.BillPay.Services
{
    public static class PrescriptionLock
    {
        private const string loclFile = "加锁处方单.json";

        public static Result AddLock(病人信息 info, string prescriptionNo)
        {
            try
            {
                var req = new Req收费加锁解锁()
                {
                    患者唯一标识 = info.patientId,
                    姓名 = info.name,
                    处方单号 = prescriptionNo,
                    加锁标志 = "1"
                };
                var res = HisHandleEx.执行收费加锁解锁(req);
                if (res.IsSuccess)
                {
                    File.WriteAllText(loclFile, req.ToJsonString());
                    return Result.Success();
                }
                return Result.Fail(res.Message);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"锁定收费处方时发生异常:{ex.Message} {ex.StackTrace}");
                return Result.Fail("保存锁定数据时发生异常，请稍后再试！");
            }
        }

        public static bool HasLock()
        {
            return File.Exists(loclFile);
        }
        public static void RemoveLock()
        {
            if (!HasLock())
            {
             return;   
            }
            try
            {
                var req = File.ReadAllText(loclFile).ToJsonObject<Req收费加锁解锁>();
                req.加锁标志 = "0";
                var res = HisHandleEx.执行收费加锁解锁(req);
            }
            catch (Exception)
            {

            }
            File.Delete(loclFile);

        }
    }
}
