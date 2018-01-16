using System;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Response;

namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Model
{
    public interface ISiModel : IModel
    {
        HiInfo HiInfo { get; set; }
        bool NeedCreate { get; set; }
        string SiPatientInfo { get; set; }

        string RetMessage { get; set; }
        int Ret { get; set; }

        个人基本信息 医保个人基本信息 { get; set; }
        Res获取参保人信息结果 获取参保人信息结果 { get; set; }
        Res交易确认结果 交易确认结果 { get; set; }
        预约挂号预处理结果 门诊挂号预处理结果 { get; set; }

        string 医保预结算结果字符串 { get; set; }
        string 医保结算结果字符串 { get; set; }
        计算结果信息 医保预结算结果 { get; set; }
        预结算结果 门诊缴费预结算结果 { get; set; }

        string 医保结算流水号 { get; set; }

        Func<Result> PreSettleAction { get; set; }

        OutData 健康卡信息 { get; set; }
    }

    public class SiModel : ModelBase, ISiModel
    {
        public bool NeedCreate { get; set; }
        public string SiPatientInfo { get; set; }

        public string RetMessage { get; set; }
        public int Ret { get; set; }

        public 个人基本信息 医保个人基本信息 { get; set; }
        public Res获取参保人信息结果 获取参保人信息结果 { get; set; }
        public Res交易确认结果 交易确认结果 { get; set; }
    

        public 预结算结果 门诊缴费预结算结果 { get; set; }

        public Func<Result> PreSettleAction { get; set; }

        public HiInfo HiInfo { get; set; }
        public 计算结果信息 医保预结算结果 { get; set; }
        public string 医保结算流水号 { get; set; }
        public string 医保预结算结果字符串 { get; set; }
        public string 医保结算结果字符串 { get; set; }
        public OutData 健康卡信息 { get; set; }
        public 预约挂号预处理结果 门诊挂号预处理结果 { get; set; }
    }
}