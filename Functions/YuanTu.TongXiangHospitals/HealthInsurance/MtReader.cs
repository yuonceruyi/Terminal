using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.TongXiangHospitals.HealthInsurance
{
    public static class MtReader
    {
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
                lock(ErrCode)
                {
                    var sb = new StringBuilder(4096);
                    var ret = UnSafeMethods.IC_ReadCardInfo_NoPin(sb);
                    if (ret != 0)
                    {
                        string message;
                        ErrCode.TryGetValue(ret, out message);
                        Logger.Device.Error($"[桐乡社保]读卡失败 返回信息:{ret} {message}");
                        return Result<string>.Fail(ret, message);
                    }
                    Logger.Device.Info("[桐乡社保]读取社保卡成功 返回信息:" + sb);
                    return Result<string>.Success(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Device.Error($"[桐乡社保]读卡失败 {ex.Message} {ex.StackTrace}");
                return Result<string>.Fail(ex.Message);
            }
        }
    }
}