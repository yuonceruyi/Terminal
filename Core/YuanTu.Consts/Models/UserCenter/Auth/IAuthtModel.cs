using YuanTu.Consts.Enums.UserCenter;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.UserCenter.Entities;

namespace YuanTu.Consts.Models.UserCenter.Auth
{
    /// <summary>
    /// 用户管理的验证
    /// </summary>
    public interface IAuthModel : IModel
    {
      

        PatientVO 当前就诊人信息 { get; set; }
        /// <summary>
        /// 用户管理的卡号
        /// </summary>
        string CardNo { get; set; }
        /// <summary>
        /// 用户管理的卡类型
        /// </summary>
        CardType CardType { get; set; }
    }

    public class AuthModel : ModelBase, IAuthModel
    {
      
        public string CardNo { get; set; }
        public CardType CardType { get; set; }
        public PatientVO 当前就诊人信息 { get; set; }
    }
}