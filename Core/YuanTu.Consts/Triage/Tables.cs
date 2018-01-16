using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using YuanTu.Consts.Gateway.Base;

namespace YuanTu.Consts.Triage
{
    #pragma warning disable 612
    public partial class 预检科室信息 : GatewayDataBase
    {
        /// <summary>
        /// 科室编码
        /// </summary>
        public string departName { get; set; }
        /// <summary>
        /// 科室名称
        /// </summary>
        public string  departCode { get; set; }
        /// <summary>
        /// 科室类别,1:标识急诊类,2:标识非急诊类
        /// </summary>
        public string departType { get; set; }
        /// <summary>
        /// 是否有效，1：有效，2：无效
        /// </summary>
        public string departStatus { get; set; }
    }

    public partial class 预检挂号类别信息 : GatewayDataBase
    {
        /// <summary>
        /// 挂号类别编码
        /// </summary>
        public string registrationTypeName { get; set; }
        /// <summary>
        /// 挂号类别名称
        /// </summary>
        public string  registrationTypeCode { get; set; }
        /// <summary>
        /// 是否有效，1：有效，2：无效
        /// </summary>
        public string registrationTypeStatus { get; set; }
    }

    public partial class 预检记录信息 : GatewayDataBase
    {
        /// <summary>
        /// 科室编码
        /// </summary>
        public string departCode { get; set; }
        /// <summary>
        /// 科室名称
        /// </summary>
        public string  departName { get; set; }
        /// <summary>
        /// 预检状态，1：已预检，2：已取消
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public string operteDate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createDate { get; set; }
        /// <summary>
        /// 门诊号/病历号,患者医院唯一ID
        /// </summary>
        public string patientId { get; set; }
    }

#pragma warning restore 612
}