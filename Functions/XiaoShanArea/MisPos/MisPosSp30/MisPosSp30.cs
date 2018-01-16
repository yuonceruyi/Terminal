using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.YuHangArea.MisPos.MisPosSp30
{
    public class MisPosSp30
    {
        private const string Sp30DllPath = "umsDevTool_sp30.dll";
        [DllImport(Sp30DllPath, EntryPoint = "SP30_DevTool_YLDS", CharSet = CharSet.Ansi,
           CallingConvention = CallingConvention.StdCall)]
        public static extern int SP30_DevTool_YLDS(
           int iFuncidx,
           string psaMchtCode,
           string psaOperNo,
           string psaAmt,
           string psaBatchNo,
           string psaSeqNo,
           string psaExFlow,
           StringBuilder psaRetInfo
           );

        private static readonly Dictionary<int, string> TransDic = new Dictionary<int, string>
        {
            {1,"消费" },
            {2,"冲正"},
             {3,"结算" },
            {4,"查询流水"},
             {5,"查询" },
            {6,"签到"}
        };

        private static int _ret;

        public static Result<Response> Do(Request req)
        {
            try
            {
                var sb = new StringBuilder(4096);
                Logger.POS.Info($"[MisPosSp30] [{TransDic[req.Funcidx]}] Req:{req.ToJsonString()}");
                _ret = SP30_DevTool_YLDS(req.Funcidx, req.PsaMchtCode, req.PsaOperNo, req.PsaAmt, req.PsaBatchNo, req.PsaSeqNo, req.PsaExFlow, sb);
                Logger.POS.Info($"[MisPosSp30] [{TransDic[req.Funcidx]}] OrgRes:{sb}");
                var res = Decipher(sb.ToString(), _ret);
                Logger.POS.Info($"[MisPosSp30] [{TransDic[req.Funcidx]}] Res:{res.ToJsonString()}");
                return Result<Response>.Success(res);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[MisPosSp30] [{TransDic[req.Funcidx]}] {ex.Message} {ex.StackTrace}");
                return Result<Response>.Fail(ex.Message);
            }
          
        }

        public static Result<Response> DoSale(decimal amount)
        {
           
                var req = new Request
                {
                    Funcidx = 1,
                    PsaMchtCode = "",
                    PsaOperNo = "",
                    PsaAmt = (Convert.ToDouble(amount) / 100).ToString(CultureInfo.InvariantCulture),
                    PsaBatchNo = "",
                    PsaSeqNo = "",
                    PsaExFlow = ""
                };
                return Do(req);
         
           
        }

        public static Result<Response> DoRefund(decimal amount, string seq)
        {
            var req = new Request
            {
                Funcidx = 2,
                PsaMchtCode = "",
                PsaOperNo = "",
                PsaAmt = (Convert.ToDouble(amount) / 100).ToString(CultureInfo.InvariantCulture),
                PsaBatchNo = "",
                PsaSeqNo = seq,
                PsaExFlow = ""
            };
            return Do(req);
        }

        public static Result<Response> DoSettle()
        {
            var req = new Request
            {
                Funcidx = 3,
                PsaMchtCode = "",
                PsaOperNo = "",
                PsaAmt = "",
                PsaBatchNo = "",
                PsaSeqNo = "",
                PsaExFlow = ""
            };
            return Do(req);
        }

        public static Result<Response> DoQuerySeq()
        {
            var req = new Request
            {
                Funcidx = 4,
                PsaMchtCode = "",
                PsaOperNo = "",
                PsaAmt = "",
                PsaBatchNo = "",
                PsaSeqNo = "",
                PsaExFlow = ""
            };
            return Do(req);
        }

        public static Result<Response> DoQuery()
        {
            var req = new Request
            {
                Funcidx = 5,
                PsaMchtCode = "",
                PsaOperNo = "",
                PsaAmt = "",
                PsaBatchNo = "",
                PsaSeqNo = "",
                PsaExFlow = ""
            };
            return Do(req);
        }

        public static Result<Response> DoLogon()
        {
            var req = new Request
            {
                Funcidx = 6,
                PsaMchtCode = "",
                PsaOperNo = "",
                PsaAmt = "",
                PsaBatchNo = "",
                PsaSeqNo = "",
                PsaExFlow = ""
            };
            return Do(req);
        }

        public static readonly Dictionary<string, string> ErrorDictionary = new Dictionary<string, string>
            {
                {"01", "终端未签到"},
                {"02", "终端不可用"},
                {"03", "终端已签到"},
                {"04", "销售终端未签到"},
                {"05", "非法卡"},
                {"06", "过期卡"},
                {"07", "止付卡"},
                {"08", "密码错请重输"},
                {"09", "未知交易"},
                {"10", "非止付卡"},
                {"11", "止付卡"},
                {"12", "交易未能处理"},
                {"13", "无未知交易"},
                {"14", "柜员轧账错"},
                {"16", "交易未授权"},
                {"17", "原始交易已冲正"},
                {"18", "权限不匹配"},
                {"19", "非联名卡"},
                {"21", "异地卡"},
                {"22", "异地交易"},
                {"23", "非取款卡"},
                {"24", "通讯校验错"},
                {"25", "无效的操作员号"},
                {"26", "操作员需签到"},
                {"27", "信息文件出错"},
                {"28", "已签到"},
                {"29", "操作员密码错"},
                {"30", "卡号校验错"},
                {"32", "同一张卡不能转帐"},
                {"33", "已取消"},
                {"34", "非法交易"},
                {"35", "金额不匹配"},
                {"36", "非法交易"},
                {"37", "对话超时"},
                {"38", "不可退货"},
                {"39", "报文格式错"},
                {"41", "超消费比例"},
                {"42", "已兑奖"},
                {"43", "已取消兑奖"},
                {"45", "流水号错"},
                {"46", "授权已确认"},
                {"47", "消费已付，交易拒绝"},
                {"49", " 帐不平"},
                {"51", "商户资金不足"},
                {"52", "帐户文件不可用"},
                {"53", "原始交易未找到"},
                {"54", "该授权已消费"},
                {"55", "超限额"},
                {"56", "请索权"},
                {"57", "付出卡不存在"},
                {"58", "不允许本地取现"},
                {"59", "止付卡"},
                {"60", "记录不存在"},
                {"61", "系统故障"},
                {"62", "只做本地取现"},
                {"63", "授权号错"},
                {"64", "非法卡"},
                {"65", "非法卡"},
                {"66", "过期卡"},
                {"67", "积分不足"},
                {"68", "预授权当日不可取消"},
                {"69", "帐户不存在"},
                {"70", "已超兑奖期"},
                {"71", " 非法卡"},
                {"72", "检查帐户错"},
                {"73", "交易未能处理"},
                {"74", "应用未找到"},
                {"75", "卡片状态不对"},
                {"76", "上门收款错误"},
                {"77", "外卡错误信息"},
                {"78", "卡表版本错误"},
                {"81", "请与发卡行联系"},
                {"82", "个人标识号错"},
                {"83", "交易不能处理"},
                {"84", "主机通讯故障"},
                {"85", "数据传输错"},
                {"87", "总行文件打开错"},
                {"88", "超限额，请电话索权"},
                {"89", "通讯校验错"},
                {"90", "无相应记录"},
                {"91", "权限不匹配"},
                {"92", "读数据库文件失败"},
                {"93", "商场记录错"},
                {"94", "更新数据库文件失败"},
                {"95", "不能在本商户使用"},
                {"96", "异地网未开通"},
                {"97", "非法交易"},
                {"98", "应用子系统未激活"},
                {"99", "交易过程未发现"}
            };

        public static Response Decipher(string text, int ret)
        {
            if (text.IndexOf('|') < 0 || ret != 0)
            {
                return new Response
                {
                    Text = text,
                    成功 = false,
                    错误信息 = ret + " " + Parse(text)
                };
            }
            var list = text.Split('|');

            var output = new Response
            {
                Text = text,
                成功 = true,
                Seq = list[2],
                Card = list[7],
                Cseq = list[9],

                交易金额 = list[0],
                批次号 = list[1],
                流水号 = list[2],
                商户代码 = list[3],
                终端号 = list[4],
                收单银行 = list[5],
                发卡银行 = list[6],
                卡号 = list[7],
                交易类型 = list[8],
                系统参考号 = list[9],
                交易时间 = list[10],
                外卡组织 = list[11],
                Tc = list[12],
                Aid = list[13],
                Tvr = list[14],
                Tsi = list[15],
                Atc = list[16],
                应用标签 = list[17],
                首选名称 = list[18],
                外卡组织代码 = list[19],
                授权号 = list[20],
                卡有效期 = list[21],
                备注 = list[22],
                输入方式 = list[23]
            };
            return output;
        }

        public static string Parse(string error)
        {
            try
            {
                return ErrorDictionary[error.Substring(0, 2)];
            }
            catch (Exception)
            {
                return error;
            }
        }
    }
}