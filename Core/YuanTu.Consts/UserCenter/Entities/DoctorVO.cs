using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class DoctorVO
    {
        ///<summary>
        ///医生姓名
        ///</summary>
        public string name { get; set; }
        ///<summary>
        ///科室名称
        ///</summary>
        public string deptName { get; set; }
        ///<summary>
        ///医院id
        ///</summary>
        public long corpId { get; set; }
        ///<summary>
        ///医生编号
        ///</summary>
        public string doctCode { get; set; }
        ///<summary>
        ///科室编号
        ///</summary>
        public string deptCode { get; set; }
        ///<summary>
        ///医生头像
        ///</summary>
        public string doctLogo { get; set; }
        ///<summary>
        /// 就诊时间
        ///</summary>
        public string medDate { get; set; }
        ///<summary>
        ///预约状态   可预约 已满
        ///</summary>
        public string medStatusDesc { get; set; }
        ///<summary>
        ///上午可预约数量
        ///</summary>
        public int medAmNum { get; set; }
        ///<summary>
        ///下午可预约数量
        ///</summary>
        public int medPmNum { get; set; }
        ///<summary>
        ///挂号金额  单位为分 挂号费+诊疗费
        ///</summary>
        public string regAmount { get; set; }
        ///<summary>
        ///医生职称
        ///</summary>
        public string doctTech { get; set; }
        ///<summary>
        ///医生擅长
        ///</summary>
        public string doctSpec { get; set; }
        ///<summary>
        ///性别
        ///</summary>
        public string sex { get; set; }
        ///<summary>
        ///专家
        ///</summary>
        public string regTypeName { get; set; }
        ///<summary>
        ///排班id
        ///</summary>
        public string scheduleId { get; set; }
        ///<summary>
        ///专家子类型
        ///</summary>
        public int subRegType { get; set; }
        ///<summary>
        ///类型 RegTypeEnums中定义
        ///</summary>
        public int regType { get; set; }

        /// <summary>
        /// 上午下午
        /// </summary>
        public int medAmPm { get; set; }
    }


   
}

