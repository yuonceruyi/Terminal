using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class AddArchiveRequest:RequestBase
    {
        public override int BussinessType { get; set; } = 8;

        /// <summary>
        /// 建档类型 0200：医保建档 0201：医保预约取号建档
        /// </summary>
        public string ArchiveType { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 医保卡内容
        /// </summary>
        public string HealthCareCardContent { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdNumber { get; set; }
        /// <summary>
        /// 家庭住址
        /// </summary>
        public string HomeAddress { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public string Birthday { get; set; }

    }
}
