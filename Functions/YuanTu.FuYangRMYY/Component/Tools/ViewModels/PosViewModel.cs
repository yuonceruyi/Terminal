using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Services;

namespace YuanTu.FuYangRMYY.Component.Tools.ViewModels
{
    public class PosViewModel:YuanTu.Default.Clinic.Component.Tools.ViewModels.PosViewModel
    {
        public ICommand MediaEndedCommand
        {
            get
            {
                return new DelegateCommand<object>((sender) =>
                {
                    MediaElement media = (MediaElement)sender;
                    media.LoadedBehavior = MediaState.Manual;
                    media.Position = TimeSpan.FromMilliseconds(1);
                    media.Play();
                });
            }

        }
        private Uri _gifUrl;

        public Uri GifUrl
        {
            get { return _gifUrl; }
            set { _gifUrl = value;OnPropertyChanged(); }
        }

        public override void OnSet()
        {
            base.OnSet();

            GifUrl= ResourceEngine.GetImageResourceUri("动画_银行卡刷卡与插卡");
        }
    }
}
