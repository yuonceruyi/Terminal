using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.YanTaiArea.Card;

namespace YuanTu.YanTaiYDYY.Component.Auth.Dialog.ViewModels
{
    public class UpdatePwd1ViewModel: ViewModelBase
    {
        public UpdatePwd1ViewModel()
        {        
        }

        public override string Title => "输入卡密码";
    }
}
