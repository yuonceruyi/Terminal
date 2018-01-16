using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.House.Component.InfoQuery.Models;
using YuanTu.Default.House.HealthManager;

namespace YuanTu.Default.House.Component.InfoQuery.ViewModels
{
    public class QueryViewModel : ViewModelBase
    {
        public override string Title => "体测报告选择";

        private ObservableCollection<InfoHouseReport> _data;

        public ObservableCollection<InfoHouseReport> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IReportModel ReportModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            #region
            //var list = new List<InfoHouseReport>();
            //var confirmCommand = new DelegateCommand<Info>(Confirm);
            //foreach (var record in ReportModel.查询体检报告单分页数据.records)
            //{
            //    var allResultList = new List<string>();
            //    foreach (var group in record.groupList)
            //    {
            //        var childList = group?.childList;
            //        if (childList == null)
            //        {
            //            continue;
            //        }

            //        childList.ForEach((p) =>
            //        {
            //            if (p.childName == "参考结果" && p.dataStr != "正常" && p.dataStr != "理想")
            //            {
            //                allResultList.Add(p.dataStr);
            //            }
            //        });
            //    }
            //    Uri imageSource;
            //    var displayResult = new StringBuilder();
            //    if (!allResultList.Any())
            //    {
            //        displayResult.Append("正常");
            //        imageSource =ResourceEngine.GetImageResourceUri("健康小屋report蓝");
            //    }
            //    else if (allResultList.Count() > 1)
            //    {
            //        allResultList.ForEach(p => { displayResult.Append($" {p}"); });
            //        imageSource =ResourceEngine.GetImageResourceUri("健康小屋report红");
            //    }
            //    else
            //    {
            //        displayResult.Append(allResultList.First());
            //        imageSource =ResourceEngine.GetImageResourceUri("健康小屋report黄");
            //    }
            //    list.Add(new InfoHouseReport
            //    {
            //        Tag = record,
            //        Date = $"测试时间: {record.date}",
            //        Result = $"测试结果: {displayResult}",
            //        ImageSource = imageSource,
            //        ConfirmCommand = confirmCommand,
            //    });
            //}
            #endregion
            var list = ReportModel.查询体检报告单分页数据.records.Select(BuildReport);
            Data = new ObservableCollection<InfoHouseReport>(list);
        }

        protected virtual InfoHouseReport BuildReport(查询体检报告单 item)
        {
            /*
             *思路：
             * 1.获取所有childName为“参考结果”的dataStr的值，并去重，得到referResultLst列表
             * 2.在referResultLst列表中删掉常规的级别
             * 3.根据剩下的结果判断测试结果并初始化数据
             */
            var report = new InfoHouseReport
            {
                Tag = item,
                Date = $"测试时间: {item.date}",
                ConfirmCommand = new DelegateCommand<Info>(Confirm)
            };
            var referResultLst = item.groupList.SelectMany(p => p.childList).Where(p => p.childName == "参考结果").Select(p => p.dataStr).Distinct().ToList();
            var normalLevel = new[] { "正常", "理想" };

            referResultLst.RemoveAll(p => normalLevel.Contains(p));

            if (!referResultLst.Any())
            {
                report.ImageSource = ResourceEngine.GetImageResourceUri("健康小屋report蓝");
                report.Result = "测试结果：正常";
            }
            else if (referResultLst.Count == 1)
            {
                report.ImageSource = ResourceEngine.GetImageResourceUri("健康小屋report黄");
                report.Result = $"测试结果：{referResultLst[0]}";
            }
            else
            {
                report.ImageSource = ResourceEngine.GetImageResourceUri("健康小屋report红");
                report.Result = $"测试结果：{string.Join(" ", referResultLst)}";
            }
            return report;
        }

        protected virtual void Confirm(Info obj)
        {
            var report = obj.Tag.As<查询体检报告单>();
            ReportModel.选中的查询体检报告单 = report;
            #region
            //foreach (var group in report.groupList)
            //{
            //    if (FunctionDic.Instance.ModelDic.Values.Contains(group.groupName))
            //    {
            //        var model = FunctionDic.Instance.ModelDic.FirstOrDefault(p => p.Value == group.groupName).Key;
            //        System.Reflection.PropertyInfo[] properties = model.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            //        foreach (var child in group.childList)
            //        {
            //            properties.ForEach((p) =>
            //            {
            //                if (child.childName == p.Name)
            //                {
            //                    var val=Convert.ChangeType(child.dataStr,p.PropertyType);
            //                    p.SetValue(model, val);
            //                }
            //            });
            //        }
            //    }
            //}
            #endregion
            Next();
        }
    }
}