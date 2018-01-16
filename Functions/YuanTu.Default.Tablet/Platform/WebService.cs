using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace YuanTu.Default.Tablet.Platform
{
    public interface IWebService
    {
        Task<Result<ResCreate>> Create(string cardNo, string cardType, decimal amount);
        Task<Result<ResPay>> Pay(ResCreate create, string auth);
        Task<Result<ResRefund>> Refund(string orderId, decimal amount);
        Task<Result<ResQuery>> Query(ResCreate create);
        Task<Result<ResMap>> Map(string cardNo, string cardType);
    }
    class WebService : IWebService
    {

        #region Helpers


        public string Url { get; set; }

        static readonly HttpClient Client = new HttpClient();

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
        };

        async Task<Result<TRes>> Query<TReq, TRes>(TReq req)
            where TReq : Req
            where TRes : Res
        {
            var json = JsonConvert.SerializeObject(req, JsonSerializerSettings);

            var response = await Client.PostAsync(Url,
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return Result<TRes>.Fail(response.ReasonPhrase);

            var content = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<TRes>(content, JsonSerializerSettings);

            if (res.Code != "10000")
                return Result<TRes>.Fail($"[{res.Code}]{res.Msg}\n[{res.SubCode}]{res.SubMsg}");

            return Result<TRes>.Success(res);
        }

        #endregion
        public async Task<Result<ResCreate>> Create(string cardNo, string cardType, decimal amount)
        {
            return await Query<ReqCreate, ResCreate>(new ReqCreate()
            {
                BizContent = new CreateParams()
                {
                    OrderNo = DateTime.Now.ToBinary().ToString(),

                    CardNo = cardNo,
                    CardType = cardType,
                    OperType = "10",

                    Subject = "消费",
                    TotalAmount = (long)(amount * 100),

                    StoreId = "NJ_001",
                    TerminalId = "NJ_T_001",
                    OperatorId = "yx_00",

                    GoodsDetail = null,
                }
            });
        }

        public async Task<Result<ResPay>> Pay(ResCreate create, string auth)
        {
            return await Query<ReqPay, ResPay>(new ReqPay()
            {
                BizContent = new PayParams()
                {
                    OrderNo = create.OrderNo,
                    OutTradeNo = create.OutTradeNo,
                    AuthCode = auth
                }
            });
        }

        public async Task<Result<ResRefund>> Refund(string orderId, decimal amount)
        {
            return await Query<ReqRefund, ResRefund>(new ReqRefund()
            {
                BizContent = new RefundParams()
                {
                    OrderNo = orderId,

                    OutTradeNo = "",

                    OutRefundNo = DateTime.Now.ToBinary().ToString(),
                    Fee = (long)(amount * 100),
                    Reason = "",

                    StoreId = "",
                    TerminalId = "",
                    OperatorId = "",
                }
            });
        }

        public async Task<Result<ResQuery>> Query(ResCreate create)
        {
            return await Query<ReqQuery, ResQuery>(new ReqQuery()
            {
                BizContent = new QueryParams()
                {
                    OrderNo = create.OrderNo,
                    OutTradeNo = create.OutTradeNo,
                }
            });
        }

        public async Task<Result<ResMap>> Map(string cardNo, string cardType)
        {
            return await Query<ReqMap, ResMap>(new ReqMap()
            {
                BizContent = new MapParams()
                {
                    CardNo = cardNo,
                    CardType = cardType,
                }
            });
        }
    }
    class MockWebService : IWebService
    {
        public async Task<Result<ResCreate>> Create(string cardNo, string cardType, decimal amount)
        {
            await Task.Delay(300);
            if (amount > 3000)
                return Result<ResCreate>.Fail("余额不足");
            return Result<ResCreate>.Success(new ResCreate()
            {
                Code = "10000",
                SubCode = "1000",
                OrderNo = "123",
                OutTradeNo = "123456",
            });
        }

        public async Task<Result<ResPay>> Pay(ResCreate create, string auth)
        {
            await Task.Delay(300);
            if (!auth.StartsWith("pay"))
                return Result<ResPay>.Fail("验证码错误");
            return Result<ResPay>.Success(new ResPay()
            {
                Code = "10000",
                SubCode = "1002",
                OrderNo = "123",
                OutTradeNo = "123456"
            });
        }

        public async Task<Result<ResRefund>> Refund(string orderId, decimal amount)
        {
            await Task.Delay(300);
            if (orderId.StartsWith("1"))
                return Result<ResRefund>.Fail("订单不存在");
            return Result<ResRefund>.Success(new ResRefund()
            {
                Code = "10000",
                SubCode = "1006",
                Fee = 2000,
                OrderNo = "123",
                OutTradeNo = "123456",
                OutRefundNo = "123456789",
            });
        }

        public async Task<Result<ResQuery>> Query(ResCreate create)
        {
            await Task.Delay(300);
            return Result<ResQuery>.Success(new ResQuery()
            {
                OrderNo = "123",
                OutTradeNo = "123456",

                Subject = "",
                TotalAmount = 0,

                BuyerId = "",
                TradeStatus = ""
            });
        }

        public async Task<Result<ResMap>> Map(string cardNo, string cardType)
        {
            await Task.Delay(300);
            return Result<ResMap>.Success(new ResMap()
            {
                BuyerId = "",
                BuyerPhone = "",
            });
        }
    }

    class Req
    {
        public string Appid { get; set; } = "202";
        public string Method { get; set; }
        public string Format { get; set; } = "JSON";
        public string Charset { get; set; } = "UTF-8";
        public string SignType { get; set; } = "RSA";
        public string Sign { get; set; }
        public string Timestamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string Version { get; set; } = "1.0";
    }

    class Req<T> : Req
    {
        public T BizContent { get; set; }
    }

    public class Res
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public string SubCode { get; set; }
        public string SubMsg { get; set; }
        public string Sign { get; set; }
    }

    #region Create

    class CreateParams
    {
        public string OrderNo { get; set; }
        public string CardNo { get; set; }
        public string CardType { get; set; }
        public string Subject { get; set; }
        public string OperType { get; set; }
        public long TotalAmount { get; set; }
        public string TerminalId { get; set; }
        public string OperatorId { get; set; }
        public string StoreId { get; set; }
        public GoodsDetail[] GoodsDetail { get; set; }
    }

    class GoodsDetail
    {
        public string GoodsName { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string GoodsCategory { get; set; }
    }

    class ReqCreate : Req<CreateParams>
    {
        public ReqCreate()
        {
            Method = "yuantupay.trade.order.create";
        }
    }

    public class ResCreate : Res
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
    }

    #endregion

    #region Pay

    class PayParams
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
        public string AuthCode { get; set; }
    }

    class ReqPay : Req<PayParams>
    {
        public ReqPay()
        {
            Method = "yuantupay.trade.order.pay";
        }
    }

    public class ResPay : Res
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
        public long TotalAmount { get; set; }
    }

    #endregion

    #region Query


    class QueryParams
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
    }

    class ReqQuery : Req<QueryParams>
    {
        public ReqQuery()
        {
            Method = "yuantupay.trade.order.query";
        }
    }

    public class ResQuery : Res
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
        public string Subject { get; set; }
        public string BuyerId { get; set; }
        public long TotalAmount { get; set; }
        public string TradeStatus { get; set; }
    }

    #endregion

    #region Map

    class MapParams
    {
        public string CardNo { get; set; }
        public string CardType { get; set; }
    }

    class ReqMap : Req<MapParams>
    {
        public ReqMap()
        {
            Method = "yuantupay.trade.card.query";
        }
    }

    public class ResMap : Res
    {
        public string BuyerId { get; set; }
        public string BuyerPhone { get; set; }

    }

    #endregion

    #region Refund

    class RefundParams
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
        public string OutRefundNo { get; set; }
        public long Fee { get; set; }
        public string Reason { get; set; }
        public string TerminalId { get; set; }
        public string OperatorId { get; set; }
        public string StoreId { get; set; }
    }

    class ReqRefund : Req<RefundParams>
    {
        public ReqRefund()
        {
            Method = "yuantupay.trade.order.refund";
        }
    }

    public class ResRefund : Res
    {
        public string OrderNo { get; set; }
        public string OutTradeNo { get; set; }
        public string OutRefundNo { get; set; }
        public long Fee { get; set; }
    }

    #endregion
}
