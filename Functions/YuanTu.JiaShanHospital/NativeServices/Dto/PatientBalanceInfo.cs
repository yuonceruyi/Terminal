﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class PatientBalanceInfo
    {
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// 市民卡余额
        /// </summary>
        public string ClitizenBalance { get; set; } = "0.00";

        /// <summary>
        /// 院内账户余额
        /// </summary>
        public string HospitalBalance { get; set; } = "0.00";

        /// <summary>
        /// 医保本年账户余额
        /// </summary>
        public string HealthCareCurrentYearBalance { get; set; } = "0.00";

        /// <summary>
        /// 医保历年账户余额
        /// </summary>
        public string HealthCareOldYearsBalance { get; set; } = "0.00";

    }
}