using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.ShengZhouHospital.HisNative.Models;

namespace YuanTu.ShengZhouHospital.Component.Auth.Models
{
    public class PatientInfoModel:YuanTu.Consts.Models.Auth.DefaultPatientModel
    {
        public Req门诊读卡 Req门诊读卡 { get; set; }
        public Res门诊读卡 Res门诊读卡 { get; set; }
    }
}
