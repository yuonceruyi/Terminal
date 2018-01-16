using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.XiaoShanZYY.Component.PrintAgain.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.PrintAgain.ViewModels
{
    public class ChoiceViewModel : ViewModelBase
    {
        public override string Title { get; }

        [Dependency]
        public IPrintAgainModel PrintAgain { get; set; }

        [Dependency]
        public IPrintAgainService PrintAgainService { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            Data = new ObservableCollection<Info>(
                PrintAgain.Res补打查询.BILLMX.Select(p => new InfoMore()
                {
                    Title = p.Jslx == "1" ? "挂号" : "结算",
                    SubTitle = p.Jsrq,
                    Amount = decimal.Parse(p.Zjje) * 100,
                    Tag = p,
                    ConfirmCommand = confirmCommand,
                }));
        }

        protected virtual void Confirm(Info i)
        {
            var item = i.Tag.As<billdetail2>();
            DoCommand(lp =>
            {
                ChangeNavigationContent(item.Jsrq);
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "补打成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分补打",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = GatherPrintables(item),
                    TipImage = "提示_凭条"
                });
                Navigate(A.JFBD.Print);
            });
        }

        protected Queue<IPrintable> GatherPrintables(billdetail2 res)
        {
            var queue = PrintManager.NewQueue(res.Tsxx);
            var builder = new StringBuilder();
            if (res.Jslx == "1")
            {
                builder.AppendLine("【挂号科室】" + res.Ghks);
                builder.AppendLine("【挂号医生】" + res.Ghys);
                builder.AppendLine("【 就诊号 】" + res.Jzxh);
                builder.AppendLine("【科室位置】" + res.Kswz);
            }
            else
            {
                builder.AppendLine("【取药窗口】" + res.Qyck);
            }

            builder.AppendLine("【总计金额】" + res.Zjje + " 元");
            builder.AppendLine("【医保报销】" + res.Ybbx + " 元");
            builder.AppendLine("【市民卡账户支付】" + res.Smkje + " 元");
            builder.AppendLine("【本年余额】" + res.Bnzhye + " 元");
            builder.AppendLine("【历年账户余额】" + res.Lnzhye + " 元");
            builder.AppendLine("【移动支付】" + res.Thirdcash + " 元");
            builder.AppendLine("【结算时间】" + res.Jsrq);
            builder.AppendLine("【发票号码】" + res.Fphm);
            builder.AppendLine(res.Bzxx);
            builder.AppendLine(".");
            builder.AppendLine(".");
            builder.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = builder.ToString() });
            return queue;
        }

        #region Binding

        private ObservableCollection<Info> _data;

        public ObservableCollection<Info> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}