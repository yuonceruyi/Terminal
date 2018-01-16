using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace YuanTu.Consts.Models.Print
{
    public  class PrintConfig
    {
        public static readonly StringFormat Center = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };

        public static readonly StringFormat Left = new StringFormat
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near
        };

        public static float DefaultX = 10;
        public static float Default3X = 220;

        public static  Font HeaderFont = new Font("微软雅黑", (CurrentStrategyType()== DeviceType.Clinic ? 12 : 16), FontStyle.Bold);
        public static  Font HeaderFont2 = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold);
        public static Font DefaultFont= new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 8 : 12), FontStyle.Regular);
        public static Dictionary<string, string[]> GetStrategy()
        {
            return DeviceType.FallBackToDefaultStrategy;
        }

        public static string CurrentStrategyType()
        {
            var stg = GetStrategy();
            if (stg.ContainsKey(FrameworkConst.DeviceType))
                return stg[FrameworkConst.DeviceType].First();
            return "";
        }

    }
}