using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;

namespace YuanTu.HuNanHangTianHospital.Common
{
    public class RegTypeConvert
    {
        public static string GetRegType(RegType type)
        {
            switch (type)
            {
                case RegType.急诊挂号:
                    return "00021";
                case RegType.简易门诊:
                    return "00022";
                case RegType.主治医师:
                    return "00016";
                case RegType.副主任医师:
                    return "00017";
                case RegType.主任医师:
                    return "00018";
                case RegType.免费挂号:
                    return "00000";
                case RegType.主治医生:
                    return "00009";
                case RegType.副主任医生:
                    return "00006";
                case RegType.主任医师_外院:
                    return "00024";
                default:
                    return ((int)type).ToString();
            }
        }
    }
}
