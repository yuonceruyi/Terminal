using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.NanYangFirstPeopleHospital.Services
{
    public class MisposUnionService: YuanTu.Devices.UnionPay.MisposUnionService
    {
        public override Result Initialize(Business businessType, string misposdllPath, BanCardMediaType bankMediaType)
        {
            var ret= base.Initialize(businessType, misposdllPath, bankMediaType);
            if (ret.IsSuccess)
            {
                _SetReq = LoadExternalFunction<IntStringDelegate>("UMS_SetReq");
            }
            return ret;
        }
    }
}
