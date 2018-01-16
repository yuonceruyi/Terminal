using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.FuYangRMYY.Component.SignIn.Models;
using YuanTu.FuYangRMYY.Services;

namespace YuanTu.FuYangRMYY.Component.SignIn.ViewModels
{
    public class QueueSelectViewModel : YuanTu.Default.Component.SignIn.ViewModels.QueueSelectViewModel
    {
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public ISignInModel SignInModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = SignInModel.ResponseOrders.Select(p => new InfoMore
            {
                Title = p.Department,
                SubTitle = p.Doctor,
                Type = "",
                Amount = null,
               // Extends = $"剩余号源 {p.restnum}",
                ConfirmCommand = confirmCommand,
                Tag = p,
             //   IsEnabled = p.restnum != "0",
              //  DisableText = p.restnum == "0" ? "已满" : ""
            }).ToList();
            Data = new ObservableCollection<Info>(list);
        }

        protected override void Confirm(Info i)
        {
            var order = i.Tag as ResponseOrder;
            DoCommand(lp =>
            {
                lp.ChangeText("正在签到，请稍后....");
                var resp = HisService.StartSignIn(PatientModel.当前病人信息, order);
                if (resp.IsSuccess)
                {
                    lp.ChangeText("正在打印凭条，请稍后....");
                    StartPrint(resp.Value);
                    ShowAlert(true, "签到结果", "签到成功！", extend: new AlertExModel()
                    {
                        HideCallback = o =>
                        {
                            Navigate(A.Home);
                        }
                    });
                }
                else
                {
                    ShowAlert(false, "签到结果", "签到失败！", extend: new AlertExModel()
                    {
                        HideCallback = o =>
                        {
                            if (o==AlertHideType.TimeOut)
                            {
                                Navigate(A.Home);
                            }
                        }
                    },debugInfo:resp.Message);
                }

               
            });
        }

        private void StartPrint(SignInfoResponse response)
        {
            var boldFont =  new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14),
                FontStyle.Bold);
            var queue = PrintManager.NewQueue("门诊报道凭证");
            var patient = PatientModel.当前病人信息;
            var sb = new StringBuilder();
            sb.Append($"姓    名：{patient.name}\n");
            sb.Append($"登 记 号：{patient.patientId}\n");
            sb.Append($"科    别：{response.RegDep}\n");
            sb.Append($"类    别：{response.SessionType}\n");
            queue.Enqueue(new PrintItemText(){Text = sb.ToString()});
            sb.Clear();
            sb.Append($"号    别：\n");
            sb.Append($"就诊日期：{response.AdmDate:yyyy-MM-dd} {response.RegTime:HH:mm:ss}\n");
            sb.Append($"就 诊 号：{response.SeqNo}\n");
            queue.Enqueue(new PrintItemText() { Text = sb.ToString(),Font = boldFont });
            sb.Clear();
            sb.Append($"报到日期：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"操 作 员：{FrameworkConst.OperatorId}\n\n");
            queue.Enqueue(new PrintItemText() { Text = sb.ToString() });

            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
    }
}
