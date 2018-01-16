using System.Windows.Media;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Configs;

namespace YuanTu.Consts.Services
{
    public interface IConfigurationManager : IService
    {
        /// <summary>
        ///     Key路径，层级Key用冒号(:)连接
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetValue(string key);
        /// <summary>
        ///     Key路径，层级Key用冒号(:)连接
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Section[] GetValues(string key);
    }

    public static class ConfigurationManagerExtension
    {
        public static int GetValueInt(this IConfigurationManager manager, string key, int defaultVal = 0)
        {
            var val = manager.GetValue(key);
            if (val == null)
            {
                return defaultVal;
            }
            var t = 0;
            if (int.TryParse(val, out t))
            {
                return t;
            }
            return defaultVal;
        }

        public static Color GetValueColor(this IConfigurationManager manager, string key, Color defaultVal = new Color())
        {
            var value = manager.GetValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultVal;
            var rgb = value.Split(',');
            if (rgb.Length < 3)
                return defaultVal;

            if (!byte.TryParse(rgb[0], out byte r))
                return defaultVal;
            if (!byte.TryParse(rgb[1], out byte g))
                return defaultVal;
            if (!byte.TryParse(rgb[2], out byte b))
                return defaultVal;
            return Color.FromRgb(r, g, b);
        }
    }
}