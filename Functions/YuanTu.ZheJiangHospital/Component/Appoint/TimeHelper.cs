using System;
using YuanTu.Consts;

namespace YuanTu.ZheJiangHospital.Component.Appoint
{
    /// <summary>
    ///     时间戳
    /// </summary>
    public static class TimeHelper
    {
        private static readonly DateTime StartTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

        /// <summary>
        ///     获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(this DateTime time, int length = 13)
        {
            var ts = ConvertDateTimeToInt(time);
            return ts.ToString().Substring(0, length);
        }

        /// <summary>
        ///     将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>long</returns>
        public static long ConvertDateTimeToInt(this DateTime time)
        {
            var t = (time.Ticks - StartTime.Ticks) / 10000; //除10000调整为13位
            return t;
        }

        /// <summary>
        ///     时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ConvertStringToDateTime(string timeStamp)
        {
            var lTime = long.Parse(timeStamp + "0000");
            var toNow = new TimeSpan(lTime);
            return StartTime.Add(toNow);
        }

        /// <summary>
        ///     时间戳转为C#格式时间10位
        /// </summary>
        /// <param name="curSeconds">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetDateTimeFrom1970Ticks(long curSeconds)
        {
            return StartTime.AddSeconds(curSeconds);
        }

        /// <summary>
        ///     验证时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <param name="interval">差值（分钟）</param>
        /// <returns></returns>
        public static bool IsTime(long time, double interval)
        {
            var dt = GetDateTimeFrom1970Ticks(time);
            //取现在时间
            var dt1 = DateTimeCore.Now.AddMinutes(interval);
            var dt2 = DateTimeCore.Now.AddMinutes(interval * -1);
            if (dt > dt2 && dt < dt1)
                return true;
            return false;
        }

        /// <summary>
        ///     判断时间戳是否正确（验证前8位）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsTime(string time)
        {
            var str = GetTimeStamp(DateTimeCore.Now, 8);
            if (str.Equals(time))
                return true;
            return false;
        }
    }
}