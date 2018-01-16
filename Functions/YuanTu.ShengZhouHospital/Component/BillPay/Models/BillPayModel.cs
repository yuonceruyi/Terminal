using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.BillPay.Models
{
    public class BillPayModel:Consts.Models.BillPay.BillPayModel
    {
        public ReqHIS缴费预结算 ReqHIS缴费预结算 { get; set; }
        public ResHIS缴费预结算 ResHis缴费预结算 { get; set; }

        public ReqHIS缴费结算 ReqHis缴费结算 { get; set; }
        public ResHIS缴费结算 ResHis缴费结算 { get; set; }

    }
}
