using System.Linq;
using System.Security.Cryptography;
using YuanTu.Consts.FrameworkBase;
using YuanTu.YuHangArea.ISO8583.External;

namespace YuanTu.YuHangArea.ISO8583
{
    public class KeyCard
    {
        private readonly ICardDevice _cardDevice;

        public KeyCard(ICardDevice cardDevice)
        {
            _cardDevice = cardDevice;
        }

        public Result<byte[]> GetSN()
        {
            var input = "80F6000008".Hex2Bytes();
            byte[] output;
            if (!_cardDevice.CPUChipIO(false, input, out output))
                return Result<byte[]>.Fail("通信错误");
            var hex = output.Bytes2Hex();
            var res = hex.Substring(hex.Length - 4);
            if (res.StartsWith("61"))
                return Result<byte[]>.Success(output.Take(output.Length - 2).ToArray());
            switch (res)
            {
                case "9000":
                    return Result<byte[]>.Success(output.Take(output.Length - 2).ToArray());

                case "6700":
                    return Result<byte[]>.Fail("错误的长度");

                case "6A81":
                    return Result<byte[]>.Fail("不支持此功能（无MF或者卡片已锁定）");

                case "6A82":
                    return Result<byte[]>.Fail("未找到文件");

                case "6A86":
                    return Result<byte[]>.Fail("参数P1 P2不正确");

                default:
                    return Result<byte[]>.Fail(res);
            }
        }

        public Result VerifyPassword(string pass)
        {
            var input = ("0020008108" + pass.PadRight(16, 'F')).Hex2Bytes();
            byte[] output;
            if (!_cardDevice.CPUChipIO(false, input, out output))
                return Result.Fail("通信错误");
            var hex = output.Bytes2Hex();
            if (hex.StartsWith("63"))
                return Result.Fail($"验证失败 还有{hex[3]}次机会");
            switch (hex)
            {
                case "9000":
                    return Result.Success();

                case "6283":
                    return Result.Fail("口令密钥校验错误");

                case "6700":
                    return Result.Fail("错误的长度");

                case "6981":
                    return Result.Fail("不是口令密钥");

                case "6982":
                    return Result.Fail("密钥使用条件不满足");

                case "6983":
                    return Result.Fail("认证方法（口令密钥）锁死");

                case "6A82":
                    return Result.Fail("KEY文件未找到");

                case "9302":
                    return Result.Fail("密钥线路保护错误");

                case "9403":
                    return Result.Fail("密钥未找到");

                default:
                    return Result.Fail(hex);
            }
        }

        public Result<byte[]> Decrypt(byte[] key, byte[] chk)
        {
            var input = ("80F8010210" + key.Bytes2Hex() + "10").Hex2Bytes();
            byte[] output;
            if (!_cardDevice.CPUChipIO(false, input, out output))
                return Result<byte[]>.Fail("通信错误");
            var hex = output.Bytes2Hex();
            var res = hex.Substring(hex.Length - 4);
            if (res.StartsWith("61"))
                return KeyCheck(output.Take(output.Length - 2).ToArray(), chk);
            switch (res)
            {
                case "9000":
                    return KeyCheck(output.Take(output.Length - 2).ToArray(), chk);

                case "6700":
                    return Result<byte[]>.Fail("错误的长度");

                case "6981":
                    return Result<byte[]>.Fail("密钥与运算方法不匹配");

                case "6982":
                    return Result<byte[]>.Fail("不满足安全状态");

                case "6985":
                    return Result<byte[]>.Fail("不满足使用条件");

                case "6A82":
                    return Result<byte[]>.Fail("KEY文件不存在");

                case "9403":
                    return Result<byte[]>.Fail("密钥未找到");

                default:
                    return Result<byte[]>.Fail(res);
            }
        }

        private Result<byte[]> KeyCheck(byte[] key, byte[] chk)
        {
            var c = new Cryptography
            {
                CipherMode = CipherMode.ECB,
                IV = new byte[8],
                Key = key,
                PaddingMode = PaddingMode.None
            };
            var myChk = c.TripleDESEncrypt(new byte[8]);
            if (myChk[0] == chk[0] && myChk[1] == chk[1])
                return Result<byte[]>.Success(key);
            return Result<byte[]>.Fail("密钥验证失败");
        }
    }
}