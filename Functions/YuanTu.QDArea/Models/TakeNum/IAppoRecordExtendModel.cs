using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.QDArea.Models.TakeNum
{
    public interface IAppoRecordExtendModel : IModel
    {
        /// <summary>
        ///1 默认 
        ///2 预约收费增加支付状态
        /// </summary>
        string version { get; set; }

    }

    public class AppoRecordExtendModel : ModelBase, IAppoRecordExtendModel
    {
        public string version { get; set; } = "0";
    }
}
