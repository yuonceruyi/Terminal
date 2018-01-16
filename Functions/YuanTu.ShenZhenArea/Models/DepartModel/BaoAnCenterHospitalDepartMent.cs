using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShenZhenArea.Models.DepartModel
{
    public class BaoAnCenterHospitalDepartment
    {
        /// <summary>
        /// 科室名称
        /// </summary>
        public string DepartName { set; get; }
        /// <summary>
        /// 子科室
        /// </summary>
        public List<BaoAnCenterHospitalDepartment> ChildDepartments { get; set; }
        /// <summary>
        /// 对应HIS科室的编码
        /// </summary>
        public string DepartCode { get; set; }
    }
}
