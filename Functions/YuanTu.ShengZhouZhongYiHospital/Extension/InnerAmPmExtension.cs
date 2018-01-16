using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;

namespace YuanTu.ShengZhouZhongYiHospital.Extension
{
    public static class InnerAmPmExtension
    {

        public static AmPmSession JudgeAmPm(this string time, string formatter = "HH:mm")
        {
            if (time.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(time));
            }
            DateTime dtime;
            TimeSpan tspan;
            if (DateTime.TryParseExact(time, formatter, null, DateTimeStyles.None, out dtime))
            {
                tspan = dtime - dtime.Date;
            }
            else
            {
                throw new FormatException($"时间'{time}' 不符合'{formatter}'格式");
            }
            if (tspan.Hours >= 12)
            {
                return AmPmSession.下午;
            }
            else
            {
                return AmPmSession.上午;
            }
        }

        public static AmPmSession JudgeAmPm(this DateTime time)
        {
            return time.Hour >= 12 ? AmPmSession.下午 : AmPmSession.上午;
        }
    }
}
