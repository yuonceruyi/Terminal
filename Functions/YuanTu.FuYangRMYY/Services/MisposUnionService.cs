using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.UnionPay;

namespace YuanTu.FuYangRMYY.Services
{
    public class MisposUnionService:YuanTu.Devices.UnionPay.MisposUnionService
    {
        public override Result SetReq(TransType transType, decimal totalMoneySencods)
        {
            IsBusy = true;
            try
            {
                var req = new TransReq()
                {
                    Amount = totalMoneySencods,
                    TransType = transType,
                    Memo = DateTimeCore.Now.Ticks.ToString()
                };
                var reqStr = Serialize(req);
                Logger.POS.Info($"[{ServiceName}]开始设置TransReq:{reqStr}");
                var ret = UMS_SetReq(reqStr);
                Logger.POS.Info($"[{ServiceName}]设置TransReq结果:{ret}");
                if (ret != 0)
                {
                    var error = nameof(UMS_SetReq).GetErrorMsgDetail(ret);
                    return Result.Fail(error);
                }
                else
                {
                    return Result.Success();
                }

            }
            catch (Exception ex)
            {
                Logger.POS.Info($"[MisPos]设置TransReq异常:{ex.Message} {ex.StackTrace}");
                return Result.Fail(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public override Result<TransResDto> DoSale(decimal totalMoneySencods)
        {
            if (!IsConnected)
            {
                return Result<TransResDto>.Fail("取消操作");
            }
            Thread.Sleep(500);
            var ret = UMS_GetPin();
            if (ret != 0)
            {
                var error = nameof(UMS_GetPin).GetErrorMsgDetail(ret);
                return Result<TransResDto>.Fail(error);
            }
            var req = new TransReq()
            {
                Amount = totalMoneySencods,
                TransType = TransType.消费,
                Memo = DateTimeCore.Now.Ticks.ToString() 
            };
            var reqStr = Serialize(req);
            Logger.POS.Info($"[Mispos消费]入参:{reqStr} 入参:{req.ToJsonString()}");
            var buffer = new byte[2048];
            ret = UMS_TransCard(reqStr, buffer);
            var res = Encoding.Default.GetString(buffer);
            Logger.POS.Info($"Mispos消费 金额:{totalMoneySencods}\r\n入参:{req.ToJsonString()}\r\n出参:{res.ToJsonString()}");
            var pret = Desclize(buffer);
            if (pret.RespCode == "00")
            {
                pret.Receipt = GetPrint();
                return Result<TransResDto>.Success(pret);
            }
            else
            {
                return Result<TransResDto>.Fail("由于银联服务异常，扣费失败。异常原因:" + pret.RespInfo);
            }
        }

        public override Result<TransResDto> Refund(string reason)
        {
            SetReq(TransType.冲正, 0);
            return base.Refund(reason);
        }

        private string Serialize(TransReq req)
        {
            var sb = new StringBuilder();
            sb.Append(PadLeft(req.CounterId, 8));
            sb.Append(PadLeft(req.OperId, 8));
            sb.Append(((int)req.TransType).ToString("D2"));
            sb.Append(((int)req.Amount).ToString("D12"));
            sb.Append(PadLeft(req.OldTrace, 6));
            sb.Append(PadLeft(req.OldDate, 8));
            sb.Append(PadLeft(req.OldRef, 12));
            sb.Append(PadLeft(req.OldAuth, 6));
            sb.Append(PadLeft(req.OldBatch, 6));
            sb.Append((req.Memo??"").PadRight(300,'.').PadRight(1024,' '));
            sb.Append(PadLeft(req.Lrc, 3));
            return sb.ToString();
        }

        private TransResDto Desclize(byte[] data)
        {
            var p = 0;
            return new TransResDto
            {
                RespCode = GetString(data, ref p, 2),
                RespInfo = GetString(data, ref p, 40),
                CardNo = GetString(data, ref p, 20),
                Amount = GetString(data, ref p, 12),
                Trace = GetString(data, ref p, 6),
                Batch = GetString(data, ref p, 6),
                TransDate = GetString(data, ref p, 4),
                TransTime = GetString(data, ref p, 6),
                Ref = GetString(data, ref p, 12),
                Auth = GetString(data, ref p, 6),
                MId = GetString(data, ref p, 15),
                TId = GetString(data, ref p, 8),
                Memo = GetString(data, ref p, 1024),
                Lrc = GetString(data, ref p, 3)
            };
        }

        protected string PadLeft(string s, int length)
        {
            var str = "";
            if (string.IsNullOrEmpty(s))
                str= string.Empty.PadLeft(length, ' ');
            else
            {
                str = s.PadLeft(length, ' ');
            }
            return str;
        }
        
    }
}
