using System;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Devices.MKeyBoard;
using YuanTu.PanYu.House.PanYuGateway;
using DataHandler = YuanTu.PanYu.House.PanYuGateway.DataHandler;
using txnChnl = YuanTu.PanYu.House.PanYuGateway.txnChnl;
using txnCode = YuanTu.PanYu.House.PanYuGateway.txnCode;

namespace YuanTu.PanYu.House.PanYuService
{
    public interface IMscService:IService
    {
        string PhoneNum { get; set; }
        string CardUid { get; set; }
        string CardSeq { get; set; }
        string CardNo { get; set; }
        Result 民生卡终端签到();
        Result 民生卡客户信息更新();
        Result 民生卡卡片信息查询();
        res民生卡卡片信息查询 Res民生卡卡片信息查询 { get;  set; }
    }
    public class MscService : ServiceBase, IMscService
    {
        protected IMKeyboard ZtMKeyboard;

        public MscService(IMKeyboard[] imKeyboards)
        {
            ZtMKeyboard = imKeyboards?.FirstOrDefault(p => p.DeviceId == "ZTKeyboard");
        }

        #region 民生卡终端签到

        public string TxnDate => DateTimeCore.Now.ToString("yyyyMMdd");
        public  string TxnTime => DateTimeCore.Now.ToString("HHmmss");
        public  string 商户代号 =>InnerConfig.商户代号;
        public string 柜员号 => InnerConfig.柜员号;
        public string 终端号 => InnerConfig.终端号;
       
        public string DateNow => DateTimeCore.Now.ToString("yyyyMMdd");
        public txnChnl TxtChnl => txnChnl.自助终端;

        public res民生卡终端签到 Res民生卡终端签到 { get; protected set; }

        public Result 民生卡终端签到()
        {
            var req = new req民生卡终端签到
            {
                TxnCode = ((int)txnCode.民生卡终端签到).ToString(),
                TxnChnl = ((int)TxtChnl).ToString(),
                MerchantId = 商户代号,
                TellerNo = 柜员号,
                TerminalNO = 终端号,
                TxnDate = TxnDate,
                TxnTime = TxnTime,
                ChnlSsn = GetFlowId()
            };
            var res = DataHandler.民生卡终端签到(req);
            Res民生卡终端签到 = res;
            if (!(res?.success ?? false))
                return Result.Fail("民生卡终端签到失败\n" + res?.msg);
            InnerConfig.DoLogonDate = DateTimeCore.Now.ToString("yyyyMMdd");
            return Result.Success();
        }

        #endregion 民生卡终端签到

        #region 民生卡客户信息更新

        public res民生卡客户信息更新 Res民生卡客户信息更新 { get; protected set; }

        public Result 民生卡客户信息更新()
        {
            var req = new req民生卡客户信息更新
            {
                TxnCode = ((int)txnCode.民生卡客户信息更新).ToString(),
                TxnChnl = ((int)TxtChnl).ToString(),
                MerchantId = 商户代号,
                TellerNo = 柜员号,
                TerminalNO = 终端号,
                TxnDate = TxnDate,
                TxnTime = TxnTime,
                ChnlSsn = GetChnlSsn(),
                CardNo = CardNo,
                CardUid = CardUid,
                PhoneNum = PhoneNum,
                CardSeq = CardSeq
            };
            var result = ClaMac(req.TxnCode, req.TxnChnl, req.MerchantId, req.TellerNo, req.TerminalNO, req.TxnDate, req.TxnTime, req.ChnlSsn);
            if (!result.IsSuccess)
                return Result.Fail("民生卡客户信息更新失败\n" + result.Message);
            req.Mac = result.Value;
            var res = DataHandler.民生卡客户信息更新(req);
            Res民生卡客户信息更新 = res;
            if (res?.success ?? false)
                return Result.Success();
            return Result.Fail("民生卡客户信息更新失败\n" + res?.msg);
        }

        #endregion 民生卡客户信息更新

        #region 民生卡卡片信息查询
        public string CardNo { get; set; }
        public string PhoneNum { get; set; }
        public string CardUid { get; set; }
        public string CardSeq { get; set; }
        public res民生卡卡片信息查询 Res民生卡卡片信息查询 { get;  set; }

        public Result 民生卡卡片信息查询()
        {
            var req = new req民生卡卡片信息查询
            {
                TxnCode = ((int)txnCode.民生卡卡片信息查询).ToString(),
                TxnChnl = ((int)TxtChnl).ToString(),
                MerchantId = 商户代号,
                TellerNo = 柜员号,
                TerminalNO = 终端号,
                TxnDate = TxnDate,
                TxnTime = TxnTime,
                ChnlSsn = GetChnlSsn(),
                CardNo = CardNo,
                CardSeq = CardSeq
            }; 
            var result = ClaMac(req.TxnCode, req.TxnChnl, req.MerchantId, req.TellerNo, req.TerminalNO, req.TxnDate, req.TxnTime, req.ChnlSsn);
            if (!result.IsSuccess)
                return Result.Fail("民生卡卡片信息查询失败\n" + result.Message);
            req.Mac = result.Value;
            var res = DataHandler.民生卡卡片信息查询(req);
            Res民生卡卡片信息查询 = res;
            if (res?.success ?? false)
                return Result.Success();
            return Result.Fail("民生卡卡片信息查询失败\n" + res?.msg);
        }

        #endregion 民生卡卡片信息查询

        public Result<string> ClaMac(string TxnCode, string TxnChnl, string MerchantId, string TellerNo, string TerminalNO, string TxnDate, string TxnTime, string ChnlSsn)
        {
            try
            {
                if (FrameworkConst.FakeServer)
                    return Result<string>.Success(null);
                TxnCode = TxnCode.Length < 12 ? TxnCode.PadRight(12, ' ') : TxnCode;
                MerchantId = MerchantId.Length < 11 ? MerchantId.PadRight(11, ' ') : MerchantId;
                TellerNo = TellerNo.Length < 11 ? TellerNo.PadRight(11, ' ') : TellerNo;
                TerminalNO = TerminalNO.Length < 15 ? TerminalNO.PadRight(15, ' ') : TerminalNO;
                var mac = TxnCode + TxnChnl + MerchantId + TellerNo + TerminalNO + TxnDate + TxnTime + ChnlSsn;
                Logger.Main.Info("MacOrginal:" + mac);

                var result = ZtMKeyboard.Connect();
                if (!result.IsSuccess)
                    return Result<string>.Fail($"{result.Message}");
                if (!InnerConfig.DoLogon || InnerConfig.DoLogonDate != DateNow)
                {
                    result = 民生卡终端签到();
                    if (!result.IsSuccess)
                        return Result<string>.Fail($"{result.Message}");
                    result = ZtMKeyboard.LoadWorkKey(Res民生卡终端签到.data.PinKey, Res民生卡终端签到.data.PinChk, Res民生卡终端签到.data.MacKey, Res民生卡终端签到.data.MacChk);
                    if (!result.IsSuccess)
                        return Result<string>.Fail($"{result.Message}");
                }

                InnerConfig.DoLogon = true;
                mac =mac.String2Hex(mac.Length).Byte2String();
                var res = ZtMKeyboard.CalcMac(mac,KMode.PEA_TDES,MacMode.x9算法);
                if (!res.IsSuccess)
                    return Result<string>.Fail($"{result.Message}");
                return Result<string>.Success(res.Value);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"民生卡计算mac失败：{ex.Message} {ex.StackTrace}");
                return Result<string>.Fail($"民生卡计算mac出现异常");
            }
        }


        public string ServiceName { get; }
    }
}