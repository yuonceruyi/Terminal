using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.JiaShanHospital.Component.InfoQuery.ViewModels
{
    public class InputViewModel :YuanTu.Default.Component.InfoQuery.ViewModels.InputViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            HideNavigating = true;
            PinCode = null;
            QueryType = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询 ? "药品首字母：" : "诊疗项首字母：";
            Hint = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询 ? "药品查询[价格仅供参考,价格以实际发票为准]" : "诊疗项查询";
            QueryTips = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询
                ? "请输入您要查询的药品信息的首字母"
                : "请在下方输入诊疗项首字母进行查询";
        }
    }
}