using System.Reflection;

namespace YuanTu.ShengZhouHospital.HisNative.Models
{
   public abstract class HisReq
   {
        /// <summary>
        /// 服务编号
        /// </summary>
       public abstract string 服务编号 { get; }




       public string Serialize()
       {
           return Build();
        }

       protected abstract string Build();

   }
}
