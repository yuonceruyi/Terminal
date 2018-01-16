using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Register.Models
{
    public interface IRegisterModel : IModel
    {
        int RegType { get; set; }
        bool IsDoctor { get; set; }
        List<KESHI_GUAHAO> Depts { get; set; }
        KESHI_GUAHAO SelectedDept { get; set; }
        List<PAIBAN_KESHI> DeptSchedules { get; set; }
        PAIBAN_KESHI SelectedDeptSchedule { get; set; }
        List<PAIBAN_YISHENG> DoctSchedules { get; set; }
        List<PAIBAN_YISHENG> DoctSchedulesSingleDept { get; set; }
        PAIBAN_YISHENG SelectedDoctSchedule { get; set; }
    }

    public class RegisterModel : ModelBase, IRegisterModel
    {
        public int RegType { get; set; }
        public bool IsDoctor { get; set; }
        public List<KESHI_GUAHAO> Depts { get; set; }
        public KESHI_GUAHAO SelectedDept { get; set; }
        public List<PAIBAN_KESHI> DeptSchedules { get; set; }
        public PAIBAN_KESHI SelectedDeptSchedule { get; set; }
        public List<PAIBAN_YISHENG> DoctSchedules { get; set; }
        public List<PAIBAN_YISHENG> DoctSchedulesSingleDept { get; set; }
        public PAIBAN_YISHENG SelectedDoctSchedule { get; set; }
    }
}