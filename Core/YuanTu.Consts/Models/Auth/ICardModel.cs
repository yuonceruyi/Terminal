using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.Auth
{
    public interface ICardModel : IModel
    {
        /// <summary>
        ///     病人当前持卡卡号
        /// </summary>
        string CardNo { get; set; }

        /// <summary>
        ///     病人当前持有的卡种
        /// </summary>
        CardType CardType { get; set; }

        /// <summary>
        ///     是否是监护人卡
        /// </summary>
        bool IsGuard { get; set; }

        /// <summary>
        ///     病人卡密码
        /// </summary>
        string CardPassword { get; set; }

        /// <summary>
        ///     卡中的附加验证信息
        /// </summary>
        string ExternalCardInfo { get; set; }
    }

    public class DefaultCardModel : ModelBase, ICardModel
    {
        public string CardNo { get; set; }
        public CardType CardType { get; set; }
        public bool IsGuard { get; set; }
        public string CardPassword { get; set; }
        public string ExternalCardInfo { get; set; }
    }
}