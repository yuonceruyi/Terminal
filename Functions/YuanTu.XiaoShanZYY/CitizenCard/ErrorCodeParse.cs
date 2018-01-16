using System;
using System.Collections.Generic;
using YuanTu.Core.Log;

namespace YuanTu.XiaoShanZYY.CitizenCard
{
    public class ErrorCodeParse
    {
        private static readonly Dictionary<int, string> ErrorDictionary = new Dictionary<int, string>
            {
                {0, "交易成功"},
                {1, "交易被冲正"},
                {2, "可疑交易"},
                {3, "交易失败"},
                {4, "网络错误"},
               // {5, "密码键盘错误"},
                {5,"市民卡网络异常，请稍候重试"},
                {6, "参数错误"},
                {7, "无法获取市民卡后台应答"},
                {11, "未开通该功能"},
                {12, "余额不足"},
                {13, "卡错误"},
                {15, "连续充值"},
                {17, "输入卡号同读卡信息不匹配"},
                {18, "校验失败"},
            };

        private static readonly Dictionary<string, string> RespondDictionary = new Dictionary<string, string>
            {
                {"00", "承兑或交易成功"},
                {"03","无效商户"},
                {"05","无效终端"},
                {"08","终端未签到"},
                {"12","无效交易"},
                {"13","无效金额"},
                {"14","无效卡号"},
                {"15","卡状态不正确，已挂失或冻结"},
                {"16","卡账户不存在"},
                {"17","账户状态不正确"},
                {"18","账户未启用"},
                {"20","无效应答"},
                {"22","原交易已被冲正或被撤销"},
                {"25","未能找到文件上记录"},
                {"30","格式错误"},
                {"31","当日消费金额超限"},
                {"32","单笔交易金额超限"},
                {"36","帐户类型不存在"},
                {"37","帐户余额密文校验错"},
                {"38","超过允许的PIN试输入"},
                {"39","当天交易不允许退货"},
                {"41","参数未设置或不允许下载"},
                {"42","SAM商户号不匹配"},
                {"51","无足够的存款"},
                {"52","预授权完成金额大于预授权金额"},
                {"54","已过有效期"},
                {"55","不正确的PIN"},
                {"61","超出消费金额限制"},
                {"63","无效的金额"},
                {"64","原始金额不正确"},
                {"68","收到的回答太迟"},
                {"75","允许的输入PIN次数超限"},
                {"76","两次输入密码不一致"},
                {"77","无效的刮刮卡"},
                {"78","刮刮卡不是激活状态"},
                {"79","刮刮卡密码不正确"},
                {"88","账户启用成功"},
                {"96","系统异常、失效"},
                {"97","POS终端号找不到"},
                {"99","PIN格式错"},
                {"A0","MAC校验错"},
                {"A1","卡类型错误。非记名消费卡"},
                {"C1","未开通医院应用，在医院应用"},
                {"B0","卡验证，该卡已开通市民卡账户功能"},
                {"B1","卡验证，该卡卡管状态不正确"},
                {"B2","市民卡开户，该卡不允许重复开同类型账户"},
                {"B3","卡验证，该卡卡管中未找到"},
            };
        public static string ErrorParse(int errorCode)
        {
            try
            {
                return ErrorDictionary[errorCode];
            }
            catch (Exception ex)
            {
                Logger.Net.Error(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        public static string RespondParse(string respondCode)
        {
            try
            {
                return RespondDictionary[respondCode];
            }
            catch (Exception ex)
            {
                Logger.Net.Error(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }
    }


}


