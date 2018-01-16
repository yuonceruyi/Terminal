using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.House.Common;
using YuanTu.Default.House.Component.Auth.Models;
using YuanTu.Default.House.Component.InfoQuery.Models;
using YuanTu.Default.House.Device.BloodPressure;
using YuanTu.Default.House.Device.Ecg;
using YuanTu.Default.House.Device.Fat;
using YuanTu.Default.House.Device.HeightWeight;
using YuanTu.Default.House.Device.SpO2;
using YuanTu.Default.House.Device.Temperature;
using YuanTu.Default.House.HealthManager;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        public ReportViewModel()
        {
        }

        public ICommand PreviewCommand
        {
            get
            {
                return new DelegateCommand(DoPreview);
            }
        }

        [Dependency]
        public IHeightWeightModel HeightWeightModel { get; set; }
        [Dependency]
        public ISpO2Model SpO2Model { get; set; }
        [Dependency]
        public IBloodPressureModel BloodPressureModel { get; set; }
        [Dependency]
        public IFatModel FatModel { get; set; }
        [Dependency]
        public IHealthModel HealthModel { get; set; }
        [Dependency]
        public ITemperatureModel Temperature { get; set; }
        [Dependency]
        public IEcgModel Ecg { get; set; }
        [Dependency]
        public IReportModel ReportModel { get; set; }

        public override string Title => "上传报告";
      
        private string name = "江直树";
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        private string gender = "男";
        public string Gender
        {
            get
            {
                return gender;
            }
            set
            {
                gender = value;
                OnPropertyChanged();
            }
        }
        private string age = "21";
        public string Age
        {
            get
            {
                return age;
            }
            set
            {
                age = value;
                OnPropertyChanged();
            }
        }
        private string time = "2111-10-10";
        public string Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Report> reportResult;
        public ObservableCollection<Report> ReportResult
        {
            get
            {
                return reportResult;
            }
            set
            {
                reportResult = value;
                OnPropertyChanged();
            }
        }

        public ICommand RetestCommand
        {
            get
            {
                return new DelegateCommand<string>(DoRetest);
            }
        }

        private void DoRetest(string projectName)
        {
            var route = FunctionDic.Instance.RouteDic[projectName];
            Navigate(route);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            this.Name = HealthModel?.Res查询是否已建档?.data?.name;
            this.Gender = HealthModel?.Res查询是否已建档?.data?.sex;
            this.Age = HealthModel?.Res查询是否已建档?.data?.age;
            this.Time = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ReportResult = new ObservableCollection<Report>();
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "身高体重测量") != null)
            {
                ReportResult.Add(new Report
                {
                    ProjectName = "身高体重",
                    Measurements = $"身高：{HeightWeightModel?.身高}cm  体重：{HeightWeightModel?.体重}KG  体质指数(BMI)：{HeightWeightModel?.体质指数}",
                    ReferenceResult = HeightWeightModel?.参考结果,
                    Status = "1"
                });
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体脂测量") != null)
            {
                ReportResult.Add(new Report
                {
                    ProjectName = "体脂",
                    Measurements = $"脂肪含量：{FatModel?.脂肪含量}%  基础代谢：{FatModel?.基础代谢值}Kcal ",
                    ReferenceResult = $"{FatModel?.体型参考结果}",
                    Status = "1"
                });
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血压测量") != null)
            {
                ReportResult.Add(new Report
                {
                    ProjectName = "血压",
                    Measurements = $"收缩压：{BloodPressureModel?.收缩压}mmHg  舒张压：{BloodPressureModel?.舒张压}mmHg  脉搏：{BloodPressureModel?.脉搏}bpm",
                    ReferenceResult = BloodPressureModel?.参考结果,
                    Status = "1"
                });
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体温测量") != null)
            {
                ReportResult.Add(new Report
                {
                    ProjectName = "体温",
                    Measurements = $"表面温度：{Temperature?.表面温度}℃  人体温度：{Temperature?.人体温度}℃  环境温度：{Temperature?.环境温度}℃",
                    ReferenceResult = Temperature?.参考结果,
                    Status = "1"
                });
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "心电测量") != null)
            {
                ReportResult.Add(new Report
                {
                    ProjectName = "心电",
                    Measurements = $"脉搏：{Ecg?.PR}bpm",
                    ReferenceResult = Ecg?.参考结果,
                    Status = "1"
                });
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血氧测量") != null)
            {
                ReportResult.Add(new Report
                {
                    ProjectName = "血氧",
                    Measurements = $"血氧饱和度：{SpO2Model?.SpO2}%  脉搏：{SpO2Model?.PR}bpm  灌注指数：{SpO2Model?.PI}",
                    ReferenceResult = SpO2Model?.参考结果,
                    Status = "1"
                });
            }
            PlaySound(SoundMapping.生成体检报告);
        }

        private void DoPreview()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在生成检查报告单，请稍候...");
                var groupList = GetData();
                var req = new req上传体检报告
                {
                    healthUserId = HealthModel.Res查询是否已建档.data.id,
                    idNo = HealthModel.Res查询是否已建档.data.idNo,
                    icpcode = FrameworkConst.OperatorId,
                    groupList = groupList
                };
               
                var res = HealthDataHandlerEx.上传体检报告(req);
                if (res.success)
                {
                    ReportModel.上传体检报告结果 = res;
                    Next();
                }
                else
                {
                    ShowAlert(false, "友好提示", "生成检查报告单失败", debugInfo: res.msg);
                }
            });
        }

        public List<测量组> GetData()
        {
            var groupList = new List<测量组>();
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "身高体重测量") != null)
            {
                var heightWeightGroup = new 测量组() { groupName = "身高体重" };
                heightWeightGroup.childList = new List<测量组子集>();
                heightWeightGroup.childList.Add(new 测量组子集
                {
                    childName = "身高",
                    dataStr = HeightWeightModel.身高.ToString(),
                    unit = "cm"
                });
                heightWeightGroup.childList.Add(new 测量组子集
                {
                    childName = "体重",
                    dataStr = HeightWeightModel.体重.ToString(),
                    unit = "kg"
                });
                heightWeightGroup.childList.Add(new 测量组子集
                {
                    childName = "体质指数(BMI)",
                    dataStr = HeightWeightModel.体质指数.ToString(),
                    unit = ""
                });
                heightWeightGroup.childList.Add(new 测量组子集
                {
                    childName = "参考结果",
                    dataStr = HeightWeightModel.参考结果.ToString(),
                    unit = ""
                });
                groupList.Add(heightWeightGroup);
            }

            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体脂测量") != null)
            {
                var fatGroup = new 测量组() { groupName = "体脂" };
                fatGroup.childList = new List<测量组子集>();
                fatGroup.childList.Add(new 测量组子集 { childName = "脂肪含量", dataStr = FatModel.脂肪含量.ToString(), unit = "%" });
                fatGroup.childList.Add(new 测量组子集
                {
                    childName = "基础代谢值",
                    dataStr = FatModel.基础代谢值.ToString(),
                    unit = "KCal"
                });
                fatGroup.childList.Add(new 测量组子集 { childName = "体型参考结果", dataStr = FatModel.体型参考结果.ToString(), unit = "" });
                groupList.Add(fatGroup);
            }

            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血压测量") != null)
            {
                var bloodPressureGroup = new 测量组() { groupName = "血压" };
                bloodPressureGroup.childList = new List<测量组子集>();
                bloodPressureGroup.childList.Add(new 测量组子集
                {
                    childName = "收缩压(SBP)",
                    dataStr = BloodPressureModel.收缩压.ToString(),
                    unit = "mmHg"
                });
                bloodPressureGroup.childList.Add(new 测量组子集
                {
                    childName = "舒张压(DBP)",
                    dataStr = BloodPressureModel.舒张压.ToString(),
                    unit = "mmHg"
                });
                bloodPressureGroup.childList.Add(new 测量组子集
                {
                    childName = "脉搏(PR)",
                    dataStr = BloodPressureModel.脉搏.ToString(),
                    unit = "bpm"
                });
                bloodPressureGroup.childList.Add(new 测量组子集
                {
                    childName = "参考结果",
                    dataStr = BloodPressureModel.参考结果.ToString(),
                    unit = ""
                });
                groupList.Add(bloodPressureGroup);
            }

            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血氧测量") != null)
            {
                var spO2Group = new 测量组() { groupName = "血氧" };
                spO2Group.childList = new List<测量组子集>();
                spO2Group.childList.Add(new 测量组子集 { childName = "血氧饱和度", dataStr = SpO2Model.SpO2.ToString(), unit = "%" });
                spO2Group.childList.Add(new 测量组子集
                {
                    childName = "脉搏(PR)",
                    dataStr = SpO2Model.PR.ToString(),
                    unit = "bpm"
                });
                spO2Group.childList.Add(new 测量组子集 { childName = "灌注指数(PI)", dataStr = SpO2Model.PI.ToString(), unit = "" });
                spO2Group.childList.Add(new 测量组子集 { childName = "参考结果", dataStr = SpO2Model.参考结果.ToString(), unit = "" });
                groupList.Add(spO2Group);
            }

            return groupList;
        }
        public void GetProperties<T>(T t, ref List<测量组> groupList)
        {
            if (t == null)
            {
                return;
            }
            var groupName = FunctionDic.Instance.ModelDic[(IModel)t];
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                var name = item.Name;
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    groupList.Add(new 测量组 { groupName = groupName, childList = new List<测量组子集> { new 测量组子集 { childName = name, dataStr = value.ToString() } } });
                }
                else
                {
                    GetProperties(value, ref groupList);
                }
            }
            return;
        }

    }
    public class Report
    {
        public string ProjectName { get; set; }
        public string Measurements { get; set; }
        public string ReferenceResult { get; set; }
        public string Status { get; set; }
    }
}
