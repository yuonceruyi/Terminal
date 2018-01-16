using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.TakeNum
{
    public interface IAppoRecordModel : IModel
    {
        /// <summary>
        /// 取号密码
        /// </summary>
        string RegNo { get; set; }
        req挂号预约记录查询 Req挂号预约记录查询 { get; set; }
        res挂号预约记录查询 Res挂号预约记录查询 { get; set; }
        挂号预约记录 所选记录 { get; set; }
    }

    public class RecordModel : ModelBase, IAppoRecordModel
    {
        public string RegNo { get; set; }

        public req挂号预约记录查询 Req挂号预约记录查询 { get; set; }
        public res挂号预约记录查询 Res挂号预约记录查询 { get; set; }
        public 挂号预约记录 所选记录 { get; set; }
    }
}