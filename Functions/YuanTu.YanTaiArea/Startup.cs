using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.YanTaiArea
{    
    public class Startup : DefaultStartup
    {
     
        /// <summary>
        /// 优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => 100;


        public override string[] UseConfigPath()
        {
            return ConfigYanTai.UseConfigPath();
           
        }
        public override void AfterStartup()
        {
            #region 自动更新简略版
            try
            {
                if (FrameworkConst.AutoUpdateEnable)
                {
                    YuanTu.AutoUpdater.Update.Do();
                }
            }
            catch (Exception ex)
            {
                var str = GetExceptionMsg(ex, ex.ToString());
                YuanTu.Core.Log.Logger.Main.Error("[系统错误0x000000003]" + ex.ToString());
            }
            #endregion
            ConfigYanTai.Init();
        }
        private static string GetExceptionMsg(Exception ex, string backStr)
        {
            var sb = new StringBuilder();
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTimeCore.Now);
            if (ex != null)
            {
                sb.AppendLine("【异常类型】：" + ex.GetType().Name);
                sb.AppendLine("【异常信息】：" + ex.Message);
                if (ex is ReflectionTypeLoadException)
                {
                    var le = ex as ReflectionTypeLoadException;
                    foreach (var exception in le.LoaderExceptions)
                    {
                        sb.AppendLine("【加载异常】：" + exception.Message);
                    }
                }
                else
                {
                    sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
                    sb.AppendLine("【其他错误】：" + ex.Source);
                }
                var innexcpetion = ex.InnerException;
                var innIndex = 0;
                while (innexcpetion != null)
                {
                    sb.AppendLine($"【内部异常{innIndex++}】：" + innexcpetion.Source);
                    innexcpetion = innexcpetion.InnerException;
                }
            }
            else
            {
                sb.AppendLine("【未处理异常】：" + backStr);
            }
            sb.AppendLine("***************************************************************");
            return sb.ToString();
        }
    }
}