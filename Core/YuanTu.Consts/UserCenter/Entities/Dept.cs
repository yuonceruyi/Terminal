using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class Dept : BaseDO
    {
        /// <summary>
        ///     医院id
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     医院id  该字段不存库，只做接口上的返回值使用
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     医院id在排班系统中的编号
        /// </summary>
        public string corpCode { get; set; }

        /// <summary>
        ///     父科室代码
        /// </summary>
        public string parentDeptCode { get; set; }

        /// <summary>
        ///     父科室名称
        /// </summary>
        public string parentDeptName { get; set; }

        /// <summary>
        ///     父科室名称的全拼
        /// </summary>
        public string parentDeptPY { get; set; }

        /// <summary>
        ///     父科室名称的简拼
        /// </summary>
        public string parentDeptSimplePY { get; set; }

        /// <summary>
        ///     科室代码
        /// </summary>
        public string deptCode { get; set; }

        /// <summary>
        ///     科室名称
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        ///     科室名称的全拼
        /// </summary>
        public string deptPY { get; set; }

        /// <summary>
        ///     科室名称的简拼
        /// </summary>
        public string deptSimplePY { get; set; }

        /// <summary>
        ///     科室联系电话
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        ///     科室地址
        /// </summary>
        public string address { get; set; }

        /// <summary>
        ///     科室介绍
        /// </summary>
        public string deptIntro { get; set; }

        /// <summary>
        ///     手动设置 排序序号
        /// </summary>
        public int serialNum { get; set; } = 999;

        /// <summary>
        ///     状态 1正常  2 删除
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     如果需要用到一级科室编码,请使用baseDeptCode变量
        ///     @since v2.3.4
        ///     大科室code
        /// </summary>
        public long bigDeptCode { get; set; }

        /// <summary>
        ///     大科室name
        /// </summary>
        public string bigDeptName { get; set; }

        /// <summary>
        ///     科室所属院区的标识，因为市立 查询医院所有科室 ，返回的数据包含所有院区的，所以需要二次过滤
        /// </summary>
        public string hospitalPartQY { get; set; }

        /// <summary>
        ///     一级科室编码  用于替换先前用的 bigDeptCode 对应一级科室的deptCode
        /// </summary>

        public string baseDeptCode { get; set; }

        /// <summary>
        ///     子科室
        /// </summary>
        public List<Dept> children { get; set; }

        /// <summary>
        ///     挂号的号源类型 list
        /// </summary>
        public List<RegTypeOutParams> regConfigList { get; set; }

        /// <summary>
        ///     预约的号源类型 list
        /// </summary>
        public List<RegTypeOutParams> appoConfigList { get; set; }

        /// <summary>
        ///     性别限制 0:无限制  1:男  2:女
        /// </summary>
        public int genderLimit { get; set; }

        /// <summary>
        ///     年龄限制
        /// </summary>
        public string ageLimit { get; set; }
    }
}