using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.Component.ViewModels;

namespace YuanTu.Default.Component.Tools.Models
{
    /// <summary>
    /// 第三方支付专属Model
    /// </summary>
    public interface IExtraPaymentModel:IModel
    {
        /// <summary>
        /// 需要支付的金额(单位：分)
        /// </summary>
        decimal TotalMoney { get; set; }
        /// <summary>
        /// 当前的实际业务(与流程业务区分开)
        /// </summary>
        Business CurrentBusiness { get; set; }
        /// <summary>
        /// 当前实际支付方式
        /// </summary>
        PayMethod CurrentPayMethod { get; set; }
       /// <summary>
       /// 是否已经完成支付操作
       /// </summary>
        bool Complete { get; set; }
        /// <summary>
        /// 支付成功后的回调
        /// </summary>
        Func<Task<Result>> FinishFunc { get; set; }

        /// <summary>
        /// 第三方支付成功后，返回的具体参数
        /// 通常现金支付没有返回，Pos支付TransResDto，支付宝微信是“订单状态”
        /// </summary>
        object PaymentResult { get; set; }
        /// <summary>
        /// 第三方支付余额
        /// </summary>
        decimal ThridRemain { get; set; }
        /// <summary>
        /// 病人信息 姓名 id 身份证号 监护人身份证号 卡号 余额 
        /// </summary>
        PatientInfo PatientInfo { get; set; }
    }

    public class ExtraPaymentModel :ModelBase, IExtraPaymentModel
    {
        #region Implementation of IExtraPaymentModel

        public decimal TotalMoney { get; set; }
        public Business CurrentBusiness { get; set; }
        public PayMethod CurrentPayMethod { get; set; }
        public bool Complete { get; set; }

        public Func<Task<Result>> FinishFunc { get; set; }

        public object PaymentResult { get; set; }

        public decimal ThridRemain { get; set; }
        public PatientInfo PatientInfo { get; set; }

        #endregion
    }

 
    public class PatientInfo
    {
        public string Name { get; set; }
        public string PatientId { get; set; }
        public string IdNo { get; set; }
        public string GuardianNo { get; set; }

        public string CardNo { get; set; }
        public CardType CardType { get; set; }
 
        public decimal Remain { get; set; }
    }
}
