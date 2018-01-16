using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDKouQiangYY.Component.Register.Services
{
    public class DoctTechInfoHelper
    {
        private static readonly Dictionary<string,string> _dic =new Dictionary<string, string>
        {
            ["专家"]="专",
            ["普通"]="普",
            ["便民"]="",
            ["名医"]="",
            ["急诊"]="",
        }; 
        public static Result<string> GetShortDescription(string fullDescription)
        {
            if (_dic.ContainsKey(fullDescription))
            {
                return Result<string>.Success(_dic[fullDescription]);
            }
            return Result<string>.Fail($"{fullDescription}非法");

        } 
    }
}
