using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.FuYangRMYY.HisNative.Models.Base
{
   public abstract class InsuranceResponseBase<T> where T: InsuranceResponseBase<T>, new()
    {
        public string OriginStr { get; private set; }
        public string[] ExternalContene { get; private set; }
        /// <summary>
        /// 每个数据域之间的分隔符
        /// </summary>
        public abstract char  DataSplit { get; }
        /// <summary>
        /// 提示域和数据域分隔符
        /// </summary>
        public abstract char OriginSplit { get; }
        public static Result<T> Build(string content)
        {
            var t = new T {OriginStr = content};
            
            if (content.StartsWith("0"))
            {
              
                var arr = content.Split(t.OriginSplit);
                if (arr.Length>2)
                {
                    t.ExternalContene = arr.Skip(2).ToArray();
                }

                if (arr.Length>1)
                {
                    var ret = t.Format(arr[1]);
                    if (ret)
                    {
                        return Result<T>.Success(t);
                    }
                    return Result<T>.Fail(-1, ret.Message);
                }
                return Result<T>.Success(t);


            }
            return Result<T>.Fail(-1,content);
        }

        public abstract Result Format(string msg);
    }
}
