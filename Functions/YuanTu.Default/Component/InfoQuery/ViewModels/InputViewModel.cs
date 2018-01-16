using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class InputViewModel : ViewModelBase
    {
        private string _hint;
        private string _pinCode;
        private string _queryTips;
        private string _queryType;

        //public Uri KeyboardImage { get; set; } =ResourceEngine.GetImageResourceUri("键盘_退格") ;
        public InputViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "药品查询";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public string PinCode
        {
            get { return _pinCode; }
            set
            {
                _pinCode = value;
                OnPropertyChanged();
            }
        }

        public string QueryType
        {
            get { return _queryType; }
            set
            {
                _queryType = value;
                OnPropertyChanged();
            }
        }

        public string QueryTips
        {
            get { return _queryTips; }
            set
            {
                _queryTips = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IQueryChoiceModel QueryChoiceModel { get; set; }

        [Dependency]
        public IMedicineModel MedicineModel { get; set; }

        [Dependency]
        public IChargeItemsModel ChargeItemsModel { get; set; }

        public ICommand ConfirmCommand { get; set; }

        protected virtual void Confirm()
        {
            if (string.IsNullOrWhiteSpace(PinCode))
            {
                ShowAlert(false, "温馨提示", "请输入首字母");
                return;
            }
            ChangeNavigationContent(PinCode);
            DoCommand(ctx =>
            {
                ctx.ChangeText("正在查询，请稍候...");
                if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询)
                {
                    MedicineModel.Req药品项目查询 = new req药品项目查询
                    {
                        pinyinCode = PinCode.ToUpper()
                    };
                    MedicineModel.Res药品项目查询 = DataHandlerEx.药品项目查询(MedicineModel.Req药品项目查询);
                    if (MedicineModel.Res药品项目查询.success)
                    {
                        if (MedicineModel.Res药品项目查询.data == null || MedicineModel.Res药品项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "药品信息查询", "未查询到相关药品的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "药品信息查询", "未查询到相关药品的信息", debugInfo: MedicineModel.Res药品项目查询.msg);
                    }
                }
                else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.项目查询)
                {
                    ChargeItemsModel.Req收费项目查询 = new req收费项目查询
                    {
                        pinyinCode = PinCode.ToUpper()
                    };
                    ChargeItemsModel.Res收费项目查询 = DataHandlerEx.收费项目查询(ChargeItemsModel.Req收费项目查询);
                    if (ChargeItemsModel.Res收费项目查询.success)
                    {
                        if (ChargeItemsModel.Res收费项目查询.data == null || ChargeItemsModel.Res收费项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息(列表为空)");
                            return;
                        }
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息", debugInfo: ChargeItemsModel.Res收费项目查询.msg);
                    }
                }
            });
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            HideNavigating = true;
            PinCode = null;
            QueryType = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询 ? "药品首字母：" : "诊疗项首字母：";
            Hint = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询 ? "药品查询" : "诊疗项查询";
            QueryTips = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询
                ? "请输入您要查询的药品信息的首字母"
                : "请在下方输入诊疗项首字母进行查询";
        }
    }
}