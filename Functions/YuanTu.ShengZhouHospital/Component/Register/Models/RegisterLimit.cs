using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.ShengZhouHospital.Component.Register.Models
{

    public class RegisterLimitTools
    {
        private const string path = "CurrentResource\\YuanTu.ShengZhouHospital\\挂号限制.json";
        private static RegisterLimit _registerLimit;

        public static RegisterLimit RegisterLimit
        {
            get
            {
                try
                {
                    if (_registerLimit == null)
                    {
                        var full = Path.Combine(FrameworkConst.RootDirectory, path);
                        if (File.Exists(full))
                        {
                            var content = File.ReadAllText(full, Encoding.UTF8);
                            _registerLimit = content.ToJsonObject<RegisterLimit>();
                        }
                    }
                }
                catch (Exception ex)
                {

                    Logger.Main.Error($"[挂号限制]获取挂号限制配置报错，{ex.Message} {ex.StackTrace}");
                }
              
                return _registerLimit;
            }
        }

        public static Result JudgeDeptTypes(string typeName,int age, int sex)
        {
            if (RegisterLimit==null)
            {
                return Result.Success();
            }
            var suitType = RegisterLimit.科室类别限制.FirstOrDefault(p => typeName.Contains(p.类别名称) || p.类别名称.Contains(typeName));
            if (suitType == null)
            {
                return Result.Success();
            }
            return suitType.Judge(age, sex);
        }
        public static Result JudgeDeptss(string deptName, int age, int sex)
        {
            if (RegisterLimit == null)
            {
                return Result.Success();
            }
            var suitdept = RegisterLimit.科室限制.FirstOrDefault(p => deptName.Contains(p.科室名称) || p.科室名称.Contains(deptName));
            if (suitdept == null)
            {
                return Result.Success();
            }
            return suitdept.Judge(age, sex);
        }
    }
    public class RegisterLimit
    {
        public 科室类别限制[] 科室类别限制 { get; set; }
        public 科室限制[] 科室限制 { get; set; }

       
    }

    public class 科室类别限制
    {
        public string 类别名称 { get; set; }
        public 允许范围[] 允许范围 { get; set; }

        public Result Judge( int age, int sex)
        {
            var errs = new List<string>();
            var errorCode = 0l;
            foreach (var range in 允许范围)
            {
                var ret = range.Judge(age, sex);
                if (ret.IsSuccess)
                {
                    return ret;
                }
                errorCode = ret.ResultCode;
                errs.Add(ret.Message);
            }
            return Result.Fail($"仅能在{string.Join(" ", errs)}操作该功能\r\n{(errorCode == -1 ? "其他时间请挂急诊号" : "")}");
        }

    }

    public class 科室限制
    {
        public string 科室名称 { get; set; }
        public 允许范围[] 允许范围 { get; set; }
        public Result Judge( int age, int sex)
        {
            var errs = new List<string>();
            var errorCode = 0l;
            foreach (var range in 允许范围)
            {
                var ret = range.Judge(age, sex);
                if (ret.IsSuccess)
                {
                    return ret;
                }
                errorCode = ret.ResultCode;
                errs.Add(ret.Message);
            }
            return Result.Fail($"仅能在{string.Join(" ", errs)}操作该功能\r\n{(errorCode == -1 ? "其他时间请挂急诊号" : "")}");
        }
    }


    public class 允许范围
    {
        public string 起始时间 { get; set; }="00:00";
        public string 结束时间 { get; set; }="23:59";

        public int 最小年龄 { get; set; } = 0;
        public int 最大年龄 { get; set; } = 9999;
        public int 性别 { get; set; } = 0;

        public Result Judge(int age, int sex)
        {
            try
            {
                var startTime = (DateTime.ParseExact(起始时间, "HH:mm", null) );
                var endTime = (DateTime.ParseExact(结束时间, "HH:mm", null));
                if (startTime>endTime)//隔天了
                {
                    endTime = endTime.AddDays(1);
                }
                var nowtime = DateTime.ParseExact(DateTimeCore.Now.ToString("HH:mm"), "HH:mm", null);
                if (nowtime<=startTime||nowtime>=endTime)
                {
                    return Result.Fail(-1,$"【{起始时间}-{结束时间}】");
                }
                if (age<最小年龄||age>最大年龄)
                {
                    return Result.Fail(-1, $"【{最小年龄}岁-{最小年龄}岁】");
                }

                if (性别!=0)
                {
                    if (sex!=性别)
                    {
                        return Result.Fail(-2,$"性别为[{(sex == 1 ? "男性":"女性")}]");
                    }
                }
                

                return Result.Success();

            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[挂号限制]匹配规则发生错误，{ex.Message} {ex.StackTrace}");
                return Result.Fail("发生错误，该配置不可用");

            }
        } 

    }

    
}
