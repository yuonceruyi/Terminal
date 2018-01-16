using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;

namespace YuanTu.YiWuZYY.Component.InfoQuery.ViewModels
{
    public class QueryChoiceViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.QueryChoiceViewModel
    {
        protected override void OnButtonClick(InfoQueryButtonInfo obj)
        {
            var engine = NavigationEngine;
            var choiceModel = GetInstance<IChoiceModel>();
            var queryChoiceModel = GetInstance<IQueryChoiceModel>();
            choiceModel.HasAuthFlow = true;
            //手动清空导航栏
            GetInstance<INavigationModel>().Items.Clear();
            queryChoiceModel.InfoQueryType = obj.InfoQueryType;
            switch (obj.InfoQueryType)
            {
                case InfoQueryTypeEnum.药品查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.MedicineQuery, A.YP.Query), obj.Name);
                    break;

                case InfoQueryTypeEnum.项目查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.ChargeItemsQuery, A.XM.Query), obj.Name);
                    break;

                case InfoQueryTypeEnum.已缴费明细:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.PayCostQuery, A.JFJL.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.住院一日清单:
                    //choiceModel.HasAuthFlow = false;
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), obj.Name);
                    break;

                case InfoQueryTypeEnum.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        LisPrintJump,
                        new FormContext(A.DiagReportQuery, AInner.JYJL.Print), obj.Name);
                    break;

                case InfoQueryTypeEnum.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), obj.Name);
                    break;

                default:
                    ShowAlert(false, "友好提示", "业务未实现");
                    break;
            }
        }


        public Task<Result<FormContext>> LisPrintJump()
        {
            return DoCommand(lp =>
            {
                var config = GetInstance<IConfigurationManager>();
                var patientInfo = GetInstance<IPatientModel>();
                var path = config.GetValue("Lis:Path");
                if (File.Exists(path))
                {
                    var wd = Path.GetDirectoryName(path);
                    var fname = Path.GetFileName(path);
                    Process.Start(new ProcessStartInfo()
                    {
                        WorkingDirectory = wd,
                        FileName = fname,
                        Arguments = patientInfo?.当前病人信息?.cardNo??""
                    });
                    var printModel = GetInstance<IPrintModel>();
                    printModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "查询成功",
                  
                    });
                    //Navigate(A.JF.Print);
                    return Result<FormContext>.Success(null);
                }
                ShowAlert(false,"LIS配置异常","当前自助终端没有配置报告打印系统，请联系工作人员处理！");
                return Result<FormContext>.Fail(null);
            });
          
        }
    }
}
