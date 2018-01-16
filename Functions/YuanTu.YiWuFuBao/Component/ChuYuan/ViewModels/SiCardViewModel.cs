using System.Windows;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Navigating;
using YuanTu.Devices.CardReader;
using YuanTu.YiWuArea.Insurance.Models;
using YuanTu.YiWuArea.Insurance.Models.Base;

namespace YuanTu.YiWuFuBao.Component.ChuYuan.ViewModels
{
    public class SiCardViewModel : YuanTu.YiWuFuBao.Component.Auth.ViewModels.CardViewModel
    {
        public SiCardViewModel(IRFCpuCardReader[] rfCpuCardReader) : base(rfCpuCardReader)
        {
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            ShowSiCardAnimation = Visibility.Visible;
            ShowBarCodeCardAnimation = Visibility.Collapsed;
            HospitalInputFocus = false;
        }

        protected override void ConfirmHospitalCardNo()
        {
            //不进行任何处理
        }

        protected override void StartReadInsuranceCard(string siCardNo)
        {
            DoCommand(ctx =>
            {
                //cardInReader = true;//表示读卡器有卡，此时应该禁止扫码输入了
                //CardModel.CardNo = siCardNo;
                //var choiceModel = GetInstance<IChoiceModel>();
                //if (choiceModel.Business == Business.查询)
                //{
                //    var queryChoiceModel = GetInstance<IQueryChoiceModel>();
                //    if (queryChoiceModel.InfoQueryType == InfoQueryTypeEnum.检验结果)
                //    {
                //        Task.Run(() =>
                //        {
                //            Task.Delay(1 * 1000);
                //            NavigationEngine.Next(new FormContext(A.ChaKa_Context, A.CK.Info));
                //        });
                //        return true;
                //    }
                //}
                //var cm = (CardModel as CardModel);
                //cm.SiCardUseSiNetWork = true;
                //CardModel.CardType = CardType.社保卡;
                ctx.ChangeText("正在读卡，请稍后...");

                var ybInfoRest = GetInfoFromSi();//
                if (!ybInfoRest.IsSuccess || !ybInfoRest.Value.IsSuccess)
                {
                    //$$-400~对不起,该人员处于人员黑名单冻结状态,不能在本地医疗机构使用IC卡消费!%%Sim_Operation.F_Orap22%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~此卡状态不正常,此卡状态为挂失%%SIM_TRANSPACK.F_OrafGetPsseno%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~编码为11388940参保人待遇享受时间信息不存在或还未到待遇享受开始时间%%SIM_MEDPUBLIC.P_GetQualification%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~人员缴费状态不正常%%Sim_Operation.F_Orap22%%ORA-0000: normal, successful completion~~~$$
                    //$$-400~找不到该卡号(W33724743)的有关信息%%SIM_TRANSPACK.F_OrafGetPsseno%%ORA-01403: no data found~~~$$
                    if (ybInfoRest.Message.Contains("卡上电复位失败"))
                    {
                        ShowAlert(false, "社保读卡", "您的社保卡没有正确插入，请按图片提示插入社保卡。");
                        return false;
                    }
                    if (ybInfoRest.Message.Contains("此卡状态为挂失"))
                    {
                        ShowAlert(false, "社保读卡", "您的社保卡处于挂失状态");
                        return false;
                    }
                    if (ybInfoRest.Message.Contains("找不到该卡号"))
                    {
                        ShowAlert(false, "社保读卡", "暂时不支持您的社保卡，请到窗口办理业务。");
                        return false;
                    }


                    var errorMsg = "由于社保网络原因，系统将不能使用社保支付";
                    if (ybInfoRest.Message.Contains("黑名单"))
                    {
                        errorMsg = $"由于您处于社保黑名单冻结状态，系统将不能使用社保支付";
                    }
                    else if (ybInfoRest.Message.Contains("人员缴费状态不正常"))
                    {
                        errorMsg = $"您的社保卡状态不正常，系统将不能使用社保支付";
                    }
                    else if (ybInfoRest.Message.Contains("待遇享受时间信息不存在或还未到待遇享受开始时间"))
                    {
                        errorMsg = $"由于您待遇享受时间信息不存在或还未到待遇享受开始时间，系统将不能使用社保支付";
                    }
                    //ShowConfirm("社保操作失败", errorMsg, cbk =>
                    //{
                    //    Navigate(A.Home);
                    //}, 30);
                    ShowAlert(false, "社保读卡", errorMsg,extend:new AlertExModel(){HideCallback = tp =>
                    {
                        Navigate(A.Home);
                    }});
                    return true;
                    //return false;//此处会触发重试
                }

                return ProcessWithSiData(ctx, ybInfoRest.Value);


            }).ContinueWith(ret =>
            {
                if (!ret.Result) //失败了，重来
                {
                    if (NavigationEngine.State == A.CK.Card)//确定还在当前页面
                    {
                        StartRead();
                    }
                }
            });
        }

        protected override bool ProcessWithSiData(LoadingProcesser ctx, Res获取参保人员信息 res)
        {
            var ptInfoRest = PatientIcData.Deserialize(res.写卡后IC卡数据);
            if (!ptInfoRest)
            {
                ShowAlert(false, "社保读卡失败", ptInfoRest.Message);
                return false;
            }
            if (ptInfoRest.Value.姓名!=PatientModel.住院患者信息.name)
            {
                ShowAlert(false, "身份信息不匹配",$"社保卡姓名为【{ptInfoRest.Value.姓名}】,与住院登记档案中的姓名【{PatientModel.住院患者信息.name}】不一致，不允许结算！");
                return false;
            }
            Next();
            return true;
        }
    }
}
