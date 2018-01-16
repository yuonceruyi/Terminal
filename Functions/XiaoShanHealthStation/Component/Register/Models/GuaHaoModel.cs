using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanArea.CYHIS.DLL;
using YuanTu.XiaoShanArea.CYHIS.WebService;
using PAIBANMX = YuanTu.XiaoShanArea.CYHIS.WebService.PAIBANMX;
using YISHENGXX = YuanTu.XiaoShanArea.CYHIS.WebService.YISHENGXX;
using HAOYUANXX= YuanTu.XiaoShanArea.CYHIS.WebService.HAOYUANXX;

namespace YuanTu.XiaoShanHealthStation.Component.Register.Models
{
    public interface IGuaHaoModel:IModel
    {
        AmPm AmPm { get; set; }
        DateTime RegDate { get; set; }
        List<PAIBANMX> 排班明细 { get; set; }
        PAIBANMX 所选排班 { get; set; }
        List<YISHENGXX> 医生列表 { get; set; }
        YISHENGXX 所选医生 { get; set; }
        挂号取号_OUT 挂号结果 { get; set; }
        List<HAOYUANXX> 号源信息 { get; set; }
        HAOYUANXX 所选号源 { get; set; }
        string AppointPhone { get; set; }
        CLINICORDERD_OUT 预约结果 { get; set; }
    }
    public class GuaHaoModel:ModelBase, IGuaHaoModel
    {
        public AmPm AmPm { get; set; }
        public DateTime RegDate { get; set; }
        public List<PAIBANMX> 排班明细 { get; set; }
        public PAIBANMX 所选排班 { get; set; }
        public List<YISHENGXX> 医生列表 { get; set; }
        public YISHENGXX 所选医生 { get; set; }
        public 挂号取号_OUT 挂号结果 { get; set; }
        public List<HAOYUANXX> 号源信息 { get; set; }
        public HAOYUANXX 所选号源 { get; set; }
        public string AppointPhone { get; set; }
        public CLINICORDERD_OUT 预约结果 { get; set; }
    }

    public enum AmPm
    {
        Am=1,
        Pm=2
    }
}
