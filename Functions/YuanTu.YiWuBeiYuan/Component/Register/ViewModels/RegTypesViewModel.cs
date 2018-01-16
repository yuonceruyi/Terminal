using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Models;

namespace YuanTu.YiWuBeiYuan.Component.Register.ViewModels
{
    public class RegTypesViewModel:YuanTu.Default.Component.Register.ViewModels.RegTypesViewModel
    {
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;
            RegTypesModel.SelectRegTypeName = i.Title;
            ChangeNavigationContent(i.Title);
            Next();
           
        }
    }
}
