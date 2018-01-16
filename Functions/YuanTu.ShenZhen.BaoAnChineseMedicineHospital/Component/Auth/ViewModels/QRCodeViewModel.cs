using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.UserCenter.Entities;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Systems;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Tools;
using Prism.Commands;
using System.Windows.Input;
using System.Windows.Controls;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Auth.ViewModels
{
    public class QrCodeViewModel : ViewModelBase
    {
        public QrCodeViewModel()
        {
            TextChangedCommand = new DelegateCommand<TextBox>(Confirm);
        }

        public override string Title => "扫描带登记号的条码";

        public string Hint { get; set; } = "请按提示扫描带登记号的条码";

        public DelegateCommand<TextBox> TextChangedCommand { get; set; }


        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        private Uri _cardUri;
        public Uri CardUri
        {
            get { return _cardUri; }
            set
            {
                _cardUri = value;
                OnPropertyChanged();
            }
        }

        private Uri _backUri;
        public Uri BackUri
        {
            get { return _backUri; }
            set
            {
                _backUri = value;
                OnPropertyChanged();
            }
        }


        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("条码扫描");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_登记号");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {

        }

        public virtual void Confirm(TextBox txtBoxPatientId)
        {
            var sPatientId = txtBoxPatientId.Text;
            if (string.IsNullOrEmpty(sPatientId))
                return;

            if (sPatientId.EndsWith("\n"))
            {
                OnGetInfo(sPatientId.Replace("\n", "").Replace("\r", ""));
                txtBoxPatientId.Text = "";
            }
            return;
        }


        protected virtual void OnGetInfo(string sPatientId)
        {
            if (sPatientId.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "未扫描到正确的登记号");
                return;
            }
            DoCommand(ctx =>
            {
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = sPatientId,
                    cardType = "14",
                };
                PatientModel.Res病人信息查询 = Consts.Gateway.DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(PatientModel.当前病人信息.idNo))
                    {
                        ShowAlert(false, "病人信息查询", PatientModel.当前病人信息.name + "未登记身份证号进行实名认证");
                        Navigate(A.Home);
                        return;
                    }
                    CardModel.CardNo = PatientModel.当前病人信息.cardNo;
                    CardModel.CardType = (CardType)(Convert.ToInt32(PatientModel.当前病人信息.cardType));
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                }
            });
        }


        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试登记号");
        }
    }
}