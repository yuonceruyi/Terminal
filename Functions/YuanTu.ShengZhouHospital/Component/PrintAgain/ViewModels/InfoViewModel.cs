using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.PrintAgain;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Services.ConfigService;
using YuanTu.Core.Services.PrintService;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShengZhouHospital.Component.Auth.Models;
using YuanTu.ShengZhouHospital.Component.PrintAgain.Common;

namespace YuanTu.ShengZhouHospital.Component.PrintAgain.ViewModels
{
    public class InfoViewModel:ViewModelBase
    {

        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override string Title => "请触摸下方卡片选择挂号科室";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var printAgainChoiceModel = GetInstance<IPrintAgainModel>();
            var list = printAgainChoiceModel.Slips.Select(p => new Info
            {
                Title = p.gmtCreate,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list);
        }

        protected virtual void Confirm(Info i)
        {
            var data = i.Tag.As<凭条记录>();
            var queue = SilpHandler.DeserializeObject(data.content).Value;
            var res=AddImagePrintable(queue);
            if (!res)
            {
                ShowAlert(false,"凭条补打","条形码构造失败");
                return;
            }
            PrintModel.SetPrintInfo(true, new PrintInfo
            {
                TypeMsg = "凭条补打",
                TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + "补打",
                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                Printables = queue,
                TipImage = "提示_凭条"
            });
            Navigate(AInner.PTBD.Print);
        }

        private Result<Queue<IPrintable>> AddImagePrintable(Queue<IPrintable> queue)
        {
            var sb = new StringBuilder();
            var image = BarCode128.GetCodeImage(PatientModel.当前病人信息.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 2f,
                Width = image.Width / 2f
            });
            sb.Append(PatientModel.当前病人信息.patientId);
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 9, System.Drawing.FontStyle.Regular) });
            return Result<Queue<IPrintable>>.Success(queue);
        }


        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };

        #region Binding

        private ObservableCollection<Info> _data;

        public ObservableCollection<Info> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}
