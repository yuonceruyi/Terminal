namespace YuanTu.Consts.Gateway.Base
{
    public interface IDataHandler
    {
        TRes Query<TRes, TReq>(TReq req) 
            where TRes : GatewayResponse, new() 
            where TReq : GatewayRequest;
    }
}
