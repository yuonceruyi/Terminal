using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Enums;

namespace YuanTu.Consts.Models
{
    public interface IChoiceModel:IModel
    {
        /// <summary>
        /// 是否有病人信息认证页面(插卡页面)
        /// </summary>
        bool HasAuthFlow { get; set; }
        /// <summary>
        /// 验证流程 默认ChaKa_Context
        /// </summary>
        string AuthContext { get; set; }
        Business Business { get; set; }
        //HouseBusiness HouseBusiness { get; set; }
    }

    public class ChoiceModel : ModelBase, IChoiceModel
    {
        public bool HasAuthFlow { get; set; }
        public string AuthContext { get; set; } = A.ChaKa_Context;
        public Business Business { get; set; }
        //public HouseBusiness HouseBusiness { get; set; }
    }
}
