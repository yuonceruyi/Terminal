using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using Microsoft.Practices.Unity;
using System.Windows;
using System.Windows.Documents;
using YuanTu.Core.Extension;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.UserControls;
using YuanTu.QDQLYY.Current.Models;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.PrintService;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Prism.Commands;
using System.IO;
using YuanTu.Core.FrameworkBase.Loadings;
using Microsoft.Practices.ServiceLocation;
using System.Windows.Threading;
using YuanTu.Consts.FrameworkBase;

//未用
namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public class InAllDetailViewModel : ViewModelBase
    {
        #region binding
        public static int PageSize = 56;
        public static int maxPrintTimes = 1;
        private ListDataGrid.PageDataEx _listData;
        private string _hint = "出院结算清单";
        public override string Title => "出院结算清单";
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public IInAllDetailModel inAllDetailModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        public ICommand PrintCommand
        {
            get
            {
                return new DelegateCommand<string>(printConfirm);
            }
        }
        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }


        public ListDataGrid.PageDataEx ListData
        {
            get { return _listData; }
            set
            {
                _listData = value;
                OnPropertyChanged();
            }
        }

        protected virtual string Caption
        {
            get { return FrameworkConst.HospitalName + "住院费用明细"; }
        }
        #endregion

        public InAllDetailViewModel()
        {
            //PrintCommand = new DelegateCommand<string>(printConfirm);
        }
        protected string Tips2 = "";
        public override void OnSet()
        {
            base.OnSet();
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            maxPrintTimes = config.GetValueInt("maxPrintTimes", 1);
            HideNavigating = true;
            TimeOut = 120;

            GetInAllDetailCount();
            //FillData();
        }
        #region 查询数据
        /// <summary>
        /// 获取结算次数，绑定
        /// </summary>
        private void GetInAllDetailCount()
        {
            var inAllDetailModel = GetInstance<IInAllDetailModel>();
            switch (CardModel.CardType)
            {
                case Consts.Enums.CardType.住院号:
                    inAllDetailModel.Req出院结算明细次数 = new req出院结算明细次数
                    {
                        patientHosId = PatientModel.住院患者信息.patientHosId,
                    };
                    break;
                case Consts.Enums.CardType.社保卡:
                case Consts.Enums.CardType.身份证:
                    inAllDetailModel.Req出院结算明细次数 = new req出院结算明细次数
                    {
                        cardType = PatientModel.Req住院患者信息查询.cardType,
                        cardNo = PatientModel.Req住院患者信息查询.cardNo,
                    };
                    break;
                default:
                    inAllDetailModel.Req出院结算明细次数 = new req出院结算明细次数
                    {
                        patientHosId = PatientModel.住院患者信息.patientHosId,
                        cardType = PatientModel.Req住院患者信息查询.cardType,
                        cardNo = PatientModel.Req住院患者信息查询.cardNo,
                    };
                    break;
            }

            inAllDetailModel.Res出院结算明细次数 = DataHandlerEx.出院结算明细次数(inAllDetailModel.Req出院结算明细次数);
            if (inAllDetailModel.Res出院结算明细次数?.success ?? false && inAllDetailModel.Res出院结算明细次数.data.Count > 0)
            {
                var info = PatientModel.住院患者信息;
                ListData = new ListDataGrid.PageDataEx
                {
                    CatalogContent = "",
                    List = inAllDetailModel.Res出院结算明细次数.data.Select(p => new
                    {
                        deptName = info.deptName,
                        startDate = p.startDate,
                        billDate = p.billDate,
                        printTimes = p.printTimes,
                        receiptNo = p.receiptNo,
                        CanPrint = int.Parse(p.printTimes) < maxPrintTimes,
                    }),
                    Tag = ""
                };
                var 出院结算次数 = inAllDetailModel.Res出院结算明细次数.data[0];
                if (出院结算次数.isBalanceRecord != "0")
                {
                    string msg = "您尚有未结算的费用，请确认结算时间段后再打印";
                    ShowAlert(false, "住院患者信息查询", msg, 30);
                }
            }
            else
            {
                ShowAlert(false, "住院患者信息查询", "查询出院结算记录错误" + inAllDetailModel.Res出院结算明细次数.msg, 5);
            }
            OnPropertyChanged();
        }
        /// <summary>
        /// 点击打印
        /// </summary>
        protected void printConfirm(string receiptNo)
        {
            ShowAlert(true, "正在打印", "正在打印，请稍候。\r\n请在打印完成后取走您的清单", 1);
            DoCommand(lp => {
                lp.ChangeText("正在打印，请稍候...");
                Invoke(DispatcherPriority.ContextIdle, () => { PrintConfirm(receiptNo); });
                return Result<string>.Success("");
            }).ContinueWith(
                t => {
                    GetInAllDetailCount();
                    ShowAlert(true, "正在打印", "正在打印，请稍候。\r\n请在打印完成后取走您的清单", 15);
                });
        }
        /// <summary>
        /// 执行打印
        /// </summary>
        /// <param name="receiptNo"></param>
        public void PrintConfirm(string receiptNo)
        {
            FillData(receiptNo);
            //ShowConfirm
            var contentRows = this.View.FindName("details") as TableRowGroup;
            var doc = this.View.FindName("doc") as FlowDocument;
            var pi = this.View.FindName("pageIndex") as Run;
            List<TableRow> AllRows = contentRows.Rows.ToList();
            contentRows.Rows.Clear();
            int doubleRow = 0;
            int pageIndex = 1;
            for (int i = 0; i < AllRows.Count; i++)
            {
                contentRows.Rows.Add(AllRows[i]);
                doubleRow += int.Parse(AllRows[i].Tag.ToString()) - 1;
                if (contentRows.Rows.Count + doubleRow >= PageSize)
                {
                    pi.Text = "第" + pageIndex.ToString() + "页";
                    Print();
                    doubleRow = 0;
                    contentRows.Rows.Clear();
                    pageIndex++;
                }
            }
            pi.Text = "第" + pageIndex.ToString() + "页";
            Print();
            var Req出院结算明细打印 = new req出院结算明细打印
            {
                patientHosId = PatientModel.住院患者信息.patientHosId,
                receiptNo = inAllDetailModel.Res出院结算明细次数.data[0].receiptNo,
            };
            var Res出院结算明细打印 = DataHandlerEx.出院结算明细打印(Req出院结算明细打印);
        }

        /// <summary>
        /// 填写数据
        /// </summary>
        /// <param name="receiptNo"></param>
        /// <returns></returns>
        private bool FillData(string receiptNo)
        {
            //查询
            //普通也用医保格式。20170922医院提出
            string InPatientType = "医保";
            inAllDetailModel.Req出院结算明细查询 = new req出院结算明细查询
            {
                patientHosId = PatientModel.住院患者信息.patientHosId,
                receiptNo = receiptNo,
            };
            inAllDetailModel.Res出院结算明细查询 = DataHandlerEx.出院结算明细查询(inAllDetailModel.Req出院结算明细查询);
            if (inAllDetailModel.Res出院结算明细查询?.success ?? false)
            {
                if (inAllDetailModel.Res出院结算明细查询.data[0].billItem.Count > 0)
                {
                    //continue
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "此出院结算记录没有明细，不需要打印", 5);
                    return false;
                }
            }
            else
            {
                ShowAlert(false, "住院患者信息查询", "查询出院结算记录错误" + inAllDetailModel.Res出院结算明细次数.msg, 5);
                return false;
            }

            var items = inAllDetailModel.Res出院结算明细查询.data[0].billItem;
            var info = PatientModel.住院患者信息;
            //测试分页
            //while (items.Count < 56 && info.patientHosId.EndsWith( "22396"))
            //{
            //    items.AddRange(items);
            //}
            /////////////////
            var doc = this.View.FindName("doc") as FlowDocument;
            var resources = doc.Resources;
            #region 排序
            items.Sort((x, y) =>
            {
                if (x.itemsTypeName.CompareTo(y.itemsTypeName) == 0)
                    return x.itemName.CompareTo(y.itemName);
                else
                    return x.itemsTypeName.CompareTo(y.itemsTypeName);
            });
            #endregion
            #region 填写
            resources["医院名称"] = Caption;
            string firstRow = $"姓名:{info.name} 性别:{info.sex} 住院号:{info.patientHosId} 科室:{info.deptName} 入出院日期:{inAllDetailModel.Res出院结算明细查询.data[0].startDate}-{inAllDetailModel.Res出院结算明细查询.data[0].billDate}";
            resources["首行"] = firstRow;
            var contentBody = this.View.FindName("header") as TableRowGroup;
            if (contentBody == null)
                return false;
            contentBody.Rows.Clear();
            if (InPatientType == "医保")
            {
                contentBody.Rows.Add(GetNewRow(new string[] { "项目名称:", "规格 ", "单位", "单价", "数量", "全额统筹", "自负比例", "部分统筹", "部分自负", "全额自费", "金额" }));
            }
            else if (InPatientType == "普通")
            {
                contentBody.Rows.Add(GetNewRow(new string[] { "项目名称:", "规格 ", "单位", "单价", "数量", "金额" }));
            }
            //var headerRows = this.View.FindName("header") as TableRowGroup;
            //InsertTableHead(headerRows);
            return FillDetail(items, InPatientType);
            #endregion

        }
        /// <summary>
        /// 填写明细
        /// </summary>
        /// <param name="items"></param>
        /// <param name="InPatientType"></param>
        /// <returns></returns>
        private bool FillDetail(List<出院结算项目> items, string InPatientType)
        {
            var contentRows = this.View.FindName("details") as TableRowGroup;
            if (contentRows == null)
                return false;
            contentRows.Rows.Clear();
            var sum = new decimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 0m };//合计
            string ITN_l = null;//当前类别名
            var ITS_l = new decimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 0m };//当前类别小计
            string rowtag = "1";
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.itemsTypeName != ITN_l)
                {//新类别
                    if (ITN_l != null || ITS_l[6] > 0)
                    {//上一类别有数据

                        if (InPatientType == "医保")
                        {
                            contentRows.Rows.Add(GetNewRow(new string[]{"小计:","","","",ITS_l[0].ToString(),ITS_l[1].ToString(),ITS_l[2].ToString(),ITS_l[3].ToString(),ITS_l[4].ToString(),
                            ITS_l[5].ToString(),ITS_l[6].ToString() }, TextAlignment.Left));
                        }
                        else if (InPatientType == "普通")
                        {
                            contentRows.Rows.Add(GetNewRow(new string[] { "小计:", "", "", "", ITS_l[0].ToString(), ITS_l[6].ToString() }, TextAlignment.Left));
                        }
                    }
                    ITS_l = new decimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 0m };
                    ITN_l = item.itemsTypeName;
                    if (InPatientType == "医保")
                    {
                        contentRows.Rows.Add(GetNewRow(new string[] { ITN_l }, TextAlignment.Left, 4, 11));
                    }
                    else if (InPatientType == "普通")
                    {
                        contentRows.Rows.Add(GetNewRow(new string[] { ITN_l }, TextAlignment.Left, 4, 6));
                    }
                }
                if (InPatientType == "医保")
                {
                    rowtag = Math.Max(item.itemName.StringLength() / 32 + 1, item.itemSpecs.StringLength() / 8 + 1).ToString();
                    contentRows.Rows.Add(GetNewRow(new string[]{item.itemName,item.itemSpecs, item.itemUnits, item.itemPrice.InRMB(), item.itemQty
                    , item.allCommonMoney.InRMB(), item.ratio, item.commonMoney.InRMB(), item.selfMoney.InRMB(), item.allSelfMoney.InRMB(), item.cost.InRMB() }, rowTag: rowtag));
                }
                else if (InPatientType == "普通")
                {
                    rowtag = Math.Max(item.itemName.StringLength() / 50 + 1, item.itemSpecs.StringLength() / 12 + 1).ToString();
                    contentRows.Rows.Add(GetNewRow(new string[] { item.itemName, item.itemSpecs, item.itemUnits, item.itemPrice.InRMB(), item.itemQty, item.cost.InRMB() }, rowTag: rowtag));
                }
                ITS_l[0] += decimal.Parse(item.itemQty);
                ITS_l[1] += decimal.Parse(item.allCommonMoney.InRMB());
                ITS_l[2] += decimal.Parse(item.ratio);
                ITS_l[3] += decimal.Parse(item.commonMoney.InRMB());
                ITS_l[4] += decimal.Parse(item.selfMoney.InRMB());
                ITS_l[5] += decimal.Parse(item.allSelfMoney.InRMB());
                ITS_l[6] += decimal.Parse(item.cost.InRMB());
                sum[0] += decimal.Parse(item.itemQty);
                sum[1] += decimal.Parse(item.allCommonMoney.InRMB());
                sum[2] += decimal.Parse(item.ratio);
                sum[3] += decimal.Parse(item.commonMoney.InRMB());
                sum[4] += decimal.Parse(item.selfMoney.InRMB());
                sum[5] += decimal.Parse(item.allSelfMoney.InRMB());
                sum[6] += decimal.Parse(item.cost.InRMB());
            }
            if (InPatientType == "医保")
            {
                contentRows.Rows.Add(GetNewRow(new string[]{"小计:","","","",ITS_l[0].ToString(),ITS_l[1].ToString(),ITS_l[2].ToString(),ITS_l[3].ToString(),ITS_l[4].ToString(),
                            ITS_l[5].ToString(),ITS_l[6].ToString() }, TextAlignment.Left));
                contentRows.Rows.Add(GetNewRow(new string[]{"合计:","","","",sum[0].ToString(),sum[1].ToString(),sum[2].ToString(),sum[3].ToString(),sum[4].ToString(),
                            sum[5].ToString(),sum[6].ToString() }, TextAlignment.Left));
            }
            else if (InPatientType == "普通")
            {
                contentRows.Rows.Add(GetNewRow(new string[] { "小计:", "", "", "", ITS_l[0].ToString(), ITS_l[6].ToString() }, TextAlignment.Left));
                contentRows.Rows.Add(GetNewRow(new string[] { "合计:", "", "", "", sum[0].ToString(), sum[6].ToString() }, TextAlignment.Left));
            }
            return true;
        }
        /// <summary>
        /// 获取新行
        /// </summary>
        /// <param name="CellValues"></param>
        /// <param name="FirstColumnTextAlignment"></param>
        /// <param name="FirstColumnSpan"></param>
        /// <param name="columnCount"></param>
        /// <param name="rowTag"></param>
        /// <returns></returns>
        private TableRow GetNewRow(string[] CellValues, TextAlignment FirstColumnTextAlignment = TextAlignment.Center, int FirstColumnSpan = 4, int columnCount = -1, string rowTag = "1")
        {
            var row = new TableRow();
            if (columnCount == -1) { columnCount = CellValues.Length; }
            row.Tag = rowTag;
            for (int c = 0; c < columnCount; c++)
            {
                if (CellValues.Length > c)
                {
                    row.Cells.Add(new TableCell(new Paragraph(new Run(CellValues[c])) { TextAlignment = (c == 0 ? FirstColumnTextAlignment : TextAlignment.Center) }) { ColumnSpan = 1 });
                }
                else
                { row.Cells.Add(new TableCell()); }
            }
            row.Cells[0].ColumnSpan = FirstColumnSpan;
            return row;
        }

        #endregion

        private void Print()
        {
            var doc = this.View.FindName("doc") as FlowDocument;
            var manager = GetInstance<IConfigurationManager>();
            var 激光 = manager.GetValue("Printer:Laser");
            new PrintHelperEx(激光).Print(doc, "出院结算单", drawFooter: Footer);
        }
        private void Footer(DrawingContext context, Rect bounds, int pageNr, int pageSize)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = ResourceEngine.GetImageResourceUri("住院处章");
            image.EndInit();
            context.DrawImage(image, new Rect(new Point(160, 700), new Size(252, 384)));
        }

    }

}
