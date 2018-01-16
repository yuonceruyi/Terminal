namespace YuanTu.Consts.UserCenter
{
    public interface IDataHandler
    {
        TRes Query<TRes, TReq>(TReq req,string url)
            where TRes : UserCenterResponse, new()
            where TReq : UserCenterRequest;

        
    }
}
