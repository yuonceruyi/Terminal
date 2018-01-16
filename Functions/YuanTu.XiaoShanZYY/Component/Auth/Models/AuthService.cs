using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.XiaoShanZYY.CitizenCard;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Auth.Models
{
    public interface IAuthService : IService
    {
        Result 读市民卡健康卡();
        Result 账户查询();
        Result 人员信息查询();
    }

    class AuthService: IAuthService
    {
        public string ServiceName { get; }

        public IAuthModel GetAuthModel() => ServiceLocator.Current.GetInstance<IAuthModel>();
        
        public Result 读市民卡健康卡()
        {
            var Auth = GetAuthModel();
            Auth.Info = null;
            var result = DataHandler.读接触非接卡号(new Req读接触非接卡号());
            if (!result.IsSuccess)
                return result.Convert();
            var res = result.Value;
            var info = new PatientInfo();

            if (res.卡类型 != "9")
            {
                if (res.卡号.Length == 8)
                    return Result.Fail("请检查您的卡位置是否正确\n请按提示正确插卡\n芯片朝下 向前插入");
                info.HaveSMK = true;

                info.Name = res.姓名;
                info.CardNo = res.卡号.Trim(' ');
                info.CardType = res.卡类型;
                info.IdNo = res.身份证号;

                if (res.卡识别码.StartsWith("330100"))
                    info.PatientType = "56";//原为56,8.22日要求改为84
                else
                    info.PatientType =
                        PatientTypeParse.CY_Parse(
                            PatientTypeParse.SMK_Parse(
                                res.卡识别码.Substring(0, 6)));

                Logger.Main.Info($"读市民卡卡号:{info.CardNo} PatientType{info.PatientType}");
            }
            else
            {
                if (res.卡号.Length != 8)
                    return Result.Fail("请检查您的卡位置是否正确\n请按提示正确插卡\n芯片朝下 向前插入");

                info.HaveSMK = false;

                info.CardNo = $"801{res.卡类型}{res.卡号}".Trim(' ');
                info.CardType = res.卡类型;
                info.IdNo = res.身份证号;

                info.PatientType = PatientTypeParse.CY_Parse("健康卡");
                Logger.Main.Info($"读非接卡号:{info.CardNo}");
            }

            Auth.Info = info;
            return Result.Success();
        }

        public Result 账户查询()
        {
            var Auth = GetAuthModel();
            if (Auth.Info.HaveSMK)
                return 账户查询_SMK(Auth);
            return 账户查询_JKK(Auth);
        }

        private Result 账户查询_JKK(IAuthModel Auth)
        {
            var info = Auth.Info;
            info.NoAccount = false;
            info.NoSmartHealth = false;
            info.Remain = 0;
            info.Name = "";
            info.IdNo = "";

            try
            {
                var req = new Req可扣查询_JKK
                {
                    卡号 = info.CardNo.FillPadChar(12),
                    卡类型 = "01"
                };
                var result = DataHandler.可扣查询_JKK(req);
                if (!result.IsSuccess)
                    return result.Convert();

                var ret = result.ResultCode;
                var res = result.Value;
                if (ret == 0 && res.应答码 == "00")
                {
                    Logger.Main.Info("账户与智慧医疗均开通");
                    
                    info.Name = res.姓名;
                    info.IdNo = res.身份证;
                    info.Remain = decimal.Parse(res.返回金额);
                    return Result.Success();
                }
                if (ret == 11 && res.应答码 == "00")
                {
                    Logger.Main.Info("账户开通，智慧医疗未开通");
                    info.NoSmartHealth = true;
                    return Result.Success();
                }
                if (ret == 12 && res.应答码 == "14")
                {
                    Logger.Main.Info("账户与智慧医疗均未开通");
                    info.NoAccount = true;
                    info.NoSmartHealth = true;
                    return Result.Success();
                }

                return Result.Fail(ErrorCodeParse.RespondParse(res.应答码));
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"账户查询_JKK Exception:{ex}");
                return Result.Fail(ex.Message, ex);
            }
        }

        //市民卡可扣查询  查询是开通账户  已开通的市民卡余额
        private Result 账户查询_SMK(IAuthModel Auth)
        {
            var info = Auth.Info;
            info.NoAccount = false;
            info.NoSmartHealth = false;
            info.Remain = 0;
            try
            {
                var req = new Req可扣查询_SMK
                {
                    卡号 = info.CardNo.FillPadChar(12),
                    卡类型 = "00"
                };
                var result = DataHandler.可扣查询_SMK(req);
                if (!result.IsSuccess)
                    return result.Convert();

                var ret = result.ResultCode;
                var res = result.Value;
                if (ret == 0 && res.应答码 == "00")
                {
                    Logger.Main.Info("账户与智慧医疗均开通");
                    info.Remain = decimal.Parse( res.返回金额);
                    info.CardNo = res.卡面号;
                    return Result.Success();
                }
                if (ret == 11 && res.应答码 == "00")
                {
                    Logger.Main.Info("账户开通，智慧医疗未开通");
                    info.NoSmartHealth = true;
                    return Result.Success();
                }
                if (ret == 12 && res.应答码 == "14")
                {
                    Logger.Main.Info("账户与智慧医疗均未开通");
                    info.NoAccount = true;
                    info.NoSmartHealth = true;
                    return Result.Success();
                }
                return Result.Fail(ErrorCodeParse.RespondParse(res.应答码));
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"账户查询_SMK Exception:{ex}");
                return Result.Fail(ex.Message, ex);
            }
        }

        public Result 人员信息查询()
        {
            try
            {
                var Auth = GetAuthModel();
                var info = Auth.Info;
                var req = new ReqDll
                {
                    卡号 = info.CardNo,
                    结算类型 = "00",
                    病人类别 = info.PatientType,
                    结算方式 = "2", //1 预结算 2 结算
                    应付金额 = "0",
                };
                var result = DataHandler.病人信息查询建档(req);
                if (!result.IsSuccess)
                {
                    Logger.Main.Error("HIS查询建档失败");
                    return result.Convert();
                }
                var res = result.Value;
                Auth.人员信息 = res;
                info.Name = res.病人姓名;
                if (!info.HaveSMK)
                    info.IdNo = res.身份证号;

                Logger.Main.Debug("HIS查询建档成功");
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"人员信息查询 Exception:{ex}");
                return Result.Fail(ex.Message, ex);
            }
        }

    }

    public class PatientTypeParse
    {
        private static readonly Dictionary<string, string> SMKTypeDictionary = new Dictionary<string, string>
        {
            {"330100", "市医保"},
            {"330199", "市医保"},
            {"330102", "市医保"},
            {"330103", "市医保"},
            {"330104", "市医保"},
            {"330105", "市医保"},
            {"330106", "市医保"},
            {"330108", "市医保"},
            {"339900", "省医保"},
            {"330109", "萧山医保"},
            {"330185", "临安市医保"},
            {"330183", "富阳市医保"},
            {"330182", "建德市医保"},
            {"330127", "淳安县医保"},
            {"330122", "桐庐县医保"},
            {"330110", "余杭医保"}
        };

        private static readonly Dictionary<string, string> ST_TypeDictionary = new Dictionary<string, string>
        {
            {"健康卡", "1"}, //自费
            {"省医保", "55"}, //原先是15
            {"市医保", "84"}, //84
            {"省异地", "55"}, //省一卡通
            {"市异地", "56"}, //市一卡通
            {"萧山医保", "88"}
        };

        private static readonly Dictionary<string, string> CY_TypeDictionary = new Dictionary<string, string>
        {
            {"健康卡", "100"},
            {"省医保", "55"}, //原先是15
            {"市医保", "56"}, //84改成56，改回去了--By 叶飞
            {"省异地", "55"},
            {"市异地", "56"},
            {"萧山医保", "88"}
        };

        public static string SMK_Parse(string code)
        {
            if (SMKTypeDictionary.ContainsKey(code))
                return SMKTypeDictionary[code];
            if (code.StartsWith("3301"))
                return "市异地";
            if (code.StartsWith("33"))
                return "省异地";

            return "自费";
        }

        public static string ST_Parse(string code)
        {
            try
            {
                return ST_TypeDictionary[code];
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.ToString());
                return null;
            }
        }

        public static string CY_Parse(string code)
        {
            try
            {
                return CY_TypeDictionary[code];
            }
            catch (Exception ex)
            {
                Logger.Main.Error(ex.ToString());
                return null;
            }
        }
    }
    public static class StringHandler
    {
        public enum EncodingFlag
        {
            GB2312 = 1,
            GBK = 2
        }

        public enum PadDirection
        {
            Left = 1,
            Right = 2
        }

        public enum PadFlag
        {
            Space = 1,
            Zero = 2
        }

        private static readonly Encoding Encoding = Encoding.GetEncoding("GB2312");
        private static readonly Encoding Encoding2 = Encoding.GetEncoding("GBK");

        public static string FillPadChar(this string s, int len,
            EncodingFlag coding = EncodingFlag.GB2312,
            PadFlag flag = PadFlag.Space,
            PadDirection direction = PadDirection.Right)
        {
            var slen = coding == EncodingFlag.GB2312 ? Encoding.GetByteCount(s) : Encoding2.GetByteCount(s);

            if (slen > len)
            {
                int i;
                var count = 0;
                var array = s.ToCharArray();
                for (i = 0; i < s.Length; i++)
                {
                    var clen = Encoding.GetByteCount(array, i, 1);
                    count += clen;
                    if (count > len)
                        break;
                }
                return s.Substring(0, i);
            }

            var sb = new StringBuilder(len);
            var c = flag == PadFlag.Space ? ' ' : '0';

            if (direction == PadDirection.Left)
                sb.Append(c, len - slen);

            sb.Append(s);

            if (direction == PadDirection.Right)
                sb.Append(c, len - slen);

            return sb.ToString();
        }
    }
}
