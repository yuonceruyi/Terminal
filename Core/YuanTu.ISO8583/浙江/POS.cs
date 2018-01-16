using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.ISO8583.Util;

namespace YuanTu.ISO8583.浙江
{
    public class POS: YuanTu.ISO8583.POS
    {
        public override Result<byte[]> DoLogon()
        {
            var now = DateTimeCore.Now;
            var encoder = ContainerWrapper.CreateEncoder();
            encoder.MessageType = "0800";
            encoder.Values = new Dictionary<int, string>
            {
                [3] = "940000",
                [25] = "14",
                //[7] = now.ToString("MMddHHmmss"),
                [41] = Config.TerminalId,
                [42] = Config.MerchantId,
                [60] = "A00100",
            };
            var send = encoder.Encode();
            var decoder = ContainerWrapper.CreateDecoder();
            decoder.Decode(send);
            var res = Connection.Handle(send);
            if (!res.IsSuccess)
                return Result<byte[]>.Fail(res.Message);

            var message = decoder.Decode(res.Value);
            if (message[39].Text != "00")
                return Result<byte[]>.Fail(message[39].Text + StatusCode.Get(message[39].Text));
            var bts = message[61].Text.Hex2Bytes();
            return Result<byte[]>.Success(bts);
        }


        private string BuildSensitiveData(ref string cardNo, ref string expir, ref string cvd, ref string _35, ref string _36)
        {
#warning 重要方法未实现
            var inner35 = _35;
            var inner36 = _36;
            if (_35.Length > 32)
            {
                inner35 = _35.Substring(0, 32);
                _35 = "".PadRight(32, '0') + _35.Substring(32);
            }
            if (_36.Length > 32)
            {
                inner36 = _36.Substring(0, 32);
                _36 = "".PadRight(32, '0') + _36.Substring(32);
            }
            inner35 = inner35.Replace('=', 'D');
            inner36 = inner36.Replace('=', 'D');
            var arr = new[] { cardNo, expir, cvd, inner35, inner36 };

            cardNo = "".PadRight(cardNo?.Length ?? 0, '0');
            expir = "".PadRight(expir?.Length ?? 0, '0');
            cvd = "".PadRight(cvd?.Length ?? 0, '0');

            var innerFunc = (Func<string, string>)(p =>
            {
                return string.IsNullOrEmpty(p) ? "" : p + "F";
            });
            var oristr = string.Join("", arr.Select(p => innerFunc(p)));
            var bitmap = new byte[2];
            for (int i = 0; i < arr.Length; i++)
            {
                bitmap[i / 8] |= (byte)(string.IsNullOrEmpty(arr[i]) ? 0 : (1 << (7 - i % 8)));
            }


            oristr = oristr.PadRight((oristr.Length + 1) / 2 * 2, 'F');
            //var c = new Cryptography
            //{
            //    CipherMode = CipherMode.ECB,
            //    PaddingMode = PaddingMode.Zeros,
            //    IV = new byte[8],
            //    Key = MACKey
            //};
            var hexBts = oristr.Hex2Bytes();
            hexBts = hexBts.Concat(new byte[(hexBts.Length + 7) / 8 * 8 - hexBts.Length]).ToArray();
            var lst = new List<byte>();

            for (int i = 0; i < hexBts.Length; i += 8)
            {
              //  lst.AddRange(MacKeyEncrptyFunc(hexBts.Skip(i).Take(8).ToArray()));
            }
            return "A00200" + bitmap.Bytes2Hex() + lst.ToArray().Bytes2Hex();

        }
    }
}
