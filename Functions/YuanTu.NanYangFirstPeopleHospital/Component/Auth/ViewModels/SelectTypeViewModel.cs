using YuanTu.Consts;
using YuanTu.Consts.Models;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class SelectTypeViewModel : Default.Component.Auth.ViewModels.SelectTypeViewModel
    {
        protected override void OnButtonClick(Info obj)
        {
            var choice = obj.Tag as CreateTypeModel;
            if (choice != null)
                CreateModel.CreateType = choice.Value;
            Navigate(A.CK.IDCard);
        }
    }
}