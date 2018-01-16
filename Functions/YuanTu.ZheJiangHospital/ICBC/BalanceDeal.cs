using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.DB;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Systems.Ini;

namespace YuanTu.ZheJiangHospital.ICBC
{
    internal class BalanceDeal
    {
        public static void Check(DateTime time)
        {
            if (time.Hour < 17) return;
            if (Busy) return;
            if (LastTryDateTime != null && time - LastTryDateTime < TimeSpan.FromMinutes(5))
                return;
            if (CheckedDateTime?.Date != time.Date)
                自动对账();
        }

        public static void 自动对账()
        {
            if (Busy)
                return;
            Busy = true;
            try
            {
                LastTryDateTime = DateTimeCore.Now;
                Logger.Main.Info("开始自动对账");
                var list = 获取未对账数据();
                if (list == null || list.Count == 0)
                {
                    Logger.Main.Info("自动对账完成 没有未对账数据");
                    CheckedDateTime = DateTimeCore.Now;
                    return;
                }
                var n = 0;
                Result ret;
                while (n < 3)
                {
                    n++;
                    ret = 轧账停机申请();
                    if (!ret.IsSuccess)
                        Logger.Main.Warn($"轧账停机申请 第{n}次失败");
                    else
                        break;
                }

                n = 0;
                while (n < 3)
                {
                    n++;
                    ret = 对账("自动对账");
                    if (!ret.IsSuccess)
                        Logger.Main.Warn($"自动对账 第{n}次失败");
                    else
                        break;
                }
                CheckedDateTime = DateTimeCore.Now;

                n = 0;
                while (n < 3)
                {
                    n++;
                    ret = 轧账开机申请();
                    if (!ret.IsSuccess)
                        Logger.Main.Warn($"轧账开机申请 第{n}次失败");
                    else
                        break;
                }
                Logger.Main.Info("自动对账完成");
            }
            finally
            {
                Busy = false;
            }
        }

        #region 对账

        public static Result 对账(string account)
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Check");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            var tradeSerial = TradeSerial;
            var fileInfo = new FileInfo(Path.Combine(
                dirPath,
                $"{FrameworkConst.OperatorId}_{DateTimeCore.Now:yyyyMMddHHmmss}.txt"));

            Req轧账对账申请 req;
            var text = 生成对账信息(out req);
            if (req == null)
                return Result.Success();

            req.TradeSerial = tradeSerial;
            req.FileName = fileInfo.Name;
            req.BankAccount = account;

            using (var sr = new StreamWriter(fileInfo.FullName, true, Encoding.GetEncoding("GB2312")))
            {
                sr.Write(text);
            }
            if (!FTP.UploadFile(fileInfo, @"\",
                PConnection.ICBC.Address.ToString(),
                $"term{PConnection.HisCode}", $"{PConnection.HisCode}term"))
            {
                return Result.Fail($"对账文件上传失败 文件名:{fileInfo.FullName}");
            }
            var result = PConnection.Handle<Res轧账对账申请>(req);
            if (!result.IsSuccess)
                return Result.Fail($"轧账对账申请失败:{result.Message}");

            InsertCheckInfo(req, true);
            return Result.Success();
        }

        public static DateTime GetLastCheckTime()
        {
            const string SQL = "SELECT CheckTime FROM CheckInfoICBC ORDER BY Id DESC LIMIT 1;";
            var list = DBManager.Query<CheckInfoICBC>(SQL);
            if (list == null || list.Count == 0)
                return DateTimeCore.Now.AddYears(-1);
            return list[0].CheckTime;
        }

        private static List<RechargeInfoICBC> 获取未对账数据()
        {
            var SQL =
                "SELECT * FROM RechargeInfoICBC WHERE TransTime > @0 AND DeviceInfo = @1 ORDER BY Id ASC;";
            return DBManager.Query<RechargeInfoICBC>(SQL, GetLastCheckTime(), FrameworkConst.OperatorId);
        }

        public static string 生成对账信息(out Req轧账对账申请 req)
        {
            var list = 获取未对账数据();
            if (list == null || list.Count == 0)
            {
                req = null;
                return null;
            }
            req = new Req轧账对账申请
            {
                CashCount = list.Count(one => one.TradeMode == 1).ToString(),
                CashTotal = list.Where(one => one.TradeMode == 1).Sum(one => one.Cash).ToString(),
                Cash100 = list.Sum(one => one.Count100).ToString(),
                Cash50 = list.Sum(one => one.Count50).ToString(),
                Cash20 = list.Sum(one => one.Count20).ToString(),
                Cash10 = list.Sum(one => one.Count10).ToString(),
                TransCount = list.Count(one => one.TradeMode == 2).ToString(),
                TransTotal = list.Where(one => one.TradeMode == 2).Sum(one => one.Cash).ToString(),
                RefundCount = "0",
                RefundTotal = "0",
                OperId = FrameworkConst.OperatorId,
                DeviceInfo = FrameworkConst.OperatorId
            };
            var sb = new StringBuilder();
            sb.AppendFormat("{0},{1},{2},{3}\n", req.CashCount, req.CashTotal, req.TransCount, req.TransTotal);
            foreach (var info in list)
            {
                sb.AppendFormat("{0},{1},{2:yyyy-MM-dd HH:mm:ss},{3},{4},{5},{6},{7},{8}\n",
                    info.AccountNo, info.BankWorkDT, info.TransTime,
                    info.Cash, info.TradeSerial, info.Count100, info.Count50, info.Count20, info.Count10);
            }
            return sb.ToString();
        }

        #endregion 对账

        #region DB

        private static void InsertCheckInfo(Req轧账对账申请 req, bool succeeded)
        {
            DBManager.Insert(new CheckInfoICBC
            {
                CheckTime = DateTimeCore.Now,
                TransCode = req.TransCode,
                HisCode = req.HisCode,
                BankAccount = req.BankAccount,
                CashCount = Convert.ToInt32(req.CashCount),
                CashTotal = Convert.ToInt32(req.CashTotal),
                Cash100 = Convert.ToInt32(req.Cash100),
                Cash50 = Convert.ToInt32(req.Cash50),
                Cash20 = Convert.ToInt32(req.Cash20),
                Cash10 = Convert.ToInt32(req.Cash10),
                TransCount = Convert.ToInt32(req.TransCount),
                TransTotal = Convert.ToInt32(req.TransTotal),
                RefundCount = Convert.ToInt32(req.RefundCount),
                RefundTotal = Convert.ToInt32(req.RefundTotal),
                OperId = req.OperId,
                DeviceInfo = req.DeviceInfo,
                TradeSerial = req.TradeSerial,
                FileName = req.FileName,
                Succeeded = succeeded
            });
        }

        public static void InsertPOSInfo(Mispos.Output output, string type)
        {
            var info = new POSInfoICBC
            {
                Type = type,
                Result = output.返回码,
                Message = output.错误信息,
                Date = output.交易日期,
                Time = output.交易时间,
                Amount = output.交易金额,
                Terms = output.分期期数,
                CardNo = output.卡号,
                ValidDate = output.卡片有效期,
                CardType = output.卡片类型,
                Issuer = output.发卡行名称,
                Memo = output.备注信息,
                BatchNo = output.批次号,
                Auth = output.授权号,
                CenterSeq = output.检索参考号,
                ClearDate = output.清算日期,
                TransSeq = output.终端流水号,
                TerminalID = output.终端编号
            };
            DBManager.Insert(info);
        }

        internal static void InsertRechargeInfo(bool success, IcbcRequest req, IcbcResponse res, int[] count)
        {
            if (count == null)
                count = new int[7];
            var info = new RechargeInfoICBC
            {
                TransTime = DateTimeCore.Now,
                //TransCode = req.TransCode,
                //HisCode = req.HisCode,
                Chanel = Convert.ToInt32(req.Chanel),
                AccountNo = req.AccountNo,
                AccountId = req.AccountId,
                TradeMode = Convert.ToInt32(req.TradeMode),
                //MisposSerNo = req.MisposSerNo,
                //MisposDate = req.MisposDate,
                //MisposTime = req.MisposTime,
                //MisposTermNo = req.MisposTermNo,
                //MisposIndexNo = req.MisposIndexNo,
                //MisposInfo = req.MisposInfo,
                Cash = Convert.ToInt32(req.Cash),
                //BankCardNo = req.BankCardNo,
                OperId = req.OperId,
                DeviceInfo = req.DeviceInfo,
                TradeSerial = req.TradeSerial,
                Rsv1 = req.Rsv1,
                Rsv2 = req.Rsv2,
                Count1 = count[0],
                Count2 = count[1],
                Count5 = count[2],
                Count10 = count[3],
                Count20 = count[4],
                Count50 = count[5],
                Count100 = count[6],
                Success = success,
                BankWorkDT = success ? res.BankWorkDt : ""
            };
            DBManager.Insert(info);
        }

        public static void InsertRechargeInfo(bool success, Req充值 req, Res充值 res, int[] count = null)
        {
            if (count == null)
                count = new int[7];
            var info = new RechargeInfoICBC
            {
                TransTime = DateTimeCore.Now,
                TransCode = req.TransCode,
                HisCode = req.HisCode,
                Chanel = Convert.ToInt32(req.Chanel),
                AccountNo = req.AccountNo,
                AccountId = req.AccountId,
                TradeMode = Convert.ToInt32(req.TradeMode),
                MisposSerNo = req.MisposSerNo,
                MisposDate = req.MisposDate,
                MisposTime = req.MisposTime,
                MisposTermNo = req.MisposTermNo,
                MisposIndexNo = req.MisposIndexNo,
                MisposInfo = req.MisposInfo,
                Cash = Convert.ToInt32(req.Cash),
                BankCardNo = req.BankCardNo,
                OperId = req.OperId,
                DeviceInfo = req.DeviceInfo,
                TradeSerial = req.TradeSerial,
                Rsv1 = req.Rsv1,
                Rsv2 = req.Rsv2,
                Count1 = count[0],
                Count2 = count[1],
                Count5 = count[2],
                Count10 = count[3],
                Count20 = count[4],
                Count50 = count[5],
                Count100 = count[6],
                Success = success,
                BankWorkDT = success ? res.BankWorkDT : ""
            };
            DBManager.Insert(info);
        }

        public class ThirdPayInfo
        {
            public string hisPatientId { get; set; }
            public string cardNo { get; set; }
            public string cardType { get; set; }
            public string idNo { get; set; }
            public string patientName { get; set; }
            public string tradeMode { get; set; }
            public bool recharge { get; set; }
            public decimal amount { get; set; }
            public string transNo { get; set; }
            public string remarks { get; set; }
            public string bankCardNo { get; set; }
            public string bankTransNo { get; set; }
            public string bankDate { get; set; }
            public string bankTime { get; set; }
            public string bankSettlementTime { get; set; }
        }

        public static void IntertThirdPayInfo(
            ThirdPayInfo info)
        {
            if (info.amount == 0)
                return;
            Task.Run(() =>
            {
                try
                {
                    var req = new req交易记录同步到his系统
                    {
                        hospCode = "3301011249",
                        hisPatientId = info.hisPatientId,
                        cardNo = info.cardNo,
                        cardType = info.cardType,
                        idNo = info.idNo,
                        patientName = info.patientName,
                        cash = info.amount.ToString("0"),
                        tradeTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        guarderId = "",
                        tradeMode = info.tradeMode,
                        tradeType = info.recharge ? "1" : "2",
                        transNo = info.transNo,
                        bankCardNo = info.bankCardNo,
                        bankTransNo = info.bankTransNo,
                        bankDate = info.bankDate,
                        bankTime = info.bankTime,
                        bankSettlementTime = info.bankSettlementTime,
                        inHos = "1",
                        remarks = info.remarks,
                    };
                    var res = DataHandlerEx.交易记录同步到his系统(req);
                }
                catch (Exception e)
                {
                    Logger.Main.Warn(e.ToString());
                }
            });
        }

        #endregion DB

        #region DataHandler

        public static Result 轧账停机申请()
        {
            var req = new Req轧账停机申请
            {
                OperId = FrameworkConst.OperatorId,
                DeviceInfo = FrameworkConst.OperatorId,
                TradeSerial = TradeSerial
            };
            var result = PConnection.Handle<Res轧账停机申请>(req);
            if (!result.IsSuccess)
                return Result.Fail($"轧账停机申请失败:{result.Message}");
            return Result.Success();
        }

        public static Result 轧账开机申请()
        {
            var req = new Req轧账开机申请
            {
                OperId = FrameworkConst.OperatorId,
                DeviceInfo = FrameworkConst.OperatorId,
                TradeSerial = TradeSerial
            };
            var result = PConnection.Handle<Res轧账开机申请>(req);
            if (!result.IsSuccess)
                return Result.Fail($"轧账开机申请失败:{result.Message}");
            return Result.Success();
        }

        public static Result 查询充值退款交易明细(Req查询充值退款交易明细 req)
        {
            var result = PConnection.Handle<Res查询充值退款交易明细>(req);
            if (!result.IsSuccess)
                return Result.Fail($"查询充值退款交易明细失败:{result.Message}");
            return Result.Success();
        }

        #endregion DataHandler

        #region Ini

        public static DateTime? LastTryDateTime { get; set; }
        public static DateTime? CheckedDateTime { get; set; }

        public static bool Busy { get; set; }

        public static string TradeSerial => GetBankNo(DateTimeCore.Now);

        private static readonly string CommonIniFile = Path.Combine(FrameworkConst.RootDirectory, "浙江医院.ini");
        private static readonly IniFile Config = new IniFile(CommonIniFile);

        private static readonly IniString _bankDate = new IniString(Config, "Main", "BankDate");

        private static readonly IniInteger _bankNum = new IniInteger(Config, "Main", "BankNum");

        private static string BankDate
        {
            get { return _bankDate.Value; }
            set { _bankDate.Value = value; }
        }

        private static int BankNum
        {
            get { return _bankNum.Value; }
            set { _bankNum.Value = value; }
        }

        public static string GetBankNo(DateTime now)
        {
            var date = now.ToString("yyyyMMdd");
            var dateString = BankDate;
            if (string.IsNullOrEmpty(dateString) || dateString != date)
            {
                BankDate = date;
                BankNum = 1;
                return now.ToString("yyyyMMddHHmmss") + "000000";
            }
            var num = BankNum++;
            return now.ToString("yyyyMMddHHmmss") + num.ToString("D6");
        }

        #endregion Ini
    }

    public class RechargeInfoICBC : Table
    {
        public DateTime TransTime { get; set; }
        public string TransCode { get; set; }
        public string HisCode { get; set; }
        public int Chanel { get; set; }
        public string AccountNo { get; set; }
        public string AccountId { get; set; }
        public int TradeMode { get; set; }
        public string MisposSerNo { get; set; }
        public string MisposDate { get; set; }
        public string MisposTime { get; set; }
        public string MisposTermNo { get; set; }
        public string MisposIndexNo { get; set; }
        public string MisposInfo { get; set; }
        public int Cash { get; set; }
        public string BankCardNo { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string Rsv1 { get; set; }
        public string Rsv2 { get; set; }
        public int Count1 { get; set; }
        public int Count2 { get; set; }
        public int Count5 { get; set; }
        public int Count10 { get; set; }
        public int Count20 { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public bool Success { get; set; }
        public string BankWorkDT { get; set; }
    }

    public class CheckInfoICBC : Table
    {
        public DateTime CheckTime { get; set; }
        public string TransCode { get; set; }
        public string HisCode { get; set; }
        public string BankAccount { get; set; }
        public int CashCount { get; set; }
        public int CashTotal { get; set; }
        public int Cash100 { get; set; }
        public int Cash50 { get; set; }
        public int Cash20 { get; set; }
        public int Cash10 { get; set; }
        public int TransCount { get; set; }
        public int TransTotal { get; set; }
        public int RefundCount { get; set; }
        public int RefundTotal { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string FileName { get; set; }
        public bool Succeeded { get; set; }
    }

    public class POSInfoICBC : Table
    {
        public string Type { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string CardNo { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string TransSeq { get; set; }
        public string BatchNo { get; set; }
        public string ClearDate { get; set; }
        public string CenterSeq { get; set; }
        public string ValidDate { get; set; }
        public string Amount { get; set; }
        public string TerminalID { get; set; }
        public string Auth { get; set; }
        public string Terms { get; set; }
        public string Issuer { get; set; }
        public string CardType { get; set; }
        public string Memo { get; set; }
    }
}