using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
namespace YuanTu.Consts.Models.InfoQuery
{
    public  interface IInPrePayRecordModel : IModel
    {
        req住院预缴金充值记录查询 Req住院预缴金充值记录查询 { get; set; }
        res住院预缴金充值记录查询 Res住院预缴金充值记录查询 { get; set; }
    }

    public class InPrePayRecordModel : ModelBase, IInPrePayRecordModel
    {
        public req住院预缴金充值记录查询 Req住院预缴金充值记录查询 { get; set; }
        public res住院预缴金充值记录查询 Res住院预缴金充值记录查询 { get; set; }
    }
}
