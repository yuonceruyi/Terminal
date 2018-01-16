using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class RegisterPay:PerRegisterPay
    {
        /// <summary>
        /// 挂号序号
        /// </summary>
        public string RegisterNo { get; set; }
        /// <summary>
      /// 就诊地点
      /// </summary>
        public string VisitingLocation { get; set; }
    }
}
