using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.House.Common;
using YuanTu.Default.House.Component.InfoQuery.Models;
using YuanTu.Default.House.Component.Auth.Models;
using YuanTu.Default.House.Device.BloodPressure;
using YuanTu.Default.House.Device.Fat;
using YuanTu.Default.House.Device.HeightWeight;
using YuanTu.Default.House.Device.SpO2;
using System.Security.Cryptography;
using YuanTu.Core.Log;
using YuanTu.Default.House.Device.Ecg;
using YuanTu.Default.House.Device.Temperature;

namespace YuanTu.Default.House.Component.HealthDetection.ViewModels
{
    public class ReportPreviewViewModel : ViewModelBase
    {
        #region Bindings

        private string age;

        private string idNo;

        private string gender;

        private string heightWeightResult;

        private string name;

        private string phoneNo;

        private ObservableCollection<Report> reportResult;
        private string time;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public string Gender
        {
            get { return gender; }
            set
            {
                gender = value;
                OnPropertyChanged();
            }
        }

        public string Age
        {
            get { return age; }
            set
            {
                age = value;
                OnPropertyChanged();
            }
        }

        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }

        public string IdNo
        {
            get { return idNo; }
            set
            {
                idNo = value;
                OnPropertyChanged();
            }
        }

        public string PhoneNo
        {
            get { return phoneNo; }
            set
            {
                phoneNo = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Report> ReportResult
        {
            get { return reportResult; }
            set
            {
                reportResult = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public override string Title => "报告预览";

        public ICommand PrintCommand
        {
            get { return new DelegateCommand(DoPrint); }
        }

        

        [Dependency]
        public IHealthModel HealthModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public IReportModel ReportModel { get; set; }

        private Uri _慧医图;
        private Image _二维码;
        public Uri 慧医图
        {
            get { return _慧医图; }
            set
            {
                _慧医图 = value;
                OnPropertyChanged();
            }
        }

        public Image 二维码
        {
            get { return _二维码; }
            set
            {
                _二维码 = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
     

        public override void OnSet()
        {
            慧医图 = ResourceEngine.GetImageResourceUri("屏保慧医二维码_House");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            DisablePreviewButton = ChoiceModel.Business == Business.健康服务;
            InitQrCode();
            InitData();
            PlaySound(SoundMapping.扫描或打印查看报告单);
        }

        protected virtual void InitQrCode()
        {
            //todo 初始化二维码
            var id = ChoiceModel.Business == Business.健康服务 ? ReportModel.上传体检报告结果.data.ToString() : ReportModel.选中的查询体检报告单.id;
            var bid = Encoding.Default.GetBytes(id);
            var base64Id = Convert.ToBase64String(bid);
            var h5Url=$"{ConfigurationManager.GetValue("H5Url")}&key={base64Id}";
            Logger.Printer.Info($"H5Url:{h5Url}");
            var barQrCodeGenerater = GetInstance<IBarQrCodeGenerator>();
            二维码 = barQrCodeGenerater.QrcodeGenerate(h5Url);
        }
        private void InitData()
        {
            Name = HealthModel?.Res查询是否已建档?.data?.name;
            Gender = HealthModel?.Res查询是否已建档?.data?.sex;
            IdNo = HealthModel?.Res查询是否已建档?.data?.idNo;
            Age = HealthModel?.Res查询是否已建档?.data?.age;
            PhoneNo = HealthModel?.Res查询是否已建档?.data?.phone;

            var heightWeightModel = GetInstance<IHeightWeightModel>();
            var fatModel = GetInstance<IFatModel>();
            var bloodPressureModel = GetInstance<IBloodPressureModel>();
            var spO2Model = GetInstance<ISpO2Model>();
            var temperatureModel = GetInstance<ITemperatureModel>();
            var ecgModel = GetInstance<IEcgModel>();

            if (ChoiceModel.Business == Business.体测查询)
            {
                var heightWeightGroup = ReportModel?.选中的查询体检报告单?.groupList?.FirstOrDefault(p => p.groupName == "身高体重");
                if (heightWeightGroup != null)
                {
                    heightWeightModel.身高 =
                        Convert.ToDecimal(heightWeightGroup.childList.FirstOrDefault(p => p.childName == "身高").dataStr);
                    heightWeightModel.体重 =
                        Convert.ToDecimal(heightWeightGroup.childList.FirstOrDefault(p => p.childName == "体重").dataStr);
                    heightWeightModel.体质指数 =
                        Convert.ToDecimal(
                            heightWeightGroup.childList.FirstOrDefault(p => p.childName.StartsWith("体质指数")).dataStr);
                    heightWeightModel.参考结果 =
                        heightWeightGroup?.childList?.FirstOrDefault(p => p.childName == "参考结果").dataStr;
                }
                var fatGroup = ReportModel.选中的查询体检报告单.groupList.FirstOrDefault(p => p.groupName == "体脂");
                if (fatGroup != null)
                {
                    fatModel.脂肪含量 =
                        Convert.ToDecimal(fatGroup.childList.FirstOrDefault(p => p.childName == "脂肪含量").dataStr);
                    fatModel.基础代谢值 =
                        Convert.ToDecimal(fatGroup.childList.FirstOrDefault(p => p.childName == "基础代谢值").dataStr);
                    fatModel.体型参考结果 = fatGroup.childList.FirstOrDefault(p => p.childName == "体型参考结果").dataStr;
                }

                var bloodPressurelGroup = ReportModel.选中的查询体检报告单.groupList.FirstOrDefault(p => p.groupName == "血压");
                if (bloodPressurelGroup != null)
                {
                    bloodPressureModel.收缩压 =
                        Convert.ToDecimal(
                            bloodPressurelGroup.childList.FirstOrDefault(p => p.childName.StartsWith("收缩压")).dataStr);
                    bloodPressureModel.脉搏 =
                        Convert.ToDecimal(
                            bloodPressurelGroup.childList.FirstOrDefault(p => p.childName.StartsWith("脉搏")).dataStr);
                    bloodPressureModel.舒张压 =
                        Convert.ToDecimal(
                            bloodPressurelGroup.childList.FirstOrDefault(p => p.childName.StartsWith("舒张压")).dataStr);
                    bloodPressureModel.参考结果 =
                        bloodPressurelGroup.childList.FirstOrDefault(p => p.childName == "参考结果").dataStr;
                }

                var temperatureGroup = ReportModel.选中的查询体检报告单.groupList.FirstOrDefault(p => p.groupName == "体温");
                if (temperatureGroup != null)
                {
                    temperatureModel.表面温度 = temperatureGroup.childList.FirstOrDefault(p => p.childName.StartsWith("表面温度")) == null
                        ? Convert.ToDecimal(temperatureGroup.childList.FirstOrDefault(p => p.childName == "Surface").dataStr)
                        : Convert.ToDecimal(
                            temperatureGroup.childList.FirstOrDefault(p => p.childName.StartsWith("表面温度")).dataStr);
                    temperatureModel.人体温度 = temperatureGroup.childList.FirstOrDefault(p => p.childName.StartsWith("人体温度")) == null
                        ? Convert.ToDecimal(temperatureGroup.childList.FirstOrDefault(p => p.childName == "Body").dataStr)
                        : Convert.ToDecimal(
                            temperatureGroup.childList.FirstOrDefault(p => p.childName.StartsWith("人体温度")).dataStr);
                    temperatureModel.环境温度 = temperatureGroup.childList.FirstOrDefault(p => p.childName == "环境温度") == null
                        ? Convert.ToDecimal(temperatureGroup.childList.FirstOrDefault(p => p.childName == "Room").dataStr)
                        : Convert.ToDecimal(temperatureGroup.childList.FirstOrDefault(p => p.childName == "环境温度").dataStr);
                    ;
                    temperatureModel.参考结果 = temperatureGroup.childList.FirstOrDefault(p => p.childName == "参考结果").dataStr;
                }

                var ecgGroup = ReportModel.选中的查询体检报告单.groupList.FirstOrDefault(p => p.groupName == "体温");
                if (ecgGroup != null)
                {
                    ecgModel.Diag = ecgGroup.childList.FirstOrDefault(p => p.childName.StartsWith("诊断类型")) == null
                        ? Convert.ToByte(ecgGroup.childList.FirstOrDefault(p => p.childName == "Diag").dataStr)
                        : Convert.ToByte(
                            ecgGroup.childList.FirstOrDefault(p => p.childName.StartsWith("诊断类型")).dataStr);
                    ecgModel.PR = ecgGroup.childList.FirstOrDefault(p => p.childName.StartsWith("脉搏")) == null
                        ? Convert.ToDecimal(ecgGroup.childList.FirstOrDefault(p => p.childName == "PR").dataStr)
                        : Convert.ToDecimal(
                            ecgGroup.childList.FirstOrDefault(p => p.childName.StartsWith("脉搏")).dataStr);
                    ecgModel.参考结果 = ecgGroup.childList.FirstOrDefault(p => p.childName == "参考结果").dataStr;
                }

                var spO2Group = ReportModel.选中的查询体检报告单.groupList.FirstOrDefault(p => p.groupName == "血氧");
                if (spO2Group != null)
                {
                    spO2Model.PI = spO2Group.childList.FirstOrDefault(p => p.childName.StartsWith("灌注指数")) == null
                        ? Convert.ToDecimal(spO2Group.childList.FirstOrDefault(p => p.childName == "PI").dataStr)
                        : Convert.ToDecimal(
                            spO2Group.childList.FirstOrDefault(p => p.childName.StartsWith("灌注指数")).dataStr);
                    spO2Model.PR = spO2Group.childList.FirstOrDefault(p => p.childName.StartsWith("脉搏")) == null
                        ? Convert.ToDecimal(spO2Group.childList.FirstOrDefault(p => p.childName == "PR").dataStr)
                        : Convert.ToDecimal(
                            spO2Group.childList.FirstOrDefault(p => p.childName.StartsWith("脉搏")).dataStr);
                    spO2Model.SpO2 = spO2Group.childList.FirstOrDefault(p => p.childName == "血氧饱和度") == null
                        ? Convert.ToDecimal(spO2Group.childList.FirstOrDefault(p => p.childName == "SpO2").dataStr)
                        : Convert.ToDecimal(spO2Group.childList.FirstOrDefault(p => p.childName == "血氧饱和度").dataStr);
                    ;
                    spO2Model.参考结果 = spO2Group.childList.FirstOrDefault(p => p.childName == "参考结果").dataStr;
                }
            }
            Time = ChoiceModel.Business == Business.体测查询
                ? ReportModel.选中的查询体检报告单.date
                : DateTimeCore.Now.ToString("yyyy-MM-dd  HH:mm:ss");
            ReportResult = new ObservableCollection<Report>();
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "身高体重测量") != null)
                ReportResult.Add(new Report
                {
                    ProjectName = "身高体重",
                    Measurements =
                        $"身高：{heightWeightModel?.身高}cm  体重：{heightWeightModel?.体重}KG  体质指数(BMI)：{heightWeightModel?.体质指数}"
                });
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体脂测量") != null)
                ReportResult.Add(new Report
                {
                    ProjectName = "体脂",
                    Measurements = $"脂肪含量：{fatModel?.脂肪含量}%  基础代谢：{fatModel?.基础代谢值}Kcal "
                });
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血压测量") != null)
                ReportResult.Add(new Report
                {
                    ProjectName = "血压",
                    Measurements =
                        $"收缩压：{bloodPressureModel?.收缩压}mmHg  舒张压：{bloodPressureModel?.舒张压}mmHg  脉搏：{bloodPressureModel?.脉搏}bpm"
                });
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体温测量") != null)
                ReportResult.Add(new Report
                {
                    ProjectName = "体温",
                    Measurements =
                        $"表面温度：{temperatureModel?.表面温度}mmHg  人体温度：{temperatureModel?.人体温度}mmHg  环境温度：{temperatureModel?.环境温度}bpm"
                });
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血氧测量") != null)
                ReportResult.Add(new Report
                {
                    ProjectName = "血氧",
                    Measurements = $"血氧饱和度：{spO2Model?.SpO2}%  脉搏：{spO2Model?.PR}bpm  灌注指数：{spO2Model?.PI}"
                });
        }

        public void DoPrint()
        {
            //ShowAlert(false, "提示", "开始打印！");
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), ReporterPrintables());
            Next();
        }

        protected virtual Queue<IPrintable> ReporterPrintables()
        {
            var heightWeightModel = GetInstance<IHeightWeightModel>();
            var fatModel = GetInstance<IFatModel>();
            var bloodPressureModel = GetInstance<IBloodPressureModel>();
            var spO2Model = GetInstance<ISpO2Model>();
            var temperatureModel = GetInstance<ITemperatureModel>();
            var queue = PrintManager.NewQueue($"体测报告单");
            var patientInfo = HealthModel.Res查询是否已建档?.data;
            var sb = new StringBuilder();

            sb.Append($"姓名：{patientInfo?.name}\n");
            sb.Append($"性别：{patientInfo?.sex}\n");
            sb.Append($"年龄：{patientInfo?.age} 岁\n");
            sb.Append($"\n");
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "身高体重测量") != null)
            {
                sb.Append($"身高：{heightWeightModel.身高} cm\n");
                sb.Append($"体重：{heightWeightModel.体重} kg\n");
                sb.Append($"体质指数(BMI)：{heightWeightModel.体质指数}\n");
                sb.Append($"参考结果：{heightWeightModel.参考结果}\n");
                sb.Append($"\n");
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体脂测量") != null)
            {
                sb.Append($"脂肪含量：{fatModel.脂肪含量} %\n");
                sb.Append($"基础代谢值：{fatModel.基础代谢值} kcal\n");
                sb.Append($"体型参考结果：{fatModel.体型参考结果}\n");
                sb.Append($"\n");
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血压测量") != null)
            {
                sb.Append($"收缩压(SBP)：{bloodPressureModel.收缩压} mmHg\n");
                sb.Append($"舒张压(DBP)：{bloodPressureModel.舒张压} mmHg\n");
                sb.Append($"脉搏(PR)：{bloodPressureModel.脉搏} bpm\n");
                sb.Append($"参考结果：{bloodPressureModel.参考结果}\n");
                sb.Append($"\n");
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "体温测量") != null)
            {
                sb.Append($"表面温度：{temperatureModel.表面温度} ℃\n");
                sb.Append($"人体温度：{temperatureModel.人体温度} ℃\n");
                sb.Append($"环境温度：{temperatureModel.环境温度} ℃\n");
                sb.Append($"参考结果：{temperatureModel.参考结果}\n");
                sb.Append($"\n");
            }
            if (ViewContexts.ViewContextList?.FirstOrDefault(p => p.Title == "血氧测量") != null)
            {
                sb.Append($"血氧饱和度：{spO2Model.SpO2} %\n");
                sb.Append($"脉搏(PR)：{spO2Model.PR} bpm\n");
                sb.Append($"灌注指数(PI)：{spO2Model.PI}\n");
                sb.Append($"参考结果：{spO2Model.参考结果}\n");
                sb.Append($"\n");
            }
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});

            var qrCodePath = ResourceEngine.GetResourceFullPath("打印二维码_House");
            var qrCode = Image.FromFile(qrCodePath);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = qrCode,
                //Height = qrCode.Height / 1.5f,
                //Width = qrCode.Width / 1.5f
                Height = qrCode.Height,
                Width = qrCode.Width
            });

            sb = new StringBuilder();
            sb.Append($"\n");
            sb.Append($"打印时间:{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"机柜编号:{FrameworkConst.OperatorId}\n");
            sb.Append($"温馨提示:以上测量数据仅供参考\n");
            sb.Append($"                           \n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});

            return queue;
        }
    }
}