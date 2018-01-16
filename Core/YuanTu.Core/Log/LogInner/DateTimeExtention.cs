using System;

namespace YuanTu.Core.Log
{
    public static class DateTimeExtention
    {
        public static int ToYearMonth(this DateTime date)
        {
            return date.Year*100+date.Month;
        }
    }
}
