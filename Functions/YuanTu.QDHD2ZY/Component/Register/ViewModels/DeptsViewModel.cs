using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;

namespace YuanTu.QDHD2ZY.Component.Register.ViewModels
{
    public class DeptsViewModel : QDKouQiangYY.Component.Register.ViewModels.DeptsViewModel
    {
        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            if (ChoiceModel.Business != Business.挂号)
            {
                base.Confirm(i);
                return;
            }
            var count = DeptartmentModel.所选科室.configList.Count(n => n.regMode == "2");
            if (count > 1)
            {
                Navigate(A.XC.Wether);
            }
            else if (count == 1)
            {
                var intRegType = int.Parse(DeptartmentModel.所选科室.configList.First(n => n.regMode == "2").regType);
                RegTypesModel.SelectRegType = new RegTypeDto {RegType = (RegType) intRegType};
                base.Confirm(i);
            }
            else
            {
                base.Confirm(i);
            }
        }
    }
}