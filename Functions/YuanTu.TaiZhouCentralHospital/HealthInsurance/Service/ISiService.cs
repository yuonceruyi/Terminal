using YuanTu.Consts.FrameworkBase;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Request;

namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Service
{
    public interface ISiService:IService
    {
        /// <summary>
        /// 交易初始化
        /// </summary>
        /// <returns></returns>
        Result Initialize();
        /// <summary>
        /// 操作员签到
        /// </summary>
        /// <returns></returns>
        Result OperatorSignIn(int transNo, string req);
        /// <summary>
        /// 获取参保人信息
        /// </summary>
        /// <returns></returns>
        Result GetSiPatientInfo(Req获取参保人信息 reqSiPatientInfo);

      
        /// <summary>
        /// 门诊缴费预结算
        /// </summary>
        /// <returns></returns>
        Result OpPreSettle(string req);
        /// <summary>
        /// 门诊缴费结算
        /// </summary>
        /// <returns></returns>
        Result OpSettle(string req);
      
        /// <summary>
        /// 门诊缴费退费
        /// </summary>
        /// <returns></returns>
        Result OpRefund(Req医保退费 req);
        /// <summary>
        /// 查询交易结果
        /// </summary>
        /// <returns></returns>
        Result QueryTradeResult(Req交易结果查询 req);
        /// <summary>
        /// 交易确认
        /// </summary>
        /// <returns></returns>
        Result TradeConfirm(Req交易确认 req);

        /// <summary>
        /// 关闭（退出系统前释放资源）
        /// </summary>
        /// <returns></returns>
        Result Close();
     
    
      

      

       

    }
}
