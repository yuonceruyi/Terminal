using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.BJArea.Services.BankPosBOC;

namespace YuanTu.BJArea
{
    
    public class Startup : DefaultStartup
    {
     

        /// <summary>
        /// 优先级，数值越小优先级越高，内部配置越优先被使用
        /// </summary>
        public override int Order => 100;

        public override string[] UseConfigPath()
        {
            return ConfigBJ.UseConfigPath();
           
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
            ConfigBJ.Init();
            LoadPatientInfo();
        }
        public void LoadPatientInfo()
        {
            #region 区域信息
            using (var zipArchive =
                new ZipArchive(File.OpenRead(Path.Combine(FrameworkConst.RootDirectory, @"Resource\YuanTu.BJArea\区域信息.zip")), ZipArchiveMode.Read))
            {
                var entry = zipArchive.GetEntry("区域信息.txt");
                using (var stream = entry.Open())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var s = sr.ReadToEnd();
                        var list = s.ToJsonObject<List<AddressItem>>();
                        AddressInfo = new AddressInfo(list);
                    }

                }
            }
            Logger.Main.Info("获取区域信息成功");
            #endregion
            #region 监护人关系
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var v = config.GetValue("PatientInfo:Relation");
            RelationInfo = new RelationInfo()
            {
                List = v.Split('|').Select(t => {
                    if (t.Contains("#"))
                    {
                        return new RelationItem()
                        {
                            Code = t.Split('#')[1],
                            Name = t.Split('#')[0],
                        };
                    }
                    else
                    {
                        return new RelationItem()
                        {
                            Code = t,
                            Name = t,
                        };
                    }
                }).ToList()
            };
            Logger.Main.Info("获取监护人关系配置[" + RelationInfo.List.Count.ToString() + "]" + v);
            #endregion

            #region 宗教信仰
            v = config.GetValue("PatientInfo:Religion");
            ReligionInfo = new ReligionInfo()
            {
                List = v.Split('|').Select(t => {
                    if (t.Contains("#"))
                    {
                        return new ReligionItem()
                        {
                            Code = t.Split('#')[1],
                            Name = t.Split('#')[0],
                        };
                    }
                    else
                    {
                        return new ReligionItem()
                        {
                            Code = t,
                            Name = t,
                        };
                    }
                }).ToList()
            };
            Logger.Main.Info("获取宗教信仰配置[" + ReligionInfo.List.Count.ToString() + "]" + v);
            #endregion
            #region 教育程度
            v= config.GetValue("PatientInfo:Education");
            EducationInfo = new EducationInfo()
            {
                List = v.Split('|').Select(t => {
                    if (t.Contains("#"))
                    {
                        return new EducationItem()
                        {
                            Code = t.Split('#')[1],
                            Name = t.Split('#')[0],
                        };
                    }
                    else
                    {
                        return new EducationItem()
                        {
                            Code = t,
                            Name = t,
                        };
                    }
                }).ToList()
            };
            Logger.Main.Info("获取教育程度配置[" + ReligionInfo.List.Count.ToString() + "]" + v);
            #endregion
            #region 民族
            v = config.GetValue("PatientInfo:Nation");
            NationInfo = new NationInfo()
            {
                List = v.Split('|').Select(t => {
                    if (t.Contains("#"))
                    {
                        return new NationItem()
                        {
                            Code = t.Split('#')[1],
                            Name = t.Split('#')[0],
                        };
                    }
                    else
                    {
                        return new NationItem()
                        {
                            Code = t,
                            Name = t,
                        };
                    }
                }).ToList()
            };
            Logger.Main.Info("获取民族配置[" + ReligionInfo.List.Count.ToString() + "]");
            #endregion
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


        public static AddressInfo AddressInfo { get; set; }
        public static RelationInfo RelationInfo { get; set; }
        public static ReligionInfo ReligionInfo { get; set; }
        public static EducationInfo EducationInfo { get; set; }
        public static NationInfo NationInfo { get; set; }


    }
}