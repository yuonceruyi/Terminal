using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Consts.Dtos.UnionPos
{
    public class TransResDto
    {
        /// <summary>
        /// 应答码
        /// </summary>
        public string RespCode { get; set; }
        /// <summary>
        /// 应答码说明信息（汉字）
        /// </summary>
        public string RespInfo { get; set; }
        /// <summary>
        /// 交易卡号
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// 终端流水号（凭证号）
        /// </summary>
        public string Trace { get; set; }
        /// <summary>
        /// 批次号
        /// </summary>
        public string Batch { get; set; }
        /// <summary>
        /// 交易日期yyyyMMdd
        /// </summary>
        public string TransDate { get; set; }
        /// <summary>
        /// 交易时间HHmmss
        /// </summary>
        public string TransTime { get; set; }
        /// <summary>
        /// 系统参考号（中心流水号）
        /// </summary>
        public string Ref { get; set; }
        /// <summary>
        /// 授权号
        /// </summary>
        public string Auth { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string MId { get; set; }
        /// <summary>
        /// 终端号
        /// </summary>
        public string TId { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 校验字符
        /// </summary>
        public string Lrc { get; set; }
        /// <summary>
        /// 银联原始交易信息
        /// </summary>
        public string Receipt { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{nameof(RespCode)}:{RespCode}\n");
            sb.Append($"{nameof(RespInfo)}:{RespInfo}\n");
            sb.Append($"{nameof(CardNo)}:{CardNo}\n");
            sb.Append($"{nameof(Amount)}:{Amount}\n");
            sb.Append($"{nameof(Trace)}:{Trace}\n");
            sb.Append($"{nameof(Batch)}:{Batch}\n");
            sb.Append($"{nameof(TransDate)}:{TransDate}\n");
            sb.Append($"{nameof(TransTime)}:{TransTime}\n");
            sb.Append($"{nameof(Ref)}:{Ref}\n");
            sb.Append($"{nameof(Auth)}:{Auth}\n");
            sb.Append($"{nameof(MId)}:{MId}\n");
            sb.Append($"{nameof(TId)}:{TId}\n");
            sb.Append($"{nameof(Memo)}:{Memo}\n");
            sb.Append($"{nameof(Lrc)}:{Lrc}\n");
            sb.Append($"{nameof(Receipt)}:{Receipt}\n");
            return sb.ToString();
        }

        private static string GetString(byte[] data, ref int start, int length)
        {
            var s = Encoding.Default.GetString(data, start, length);
            start += length;
            return s;
        }

        public static TransResDto Parse(byte[] data)
        {
            var p = 0;
            return new TransResDto
            {
                RespCode = GetString(data, ref p, 2),
                RespInfo = GetString(data, ref p, 40),
                CardNo = GetString(data, ref p, 20),
                Amount = GetString(data, ref p, 12),
                Trace = GetString(data, ref p, 6),
                Batch = GetString(data, ref p, 6),
                TransDate = GetString(data, ref p, 4),
                TransTime = GetString(data, ref p, 6),
                Ref = GetString(data, ref p, 12),
                Auth = GetString(data, ref p, 6),
                MId = GetString(data, ref p, 15),
                TId = GetString(data, ref p, 8),
                Memo = GetString(data, ref p, 1024),
                Lrc = GetString(data, ref p, 3)
            };
        }
    }
}
