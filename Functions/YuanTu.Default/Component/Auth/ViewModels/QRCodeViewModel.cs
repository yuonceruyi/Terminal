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
using DataHandlerEx = YuanTu.Consts.UserCenter.DataHandlerEx;

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class QrCodeViewModel : ViewModelBase
    {
        protected bool Exitloop; //表示已经退出轮询，保证轮询查到的数据是最新的
        protected bool Looping; //表示正在进行轮询，退出页面前必须保证已经退出轮询
        protected bool CancelPending; //需要进行取消
        private ScanDataVO 二维码;
        public override string Title => "扫码登录";

        [Dependency]
        public IQrCodeModel QrCode { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        public override void OnSet()
        {
            Hint = "请打开慧医客户端 \"扫一扫\" 选择就诊人";
            慧医扫码_Step1 = ResourceEngine.GetImageResourceUri("慧医扫码_Step1");
            慧医扫码_Step2 = ResourceEngine.GetImageResourceUri("慧医扫码_Step2");
            慧医扫码_二维码 = ResourceEngine.GetImageResourceUri("慧医扫码_二维码");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            QrCodeImage = null;
            二维码 = null;
            Scanned = false;
            DoCommand(p =>
            {
                p.ChangeText("正在获取登录二维码，请稍候...");
                var req = new req生成登录二维码()
                {
                    corpId = FrameworkConst.HospitalId,
                    deviceMac = NetworkManager.MAC
                };
                var rest = DataHandlerEx.生成登录二维码(req);
                if (!rest.success)
                {
                    ShowAlert(false, "温馨提示", $"获取登录二维码失败\r\n{rest.msg}");
                    Exitloop = true;
                    Preview(); //回退
                    return;
                }
                二维码 = rest.data;
                QrCode.Uuid = rest.data.uuid;
                Logger.Main.Info($"登录二维码生成成功 uuid:{二维码.uuid} 二维码:{二维码.url}");

                var barQrCodeGenerater = GetInstance<IBarQrCodeGenerator>();
                QrCodeImage = barQrCodeGenerater.QrcodeGenerate(二维码.url, Image.FromFile(ResourceEngine.GetResourceFullPath("慧医扫码_Logo")));

                Looping = true;
                CancelPending = true;
                Exitloop = false;
                Task.Factory.StartNew(AskingLoop); //创建成功则进行轮询
            }, false);
        }

        public override Task<bool> OnLeavingAsync(NavigationContext navigationContext)
        {
            if (!CancelPending)
                return Task.FromResult(true);
            return DoCommand(lp =>
            {
                lp.ChangeText("正在确认当前操作结果，请稍后...");
                return NeedLeaving(lp, navigationContext);
            }, false);
        }

        protected virtual bool NeedLeaving(LoadingProcesser lp, NavigationContext navigationContext)
        {
            Looping = false;
            var seed = 0;
            while (!Exitloop && !CancelPending) //确认已经退出循环
            {
                if (seed++ > 10) //重试超过10次，退出失败，则返回原状态
                {
                    //e.Cancel = true;
                    Looping = true;
                    return false;
                }
                Thread.Sleep(500);
            }
            if (CancelPending && 二维码 != null)
            {
                try
                {
                    var result = DataHandlerEx.取消扫码(new req取消扫码
                    {
                        uuid = 二维码.uuid
                    });
                    if (!result?.success ?? true)
                        Logger.Main.Error($"取消扫码失败 uuid:{二维码.uuid} 二维码:{二维码.url} 信息:{result?.msg}");
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"取消扫码异常 uuid:{二维码.uuid} 二维码:{二维码.url} 异常:{ex.Message}");
                }
            }

            二维码 = null;
            return true;
        }

        private void AskingLoop()
        {
            var req = new req根据uuid获取绑定就诊人信息
            {
                uuid = 二维码.uuid
            };
            while (Looping)
                try
                {
                    Thread.Sleep(1000);
                    var rest = DataHandlerEx.根据uuid获取绑定就诊人信息(req);
                    if (rest == null || !rest.success)
                    {
                        Logger.Main.Error(
                            $"uuid:{req.uuid} 二维码:{二维码?.url} 结果:{rest?.ToJsonString()}");
                        continue;
                    }

                    if (!Looping)
                        break;

                    //获取信息成功（100） app已扫码（508） app未扫码（509）
                    if (rest.resultCode == "508")
                    {
                        if(!Scanned)
                           Invoke(() => Scanned = true);
                        continue;
                    }
                    if (rest.resultCode == "509")
                        continue;
                    CancelPending = false;
                    if (rest.resultCode != "100")
                    {
                        ShowAlert(false, "获取绑定就诊人信息", rest.msg);
                        Preview();
                        break;
                    }
                    QrCode.Patients = rest.data;
                    if (rest.data == null || rest.data.Count == 0)
                    {
                        ShowAlert(false, "获取绑定就诊人信息", "未查询到就诊人的信息(列表为空)");
                    }
                    else if (rest.data.Count == 1)
                    {
                        QrCode.Patient = rest.data.First();
                        OnGetInfo();
                    }
                    else
                    {
                        // TODO 多就诊人选择
                        ShowAlert(false, "获取绑定就诊人信息", "暂不支持多就诊人选择");
                        Preview();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"uuid:{req.uuid}  二维码:{二维码?.url} 异常:{ex.Message}"
                    );
                }
            Exitloop = true;
        }

        protected virtual void OnGetInfo()
        {
            DoCommand(ctx =>
            {
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = QrCode.Patient.cardNo,
                    cardType = QrCode.Patient.cardType.ToString()
                };
                PatientModel.Res病人信息查询 = Consts.Gateway.DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        Preview();
                    }
                    else
                    {
                        CardModel.CardNo = QrCode.Patient.cardNo;
                        CardModel.CardType = (CardType) QrCode.Patient.cardType;
                        Next();
                    }
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    Preview();
                }
            });
        }

        #region DataBinding

        private string _hint;

        public string Hint
        {
            get { return _hint; } 
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private Image _qrCodeIamge;

        public Image QrCodeImage
        {
            get { return _qrCodeIamge; } 
            set
            {
                _qrCodeIamge = value;
                OnPropertyChanged();
            }
        }

        private Uri _慧医扫码_Step1;

        public Uri 慧医扫码_Step1
        {
            get { return _慧医扫码_Step1; }
            set
            {
                _慧医扫码_Step1 = value; 
                OnPropertyChanged();
            }
        }

        private Uri _慧医扫码_Step2;

        public Uri 慧医扫码_Step2
        {
            get { return _慧医扫码_Step2; }
            set
            {
                _慧医扫码_Step2 = value; 
                OnPropertyChanged();
            }
        }

        private Uri _慧医扫码_二维码;

        public Uri 慧医扫码_二维码
        {
            get { return _慧医扫码_二维码; }
            set
            {
                _慧医扫码_二维码 = value; 
                OnPropertyChanged();
            }
        }

        private bool _scanned;

        public bool Scanned
        {
            get { return _scanned; }
            set
            {
                _scanned = value; 
                OnPropertyChanged();
            }
        }

        #endregion DataBinding
    }
}