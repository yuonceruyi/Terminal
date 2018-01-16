using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.FuYangRMYY.HisNative.Models;

namespace YuanTu.FuYangRMYY.Component.Auth.Models
{
    public class SiCardInfo
    {
        public string Name => 社保读卡?.姓名;
        public Sex Sex => 社保读卡?.性别.SafeToSex() ?? Sex.未知;
        public string IdNo => 社保读卡?.身份证号;

        public DateTime Birth => DateTime.ParseExact(社保读卡?.生日, "yyyyMMddHHmmss", null);
        //0|10456256^KD5579819^^徐林艳^2^1^19841016000000^341225198410168521^阜阳市人民医院^^^11^^1783.59^^^^^^|102549^^^1^^^20130118153423^341201^^22^1^341299^^^2017^0^344.63^0^0^0^0^0^47^10^
        public 社保读卡 社保读卡 { get; set; }
    }
}
