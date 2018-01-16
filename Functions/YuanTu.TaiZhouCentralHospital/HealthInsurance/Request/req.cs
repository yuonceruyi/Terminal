 using System;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Request
{

    public class Req获取参保人信息:ReqBase
    {
        public string 读卡方式 { get; set; }
        public string 终端机编号 { get; set; }
      

    }

    public class Req交易确认:ReqBase
    {
        public string 交易类型 { get; set; }
        public string 医保交易流水号 { get; set; }
        public string HIS事务结果 { get; set; }
        public string 附加信息 { get; set; }
      

    }

    public class Req交易结果查询:ReqBase
    {
        public string 交易类型 { get; set; }
        public string 医保交易流水号 { get; set; }
      

    }

    public class Req医保退费:ReqBase
    {
        public string 要作废的结算流水号 { get; set; }
        public string 经办人 { get; set; }
        public string 终端机编号 { get; set; }
      

    }


}