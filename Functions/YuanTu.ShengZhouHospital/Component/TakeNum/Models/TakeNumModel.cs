using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.TakeNum.Models
{
    public class TakeNumModel:YuanTu.Consts.Models.TakeNum.TakeNumModel
    {
        public Req挂号取号预结算 Req挂号取号预结算 { get; set; }
        public Res挂号取号预结算 Res挂号取号预结算 { get; set; }
        public Req挂号取号结算 Req挂号取号结算 { get; set; }
        public Res挂号取号结算 Res挂号取号结算 { get; set; }
    }
}
