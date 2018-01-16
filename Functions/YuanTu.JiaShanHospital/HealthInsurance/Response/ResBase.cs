using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.HealthInsurance.Response
{
    public class ResBase
    {
        public string 交易状态 { get; set; }
        public string 错误信息 { get; set; }
        public string 写医保卡结果 { get; set; }
        public string 扣银行卡结果 { get; set; }
        public string 更新后ic卡数据 { get; set; }
        public new virtual string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"交易状态:{交易状态}\n");
            sb.Append($"错误信息:{错误信息}\n");
            sb.Append($"写医保卡结果:{写医保卡结果}\n");
            sb.Append($"更新后ic卡数据:{更新后ic卡数据}\n");
            return sb.ToString();
        }
    }
}
