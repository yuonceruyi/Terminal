using System;
using System.Globalization;
using System.Text;
using YuanTu.Consts;

namespace YuanTu.Core.Extension {
    public static class DateTimeExtensions {
        public static readonly DateTime BaseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static DateTime FirstDayOfMonth(this DateTime date) {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date) {
            return date.FirstDayOfMonth().AddMonths(1);
        }

        public static bool IsInInterval(this DateTime date, DateTime minDate, DateTime maxDate) {
            return minDate <= date && date < maxDate;
        }

        public static bool IsInInterval(this DateTime date, DateTime minDate, DateTime? maxDate) {
            return minDate <= date && (maxDate == null || date < maxDate);
        }

        public static bool IsInInterval(this DateTime date, DateTime? minDate, DateTime? maxDate) {
            return (minDate == null || minDate <= date) &&
                   (maxDate == null || date < maxDate);
        }

        static void AssertDateOnly(DateTime? date) {
            if (date == null)
                return;
            var d = date.Value;
            if (d.Hour != 0 || d.Minute != 0 || d.Second != 0 || d.Millisecond != 0)
                throw new InvalidOperationException("The date has some hours, minutes, seconds or milliseconds");
        }

        public static bool IsInDateInterval(this DateTime date, DateTime minDate, DateTime maxDate) {
            AssertDateOnly(date);
            AssertDateOnly(minDate);
            AssertDateOnly(maxDate);
            return minDate <= date && date <= maxDate;
        }

        public static bool IsInDateInterval(this DateTime date, DateTime minDate, DateTime? maxDate) {
            AssertDateOnly(date);
            AssertDateOnly(minDate);
            AssertDateOnly(maxDate);
            return (minDate == null || minDate <= date) &&
                   (maxDate == null || date < maxDate);
        }

        public static bool IsInDateInterval(this DateTime date, DateTime? minDate, DateTime? maxDate) {
            AssertDateOnly(date);
            AssertDateOnly(minDate);
            AssertDateOnly(maxDate);
            return (minDate == null || minDate <= date) &&
                   (maxDate == null || date < maxDate);
        }

        public static int YearsTo(this DateTime min, DateTime max) {
            var result = max.Year - min.Year;
            if (max.Month < min.Month || (max.Month == min.Month & max.Day < min.Day))
                result--;

            return result;
        }

        public static int MonthsTo(this DateTime min, DateTime max) {
            var result = max.Month - min.Month + (max.Year - min.Year) * 12;
            if (max.Day < min.Day)
                result--;

            return result;
        }
        
        public static DateTime Min(DateTime a, DateTime b) {
            return a < b ? a : b;
        }

        public static DateTime Max(DateTime a, DateTime b) {
            return a > b ? a : b;
        }

        /// <param name="precision">Using Milliseconds does nothing, using Days use DateTime.Date</param>
        public static DateTime TrimTo(this DateTime dateTime, DateTimePrecision precision) {
            switch (precision) {
                case DateTimePrecision.Days: return dateTime.Date;
                case DateTimePrecision.Hours: return TrimToHours(dateTime);
                case DateTimePrecision.Minutes: return TrimToMinutes(dateTime);
                case DateTimePrecision.Seconds: return TrimToSeconds(dateTime);
                case DateTimePrecision.Milliseconds: return dateTime;
            }
            throw new ArgumentException("precission");
        }

        public static DateTime TrimToSeconds(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind);
        }

        public static DateTime TrimToMinutes(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);
        }

        public static DateTime TrimToHours(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
        }

        public static DateTimePrecision GetPrecision(this DateTime dateTime) {
            if (dateTime.Millisecond != 0)
                return DateTimePrecision.Milliseconds;
            if (dateTime.Second != 0)
                return DateTimePrecision.Seconds;
            if (dateTime.Minute != 0)
                return DateTimePrecision.Minutes;
            if (dateTime.Hour != 0)
                return DateTimePrecision.Hours;

            return DateTimePrecision.Days;
        }

        public static TimeSpan TrimTo(this TimeSpan timeSpan, DateTimePrecision precision) {
            switch (precision) {
                case DateTimePrecision.Days: return timeSpan.TrimToDays();
                case DateTimePrecision.Hours: return TrimToHours(timeSpan);
                case DateTimePrecision.Minutes: return TrimToMinutes(timeSpan);
                case DateTimePrecision.Seconds: return TrimToSeconds(timeSpan);
                case DateTimePrecision.Milliseconds: return timeSpan;
            }
            throw new ArgumentException("precission");
        }

        public static TimeSpan TrimToSeconds(this TimeSpan dateTime) {
            return new TimeSpan(dateTime.Days, dateTime.Hours, dateTime.Minutes, dateTime.Seconds);
        }

        public static TimeSpan TrimToMinutes(this TimeSpan dateTime) {
            return new TimeSpan(dateTime.Days, dateTime.Hours, dateTime.Minutes, 0);
        }

        public static TimeSpan TrimToHours(this TimeSpan dateTime) {
            return new TimeSpan(dateTime.Days, dateTime.Hours, 0, 0);
        }

        public static TimeSpan TrimToDays(this TimeSpan dateTime) {
            return new TimeSpan(dateTime.Days, 0, 0, 0);
        }

        public static DateTimePrecision? GetPrecision(this TimeSpan timeSpan) {
            if (timeSpan.Milliseconds != 0)
                return DateTimePrecision.Milliseconds;
            if (timeSpan.Seconds != 0)
                return DateTimePrecision.Seconds;
            if (timeSpan.Minutes != 0)
                return DateTimePrecision.Minutes;
            if (timeSpan.Hours != 0)
                return DateTimePrecision.Hours;
            if (timeSpan.Days != 0)
                return DateTimePrecision.Days;

            return null;
        }

        public static DateTime MonthStart(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
        }

        public static string NiceToString(this TimeSpan timeSpan) {
            return timeSpan.NiceToString(DateTimePrecision.Milliseconds);
        }

        public static string NiceToString(this TimeSpan timeSpan, DateTimePrecision precission) {
            var sb = new StringBuilder();
            var any = false;
            if (timeSpan.Days != 0 && precission >= DateTimePrecision.Days) {
                sb.AppendFormat("{0}d ", timeSpan.Days);
                any = true;
            }

            if ((any || timeSpan.Hours != 0) && precission >= DateTimePrecision.Hours) {
                sb.AppendFormat("{0,2}h ", timeSpan.Hours);
                any = true;
            }

            if ((any || timeSpan.Minutes != 0) && precission >= DateTimePrecision.Minutes) {
                sb.AppendFormat("{0,2}m ", timeSpan.Minutes);
                any = true;
            }

            if ((any || timeSpan.Seconds != 0) && precission >= DateTimePrecision.Seconds) {
                sb.AppendFormat("{0,2}s ", timeSpan.Seconds);
                any = true;
            }

            if ((any || timeSpan.Milliseconds != 0) && precission >= DateTimePrecision.Milliseconds) {
                sb.AppendFormat("{0,3}ms", timeSpan.Milliseconds);
            }

            return sb.ToString();
        }

        public static long JavascriptMilliseconds(this DateTime dateTime) {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new InvalidOperationException("dateTime should be UTC");

            return (long)new TimeSpan(dateTime.Ticks - new DateTime(1970, 1, 1).Ticks).TotalMilliseconds;
        }

        public static string MillisecondsToDateTime(this long dateTime,string format="yyyy-MM-dd HH:mm:ss")
        {
           return dateTime > 0? new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local).AddMilliseconds(dateTime).ToString(format) : DateTimeCore.Now.ToString(format);
        }

        public static int UnixSeconds(this DateTime dateTime) {
            var span = dateTime - BaseDate;
            return (int)span.TotalSeconds;
        }

        

        public static DateTime UnixSecondsToDateTime(this ValueType unixSeconds)
        {
            return BaseDate.AddSeconds((long)unixSeconds);
        }

        public static DateTime EndOfDay(this DateTime @this) {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }

        public static DateTime EndOfMonth(this DateTime @this) {
            return new DateTime(@this.Year, @this.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startDayOfWeek = DayOfWeek.Sunday) {
            var end = dt;
            var endDayOfWeek = startDayOfWeek - 1;
            if (endDayOfWeek < 0) {
                endDayOfWeek = DayOfWeek.Saturday;
            }

            if (end.DayOfWeek != endDayOfWeek) {
                if (endDayOfWeek < end.DayOfWeek) {
                    end = end.AddDays(7 - (end.DayOfWeek - endDayOfWeek));
                }
                else {
                    end = end.AddDays(endDayOfWeek - end.DayOfWeek);
                }
            }

            return new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 999);
        }

        public static DateTime EndOfYear(this DateTime @this) {
            return new DateTime(@this.Year, 1, 1).AddYears(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }

        public static DateTime FirstDayOfWeek(this DateTime @this) {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(-(int)@this.DayOfWeek);
        }

        public static bool IsAfternoon(this DateTime @this) {
            return @this.TimeOfDay >= new DateTime(2000, 1, 1, 12, 0, 0).TimeOfDay;
        }

        public static bool IsFuture(this DateTime @this) {
            return @this > DateTimeCore.Now;
        }

        public static bool IsMorning(this DateTime @this) {
            return @this.TimeOfDay < new DateTime(2000, 1, 1, 12, 0, 0).TimeOfDay;
        }

        public static bool IsNow(this DateTime @this) {
            return @this == DateTimeCore.Now;
        }

        public static bool IsPast(this DateTime @this) {
            return @this < DateTimeCore.Now;
        }

        public static bool IsTimeEqual(this DateTime time, DateTime timeToCompare) {
            return (time.TimeOfDay == timeToCompare.TimeOfDay);
        }

        public static bool IsDateEqual(this DateTime date, DateTime dateToCompare) {
            return (date.Date == dateToCompare.Date);
        }

        public static bool IsToday(this DateTime @this) {
            return @this.Date == DateTime.Today;
        }

        public static bool IsWeekDay(this DateTime @this) {
            return !(@this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday);
        }

        public static bool IsWeekendDay(this DateTime @this) {
            return (@this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday);
        }

        public static DateTime LastDayOfWeek(this DateTime @this) {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(6 - (int)@this.DayOfWeek);
        }

        public static DateTime SetTime(this DateTime current, int hour) {
            return SetTime(current, hour, 0, 0, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute) {
            return SetTime(current, hour, minute, 0, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute, int second) {
            return SetTime(current, hour, minute, second, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute, int second, int millisecond) {
            return new DateTime(current.Year, current.Month, current.Day, hour, minute, second, millisecond);
        }

        public static DateTime StartOfDay(this DateTime @this) {
            return new DateTime(@this.Year, @this.Month, @this.Day);
        }

        public static DateTime StartOfMonth(this DateTime @this) {
            return new DateTime(@this.Year, @this.Month, 1);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startDayOfWeek = DayOfWeek.Sunday) {
            var start = new DateTime(dt.Year, dt.Month, dt.Day);

            if (start.DayOfWeek != startDayOfWeek) {
                var d = startDayOfWeek - start.DayOfWeek;
                if (startDayOfWeek <= start.DayOfWeek) {
                    return start.AddDays(d);
                }
                return start.AddDays(-7 + d);
            }

            return start;
        }

        public static DateTime StartOfYear(this DateTime @this) {
            return new DateTime(@this.Year, 1, 1);
        }

        public static TimeSpan ToEpochTimeSpan(this DateTime @this) {
            return @this.Subtract(BaseDate);
        }

        public static DateTime Tomorrow(this DateTime @this) {
            return @this.AddDays(1);
        }

        public static DateTime Yesterday(this DateTime @this) {
            return @this.AddDays(-1);
        }

        public static string FriendlyFormat(this DateTime @this) {
            var span = DateTimeCore.Now - @this;
            if (span.TotalDays > 60) {
                return @this.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (span.TotalDays > 30) {
                return "1个月前";
            }
            else if (span.TotalDays > 14) {
                return "2周前";
            }
            else if (span.TotalDays > 7) {
                return "1周前";
            }
            else if (span.TotalDays > 1) {
                return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
            }
            else if (span.TotalHours > 1) {
                return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
            }
            else if (span.TotalMinutes > 1) {
                return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
            }
            else if (span.TotalSeconds >= 1) {
                return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
            }
            else {
                return "1秒前";
            }
        }

        public static bool IsValid(this DateTime @this) {
            return @this.Year > 1970;
        }

        public static string ToRelativeString(this DateTime dt) {
            var span = (DateTimeCore.Now - dt);

            // Normalize time span
            var future = false;
            if (span.TotalSeconds < 0) {
                // In the future
                span = -span;
                future = true;
            }

            // Test for Now
            var totalSeconds = span.TotalSeconds;
            if (totalSeconds < 0.9) {
                return "现在";
            }

            // Date/time near current date/time
            var format = (future) ? "{0} {1} 之后" : "{0} {1} 之前";
            if (totalSeconds < 55) {
                // Seconds
                var seconds = Math.Max(1, span.Seconds);
                return String.Format(format, seconds, "秒");
            }

            if (totalSeconds < (55 * 60)) {
                // Minutes
                var minutes = Math.Max(1, span.Minutes);
                return String.Format(format, minutes, "分");
            }
            if (totalSeconds < (24 * 60 * 60)) {
                // Hours
                var hours = Math.Max(1, span.Hours);
                return String.Format(format, hours, "小时");
            }

            // Format both date and time
            if (totalSeconds < (48 * 60 * 60)) {
                // 1 Day
                format = (future) ? "明天" : "昨天";
            }
            else if (totalSeconds < (3 * 24 * 60 * 60)) {
                // 2 Days
                format = String.Format(format, 2, "天");
            }
            else {
                // Absolute date
                if (dt.Year == DateTimeCore.Now.Year)
                    format = dt.ToString(@"MMM d");
                else
                    format = dt.ToString(@"yyyy MMM d");
            }

            // Add time
            return String.Format("{0} at {1:h:mmt}", format, dt);
        }

        public static string ToRelativeString(this TimeSpan span) {
            var dayDiff = (int)span.TotalDays;
            var minuteDiff = (int)span.TotalMinutes;
            var hourDiff = (int)span.TotalHours;
            var secDiff = (int)span.TotalSeconds;

            var future = secDiff < 0;
            if (dayDiff > 0) {
                return span.ToString("%d'天'h'小时'mm'分'ss'秒'");
            }
            if (hourDiff > 0) {
                return span.ToString("h'小时'mm'分'ss'秒'");
            }
            if (minuteDiff > 0) {
                return span.ToString("mm'分'ss'秒'");
            }
            if (secDiff > 0) {
                return span.ToString("s'秒'");
            }
            return "现在";
        }

        public static string SafeConvertToDate(this string date,string originformat,string format)
        {
            DateTime _date;
            if (DateTime.TryParseExact(date, originformat,null,DateTimeStyles.None, out _date))
            {
                return _date.ToString(format);
            }
            return date;
        }

        public static int Age(this string date)
        {
            return DateTime.Parse(date).Age();             
        }
        public static int Age(this DateTime date)
        {
            DateTime now = DateTimeCore.Today;
            int age = now.Year - date.Year;
            if (date > now.AddYears(-age))
                age--;
            return age;
        }


        public static int ToAge(this string dateString)
        {
            DateTime date = DateTime.Parse(dateString);
            DateTime now = DateTimeCore.Today;
            int age = now.Year - date.Year;
            if (date > now.AddYears(-age))
                age--;
            if (age == 0)
                age++;
            return age;
        }

        //public static DateTime SafeConvertToDate(this string date, string formatter)
        //{

        //}


    }

    public enum DateTimePrecision {
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds,
    }
}
