using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Models.Base
{
    public abstract class InsuranceResponseBase
    {
        public bool IsSuccess => 交易状态 == "0";
        public string 交易状态 { get; set; }
        public string 错误信息 { get; set; }
        public string 写医保卡结果 { get; set; }
        public string 扣银行卡结果 { get; set; }
        public string 写卡后IC卡数据 { get; set; }

        public string 报文入参 { get; set; }
        public string 报文出参 { get; set; }

        public static T BuildResponse<T>(string originStr) where T : InsuranceResponseBase, new()
        {
            var inner = originStr.Trim('$');
            var respArr = inner.Split('~');
            var obj = new T
            {
                交易状态 = respArr[0],
                错误信息 = respArr[1],
                报文出参 = originStr
            };
            if (obj.交易状态 == "0")
            {
                obj.写医保卡结果 = respArr[2];
                obj.扣银行卡结果 = respArr[3];
                obj.写卡后IC卡数据 = respArr[4];
                obj.DataFormat(respArr);

            }
            return obj;
        }

        public abstract void DataFormat(string[] arr);


    }
}
