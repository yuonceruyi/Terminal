using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.Register.Models
{
    public class RegisterModel:YuanTu.Consts.Models.Register.RegisterModel
    {
        public Req挂号取号预结算 Req挂号取号预结算 { get; set; }
        public Res挂号取号预结算 Res挂号取号预结算 { get; set; }
        public Req挂号取号结算 Req挂号取号结算 { get; set; }
        public Res挂号取号结算 Res挂号取号结算 { get; set; }
    }
}
