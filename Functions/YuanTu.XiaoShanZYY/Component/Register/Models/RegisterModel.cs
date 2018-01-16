using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Register.Models
{
    public interface IRegisterModel : IModel
    {
        string RegDate { get; set; }
        string RegMode { get; set; }
        string RegType { get; set; }
        string AmPm { get; set; }
        string DeptId { get; set; }
        string DeptName { get; set; }
        string DoctorId { get; set; }
        string DoctorName { get; set; }

        List<PAIBANMX> PAIBANLB { get; set; }
        List<PAIBANLBItem> PAIBANLBItems { get; set; }
        PAIBANMX 所选排班 { get; set; }
        PAIBANLBItem 所选排班Item { get; set; }

        List<YISHENGXX> YISHENGMX { get; set; }
        YISHENGXX 所选医生 { get; set; }

        List<HAOYUANXX> HAOYUANMX { get; set; }
        List<HAOYUANMXItem> HAOYUANMXItems { get; set; }
        HAOYUANXX 所选号源 { get; set; }
        HAOYUANMXItem 所选号源Item { get; set; }

        Res挂号取号 Res挂号预结算 { get; set; }
        Res挂号取号 Res挂号结算 { get; set; }
        Res预约挂号处理 Res预约结算 { get; set; }

        bool IsRegister();
        bool IsAppoint();
    }
    public class RegisterModel : ModelBase, IRegisterModel
    {
        public string RegDate { get; set; }
        public string RegMode { get; set; }
        public string RegType { get; set; }
        public string AmPm { get; set; }
        public string DeptId { get; set; }
        public string DeptName { get; set; }
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }

        public List<PAIBANMX> PAIBANLB { get; set; }
        public List<PAIBANLBItem> PAIBANLBItems { get; set; }
        public PAIBANMX 所选排班 { get; set; }
        public PAIBANLBItem 所选排班Item { get; set; }

        public List<YISHENGXX> YISHENGMX { get; set; }
        public YISHENGXX 所选医生 { get; set; }

        public List<HAOYUANXX> HAOYUANMX { get; set; }
        public List<HAOYUANMXItem> HAOYUANMXItems { get; set; }
        public HAOYUANXX 所选号源 { get; set; }
        public HAOYUANMXItem 所选号源Item { get; set; }

        public Res挂号取号 Res挂号预结算 { get; set; }
        public Res挂号取号 Res挂号结算 { get; set; }
        public Res预约挂号处理 Res预约结算 { get; set; }
        public bool IsRegister()
        {
            return RegMode == "1";
        }

        public bool IsAppoint()
        {
            return RegMode == "2";
        }
    }

    public class PAIBANLBItem
    {
        public string DeptId { get; set; }
        public string DeptName { get; set; }
        public List<PAIBANMX> PAIBANLB { get; set; }
    }

    public class HAOYUANMXItem
    {
        public string RegDate { get; set; }
        public List<HAOYUANXX> HAOYUANMX { get; set; }
    }
}
