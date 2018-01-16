using System;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.JiaShanHospital.HealthInsurance.Response;

namespace YuanTu.JiaShanHospital.HealthInsurance.Model
{
    public interface ISiModel : IModel
    {
        bool NeedCreate { get; set; }
        string SiPatientInfo { get; set; }
        string CardHardInfo { get; set; }
        string OutCardNo { get; set; }

        string RetMessage { get; set; }
        int Ret { get; set; }

        个人基本信息 医保个人基本信息 { get; set; }
        Res获取参保人信息结果 获取参保人信息结果 { get; set; }
        Res交易确认结果 交易确认结果 { get; set; }
        门诊挂号预结算结果 门诊挂号预结算结果 { get; set; }
        门诊挂号预结算结果确认结果 门诊挂号预结算结果确认结果 { get; set; }
        预结算结果 门诊缴费预结算结果 { get; set; }
        门诊缴费预结算结果确认结果 门诊缴费预结算结果确认结果 { get; set; }

        Func<Result> PreSettleAction { get; set; }
        bool 诊间结算 { get; set; }

        /// <summary>
        ///     绑定的就诊卡号
        /// </summary>
        string BindingCardNo { get; set; }
    }

    public class SiModel : ModelBase, ISiModel
    {
        public bool NeedCreate { get; set; }
        public string SiPatientInfo { get; set; }
        public string CardHardInfo { get; set; }
        public string OutCardNo { get; set; }

        public string RetMessage { get; set; }
        public int Ret { get; set; }

        public 个人基本信息 医保个人基本信息 { get; set; }
        public Res获取参保人信息结果 获取参保人信息结果 { get; set; }
        public Res交易确认结果 交易确认结果 { get; set; }
        public 门诊挂号预结算结果 门诊挂号预结算结果 { get; set; }
        public 门诊挂号预结算结果确认结果 门诊挂号预结算结果确认结果 { get; set; }
        public 预结算结果 门诊缴费预结算结果 { get; set; }
        public 门诊缴费预结算结果确认结果 门诊缴费预结算结果确认结果 { get; set; }
        public Func<Result> PreSettleAction { get; set; }
        public bool 诊间结算 { get; set; }
        public string BindingCardNo { get; set; }
    }
}