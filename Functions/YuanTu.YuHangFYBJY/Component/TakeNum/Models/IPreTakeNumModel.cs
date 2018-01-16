using YuanTu.Consts.FrameworkBase;
using YuanTu.YuHangFYBJY.NativeService.Dto;

namespace YuanTu.YuHangFYBJY.Component.TakeNum.Models
{
    public interface IPreTakeNumModel : IModel
    {
        /// <summary>
        /// 取号预结算结果
        /// </summary>
        PerGetTicketCheckout ResPreTakeNum { get; set; }
        /// <summary>
        /// 取号结算结果
        /// </summary>
        GetTicketCheckout ResTakeNum { get; set; }
        /// <summary>
        /// 取号查询日期
        /// </summary>
        string TakeNumDate { get; set; }
    }

    public class PreTakeNumModel : ModelBase, IPreTakeNumModel
    {
      
        public PerGetTicketCheckout ResPreTakeNum { get; set; }
        public GetTicketCheckout ResTakeNum { get; set; }
        public string TakeNumDate { get; set; }
    }
}