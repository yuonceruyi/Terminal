using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.YiWuFuBao.Dtos;

namespace YuanTu.YiWuFuBao.Models
{
    public class PatientModel:YuanTu.Consts.Models.Auth.DefaultPatientModel
    {
        public ZHUYUANRYXX_OUT ZhuyuanryxxOut { get; set; }
    }
}
