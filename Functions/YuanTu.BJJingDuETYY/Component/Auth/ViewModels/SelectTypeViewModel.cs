using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Models;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class SelectTypeViewModel : YuanTu.Default.Component.Auth.ViewModels.SelectTypeViewModel
    {
        protected override void OnButtonClick(Info obj)
        {
            var choice = obj.Tag as CreateTypeModel;
            CreateModel.CreateType = choice.Value;
            Navigate(A.CK.IDCard);
        }
    }
}
