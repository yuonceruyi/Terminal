using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Devices;
using YuanTu.Devices.UnionPay;

namespace YuanTu.WeiHaiZXYY.Services
{
    public class MisposUnionServiceSpecial : MisposUnionService
    {
        public override Result UnInitialize(string reason)
        {
            DisConnect(reason);
            if (!_hasEject)
            {
                var ret = UMS_EjectCard();
                if (ret != 0)
                {
                    //var dic = Dics.ErrorDictionary[nameof(UMS_EjectCard)];
                    var error = nameof(UMS_EjectCard).GetErrorMsgDetail(ret);// dic.ContainsKey(ret) ? dic[ret] : "未知错误码:" + ret;
                    return Result.Fail(error);

                }
                ret = UMS_CardClose();
                _hasEject = true;
            }
            return Result.Success();
        }
    }
}
