using System;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Core.Extension
{
    public static class IDChecker
    {
        private static readonly int[] Weight = //十七位数字本体码权重
        {7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2};

        private static readonly string Validate = "10X98765432"; //mod11,对应校验码字符值
        private static readonly DateTime Begin = new DateTime(1850, 1, 1);

        public static char IDGetValidateCode(this string id17)
        {
            if (string.IsNullOrEmpty(id17))
                throw new ArgumentNullException(nameof(id17));
            if (id17.Length < 17)
                throw new ArgumentOutOfRangeException(nameof(id17));
            if (!id17.Take(17).All(char.IsDigit))
                throw new ArgumentOutOfRangeException(nameof(id17));
            var sum = 0;

            for (var i = 0; i < 17; i++)
                sum = sum + (id17[i] - '0') * Weight[i];

            return Validate[sum % 11];
        }

        public static bool IDCheck(this string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            if (id.Length != 18)
                return false;

            if(!id.Take(17).All(char.IsDigit) || !char.IsDigit(id[17]) && id[17] != 'X')
                return false;

            var dateString = $"{id.Substring(6, 4)}-{id.Substring(10, 2)}-{id.Substring(12, 2)}";

            DateTime date;
            var dateValid = DateTime.TryParse(dateString, out date);

            if (!dateValid)
                return false;
            if (date < Begin || date > DateTimeCore.Today)
                return false;

            return id[17] == IDGetValidateCode(id);
        }

        public static Result<string> ID15To18(this string id15)
        {
            if(string.IsNullOrEmpty(id15) || id15.Length!= 15 || !id15.All(char.IsDigit))
                return Result<string>.Fail("卡号不正确");
            var id17 = $"{id15.Substring(0, 6)}19{id15.Substring(6)}";
            return Result<string>.Success(id17 + id17.IDGetValidateCode());
        }

        public static Result<string> ID18To15(this string id18)
        {
            if(string.IsNullOrEmpty(id18) || id18.Length!= 18 || 
                !id18.Take(17).All(char.IsDigit) || !char.IsDigit(id18[17]) && id18[17] != 'X')
                return Result<string>.Fail("卡号不正确");
            return Result<string>.Success(id18.Substring(0,6) + id18.Substring(8,9));
        }
    }
}
