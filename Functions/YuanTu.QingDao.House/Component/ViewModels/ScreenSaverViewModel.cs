using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Services;

namespace YuanTu.QingDao.House.Component.ViewModels
{
    public class ScreenSaverViewModel:Default.House.Component.ViewModels.ScreenSaverViewModel
    {
        public override void OnSet()
        {
            HideNavigating = true;
            TimeOut = 0;
            屏保二维码2Uri = ResourceEngine.GetImageResourceUri("屏保健康青岛二维码_House");
            屏保二维码1Uri = ResourceEngine.GetImageResourceUri("屏保慧医二维码_House");
            屏保手机Uri = ResourceEngine.GetImageResourceUri("屏保手机_House");
        }
        public override string 屏保文本1 { get; set; } = "下载慧医APP\n手机上查看您的家庭健康数据";
        public override string 屏保文本3 { get; set; } = "关注「健康青岛」\n     官方微信号";
    }
}
