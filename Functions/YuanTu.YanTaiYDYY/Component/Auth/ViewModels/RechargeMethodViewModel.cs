using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.AudioPlayer;

namespace YuanTu.YanTaiYDYY.Component.Auth.ViewModels
{
    public class RechargeMethodViewModel : ViewModelBase
    {
        [Dependency]
        public IOpRechargeModel OpRechargeModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        public override string Title => "选择充值方式";

        #region Overrides of ViewModelBase

        protected bool DoEnter;

        /// <summary>
        ///     仅当在实例化试调用
        /// </summary>
        public override void OnSet()
        {
            var payButtonCmd = new DelegateCommand<Info>(OnPayButtonClick);
            var resource = GetInstance<IResourceEngine>();
            var config = GetInstance<IConfigurationManager>();
            var bts = new List<RechargeMethodModel>();
            var k = Enum.GetValues(typeof(PayMethod));
            foreach (PayMethod rechargeMethod in k)
            {
                var spex = $"Recharge:{rechargeMethod}";
                var v = config.GetValue($"{spex}:Visabled");
                if (v == "1")
                {
                    var color = config.GetValue($"{spex}:Color").Split(',');
                    bts.Add(new RechargeMethodModel
                    {
                        RechargeMethod = rechargeMethod,
                        Name = config.GetValue($"{spex}:Name") ?? "未定义",
                        Order = config.GetValueInt($"{spex}:Order"),
                        ImageSource = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName")),
                        Color = Color.FromRgb(byte.Parse(color[0]), byte.Parse(color[1]), byte.Parse(color[2]))
                    });
                }
            }
            var list = bts.OrderBy(p => p.Order).Select(p => new InfoIcon
            {
                Title = p.Name,
                ConfirmCommand = payButtonCmd,
                IconUri = p.ImageSource,
                Tag = p,
                Color = p.Color
            });

            Data = new ObservableCollection<InfoIcon>(list);

            var res = GetInstance<IResourceEngine>();
            var sound = GetInstance<IAudioPlayer[]>().FirstOrDefault();
            sound?.StartPlayAsync(res.GetResourceFullPath(SoundMapping.选择充值方式));
        }

        #endregion Overrides of ViewModelBase

        public override void OnEntered(NavigationContext navigationContext)
        {
            DoEnter = false;
        }

        protected virtual void OnPayButtonClick(Info i)
        {
            if (DoEnter)
                return;
            DoEnter = !DoEnter;

            var button = (RechargeMethodModel)i.Tag;
            OpRechargeModel.RechargeMethod = button.RechargeMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.充值;
            ExtraPaymentModel.CurrentPayMethod = button.RechargeMethod;
            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
            //准备门诊充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = IdCardModel.Name,
                PatientId = null,
                IdNo = IdCardModel.IdCardNo,
                GuardianNo = null,
                CardNo = CardModel.CardNo,
                Remain = 0
            };

            ChangeNavigationContent(OpRechargeModel.RechargeMethod.ToString());
            switch (button.RechargeMethod)
            {
                case PayMethod.未知:
                case PayMethod.预缴金:
                case PayMethod.医保:
                    throw new ArgumentOutOfRangeException();
                case PayMethod.现金:
                    OnCAClick();
                    break;

                case PayMethod.银联:
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                case PayMethod.苹果支付:
                    Navigate(InnerA.JD.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Task<Result> OnRechargeCallback()
        {
            return Task.Run(PaymentModel.ConfirmAction);
        }

        protected virtual void OnCAClick()
        {
            Navigate(A.Third.Cash);
        }

        protected virtual Queue<IPrintable> OpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("门诊充值");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：充值{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{OpRechargeModel.Req预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"充值后余额：{OpRechargeModel.Res预缴金充值.data.cash.In元()}\n");
                sb.Append($"收据号：{OpRechargeModel.Res预缴金充值.data.orderNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{OpRechargeModel.Res预缴金充值.msg}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

        public class RechargeMethodModel
        {
            public PayMethod RechargeMethod { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
            public Uri ImageSource { get; set; }

            public bool Visable { get; set; }
            public bool Disabled { get; set; }
            public Color Color { get; set; }
        }

        #region Binding

        private ObservableCollection<InfoIcon> _data;

        public ObservableCollection<InfoIcon> Data
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