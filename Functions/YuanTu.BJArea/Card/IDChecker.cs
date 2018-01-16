using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.BJArea.Card
{
    public class IDChecker
    {
        private static readonly int[] Weight = //十七位数字本体码权重
        {7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2};

        private static readonly string Validate = "10X98765432"; //mod11,对应校验码字符值
        private static readonly DateTime Begin = new DateTime(1850, 1, 1);

        public static char GetValidateCode(string id17)
        {
            if (string.IsNullOrEmpty(id17))
                throw new ArgumentNullException(nameof(id17));
            if (id17.Length < 17)
                throw new ArgumentOutOfRangeException(nameof(id17));
            var sum = 0;

            for (var i = 0; i < 17; i++)
                sum = sum + (id17[i] - '0') * Weight[i];

            return Validate[sum % 11];
        }

        public static bool Check(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            if (id.Length != 18)
                return false;

            var dateString = $"{id.Substring(6, 4)}-{id.Substring(10, 2)}-{id.Substring(12, 2)}";

            DateTime date;
            var dateValid = DateTime.TryParse(dateString, out date);

            if (!dateValid)
                return false;
            if (date < Begin || date > DateTimeCore.Today)
                return false;

            return id[17] == GetValidateCode(id);
        }
    }
}
