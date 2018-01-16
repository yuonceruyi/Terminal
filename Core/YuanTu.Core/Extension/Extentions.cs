using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;

namespace YuanTu.Core.Extension
{
    public static class Extentions
    {
        /// <summary>
        ///     将精度为 分 的十进制数转化为[##.##元]格式
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string In元(this decimal i) => $"{i / 100m:F2}元";

        /// <summary>
        ///     将精度为 元 的十进制数转化为[##.##元]格式
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string In元(this double i) => $"{i:F2}元";

        public static string InRMB(this decimal i) => (i / 100m).ToString("F2");

        public static string InRMB(this double i) => i.ToString("F2");

        public static string In元(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return "0.00元";
            decimal i;
            if (decimal.TryParse(s, out i))
                return i.In元();
            double d;
            if (double.TryParse(s, out d))
                return d.In元();
            return $"{s} 解析失败";
        }

        public static string In大写(this int i)
        {
            return ConvertToChinese((decimal) (i / 100.00));
        }

        public static string In大写(this double i)
        {
            return ConvertToChinese((decimal) (i / 100.00));
        }

        public static string In大写(this decimal i)
        {
            return ConvertToChinese(i / 100m);
        }

        public static string ConvertToChinese(decimal number)
        {
            var s = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            var d = Regex.Replace(s,
                @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))",
                "${b}${z}");
            var r = Regex.Replace(d, ".", m => "负元空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟万亿兆京垓秭穰"[m.Value[0] - '-'].ToString());
            if (r.EndsWith("元"))
                r += "整";
            return r;
        }

        public static string InRMB(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return "0.00";
            decimal i;
            if (decimal.TryParse(s, out i))
                return i.InRMB();
            double d;
            if (double.TryParse(s, out d))
                return d.InRMB();
            return $"{s} 解析失败";
        }

        public static bool IsLaterThan(this DateTime dateTime, int hour)
        {
            return dateTime > new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, 0, 0);
        }

        public static bool IsLaterThan(this DateTime dateTime, int hour, int minute)
        {
            return dateTime > new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0);
        }

        public static bool IsLaterThan(this DateTime dateTime, int hour, int minute, int second)
        {
            return dateTime > new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, second);
        }

        /// <summary>
        ///     判断是否存在分辨率适合该Window的屏幕
        ///     有则直接显示在该屏幕上
        /// </summary>
        public static void ToBestScreen(this Window window)
        {
            var width = window.Width;
            var height = window.Height;
            var screen = Screen.AllScreens.FirstOrDefault(one =>
                one.Bounds.Width >= width && one.Bounds.Height >= height);
            window.WindowStartupLocation = WindowStartupLocation.Manual;
#if DEBUG
            if (screen == null)
            {
                window.Left =  GetCommandLineArgInt("WindowLeft", 0);
                window.Top = GetCommandLineArgInt("WindowTop", 0);
            }
            else
            {
                window.Left = screen.WorkingArea.Left + GetCommandLineArgInt("WindowLeft", (int)(screen.WorkingArea.Width - width) / 2);
                window.Top = screen.WorkingArea.Top + GetCommandLineArgInt("WindowTop", (int)(screen.WorkingArea.Height - height) / 2);
            }
#else
            window.Left = screen.WorkingArea.Left + (screen.WorkingArea.Width - width) / 2;
            window.Top = screen.WorkingArea.Top + (screen.WorkingArea.Height - height) / 2;
#endif
        }

        private static int GetCommandLineArgInt(string key, int def)
        {
            var p = Environment.GetCommandLineArgs().FirstOrDefault(one => one.StartsWith(key));
            if (!string.IsNullOrEmpty(p) && p.Split('=').Length == 2)
                int.TryParse(p.Split('=')[1], out def);
            return def;
        }
    }
}