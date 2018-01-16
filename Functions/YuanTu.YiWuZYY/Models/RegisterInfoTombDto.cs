using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;

namespace YuanTu.YiWuZYY.Models
{
    public class RegisterInfoTombDto
    {
        public string CardNo { get; set; }
        public CardType CardType { get; set; }
        public 病人信息 病人信息 { get; set; }

        public string MedDate { get; set; }
        public string ScheduleId { get; set; }
        public string MedAmPm { get; set; }

        public string appoNo { get; set; }
        public string orderNo { get; set; }
        public string visitNo { get; set; }
        public string regFlowId { get; set; }
        public string extend { get; set; }
      //  public res预约挂号 挂号信息 { get; set; }
    }
}
