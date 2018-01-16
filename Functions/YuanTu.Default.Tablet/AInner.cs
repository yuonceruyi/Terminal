using YuanTu.Consts;

namespace YuanTu.Default.Tablet
{
    public class AInner
    {
        public static string Cashier => "收银服务";
        public static string Sale => "收款";
        public static string Refund => "退款";

        public class SY
        {
            public static string Choice => Cashier + A.Separator + nameof(Choice);
            public static string Amount => Cashier + A.Separator + nameof(Amount);
            public static string Card => Cashier + A.Separator + nameof(Card);
            public static string Scan => Cashier + A.Separator + nameof(Scan);
            public static string Print => Cashier + A.Separator + nameof(Print);
            public static string Select => Cashier + A.Separator + nameof(Select);
            public static string Input => Cashier + A.Separator + nameof(Input);
            public static string Confirm => Cashier + A.Separator + nameof(Confirm);
        }
        public class XC
        {
            public static string Hospitals => A.XianChang_Context + A.Separator + nameof(Hospitals);
        }

        public class YY
        {
            public static string Hospitals => A.XianChang_Context + A.Separator + nameof(Hospitals);
        }
    }
}