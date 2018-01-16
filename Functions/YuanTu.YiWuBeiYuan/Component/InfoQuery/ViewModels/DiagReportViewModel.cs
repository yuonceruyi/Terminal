using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.YiWuBeiYuan.Component.InfoQuery.ViewModels.SubViews;

namespace YuanTu.YiWuBeiYuan.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel: YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        //public ICardModel CardModel { get; set; }
        protected override void Confirm()
        {
            DiagReportModel.所选检验信息 = SelectData.Tag.As<检验基本信息>();
            var rep = DiagReportModel.所选检验信息;
            (new 检验报告单()).PrintData(CardModel,PatientModel,rep);

        }
    }
}
