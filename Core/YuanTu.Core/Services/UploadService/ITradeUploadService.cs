using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Systems.Ini;

namespace YuanTu.Core.Services.UploadService
{
    public interface ITradeUploadService:IService
    {
       
        Result Upload(TradeInfo info);
        Task<Result> UploadAsync(TradeInfo info);
    }

    public class TradeUploadService : ITradeUploadService
    {
        public string ServiceName => "交易数据上传";

        protected bool ThreadWorking = false;
        private static readonly IniFile TradeUploadIni = new IniFile("TradeUpload.ini");
        private static readonly IniInteger TradeMaxId = new IniInteger(TradeUploadIni, "Main", "TradeMaxId");
        protected AutoResetEvent AutoReset = new AutoResetEvent(false);
        public Result Upload(TradeInfo info)
        {
            try
            {
                DBManager.Insert(info);
            }
            catch (Exception e)
            {
                Logger.Main.Error($"[{ServiceName}]交易信息保存失败，原因:{e.Message},原始数据:{info.ToJsonString()}");
            }
            try
            {
                return UploadToServer(info);
            }
            catch (Exception e)
            {
                Logger.Main.Error($"[{ServiceName}]交易信息上送失败，原因:{e.Message},原始数据:{info.ToJsonString()}");
            }

            return Result.Fail("上送失败！");
        }

        public Task<Result> UploadAsync(TradeInfo info)
        {

            return Task.Run(() =>
            {
                try
                {
                    return Upload(info);
                }
                catch
                {
                    return Result.Fail("发生异常");
                }
            });
        }


        protected virtual Result UploadToServer(TradeInfo info)
        {

            StartUploadThread();
            AutoReset.Set();
            return Result.Success();
        }

        protected virtual void StartUploadThread()
        {
            if (ThreadWorking||!FrameworkConst.EnableUploadTradeInfo)
            {
                return;
            }
           
            ThreadWorking = true;
            (new Thread(_ =>
            {
                while (true)
                {
                    AutoReset.WaitOne();
                    try
                    {
                        var lastId = TradeMaxId.Value;
                        var vals = DBManager.Query<TradeInfo>($"Select * from TradeInfo Where HaveUpload=0 and Id>={lastId}").ToArray();
                        if (vals.Any())
                        {
                            UploadToTarget(vals);
                        }
                        else
                        {
                            //被触发，但却搜索不到，则认为是本地Id缓存失败，重置它！
                            TradeMaxId.Value = 0;
                        }
                    }
                    catch (Exception e)
                    {
                       Logger.Main.Error($"[交易上送]上送交易时发生异常，{e.Message}");
                    }
                    
                }
            }){
                Name = nameof(StartUploadThread),
                Priority = ThreadPriority.Lowest,
                IsBackground = true,
            }).Start();

        }

        protected virtual void UploadToTarget(TradeInfo[]lst )
        {
            foreach (var info in lst)
            {
                var req = FillTradeInfo(info);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    DBManager.Excute($"Update TradeInfo set HaveUpload=1 where Id={info.Id}");
                    TradeMaxId.Value = info.Id;
                }
            }
        }

        protected virtual req交易记录同步到his系统 FillTradeInfo( TradeInfo info)
        {
            var tradeTypeMap = new Dictionary<TradeType, string>()
            {
                [TradeType.交易成功] = "2",
                [TradeType.交易撤销成功] = "3",
                [TradeType.充值成功] = "1",
                [TradeType.充值撤销成功] = "4",
            };
            var req = new req交易记录同步到his系统
            {
                platformId = FrameworkConst.OperatorId,
                hisPatientId = info.PatientId,
                cardNo = info.CardNo,
                idNo = info.IdNo,
                patientName = info.PatientName,
                tradeType = tradeTypeMap[info.TradeType],
                tradeMode = info.PayMethod.GetEnumDescription(),
                cash = info.Amount.ToString("0"),
                operId = FrameworkConst.OperatorId,
                bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                inHos = "1",
                remarks = info.TradeName,
            };
            if (info.PayMethod == PayMethod.银联)
            {
                var posinfo = info.TradeDetail.ToJsonObject<TransResDto>();
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if ((info.PayMethod == PayMethod.支付宝 || info.PayMethod == PayMethod.微信支付) && info.TradeType == TradeType.交易成功)
            {
                var thirdpayinfo = info.TradeDetail.ToJsonObject<订单状态>();
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
            return req;
        }

    }
}
