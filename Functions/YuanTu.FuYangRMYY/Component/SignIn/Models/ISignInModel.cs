using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.FuYangRMYY.Component.SignIn.Models
{
    public interface ISignInModel:IModel
    {
        ResponseOrder[] ResponseOrders { get; set; }
    }

    public class SignInModel : ModelBase, ISignInModel
    {
        public ResponseOrder[] ResponseOrders { get; set; }
    }
}
