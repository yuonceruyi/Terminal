using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Extension;

namespace YuanTu.ShengZhouZhongYiHospital.HisNative.Models
{
    public abstract class HisRes
    {
        public int RetCode { get; set; }
        public bool IsSuccess => RetCode == 0;
        public string Message { get; set; }
        public abstract int ArrLen { get;  }
        public string 原始报文 { get; set; }
        

        public void Deserialize(string content)
        {
            原始报文 = content;
            var arrs = content.BackNotNullOrEmpty("-1|HIS返回数据不正确").Split('|');
            if (arrs.FirstOrDefault()=="-1")
            {
                RetCode = -1;
                Message = arrs.LastOrDefault();
                return;
            }
            if (arrs.Length-1<ArrLen)
            {
                RetCode = -1;
                Message = "HIS返回的报文格式不正确";
                return;
            }
            
            RetCode = 0;
            Message = null;
            Build(arrs);
        }

        public abstract void Build(string[] arrs);
    }
}
