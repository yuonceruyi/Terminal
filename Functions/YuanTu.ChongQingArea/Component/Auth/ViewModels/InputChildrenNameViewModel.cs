using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Auth.Dialog.Views;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class InputChildrenNameViewModel : ViewModelBase
    {

        public InputChildrenNameViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
            ModifyNameCommand = new DelegateCommand(ModifyNameCmd);
        }

        public override string Title => "输入内容";

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IReSendModel ReSendModel { get; set; }
        public ICommand ConfirmCommand { get; set; }
        public ICommand ModifyNameCommand { get; set; }
        public virtual void Confirm()
        {
            if (Name.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请输入" + InputName);
                return;
            }
            //CreatePatient();
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询您的挂失状态，请稍后...");
                InputValue = Name;
                if (ChoiceModel.Business == Business.补卡)
                {
                    ReSendModel.name = InputValue;
                    //检查能否补卡
                    Logger.Main.Info("开始ReSendModel.Confirm");
                    var ret = ReSendModel.Check?.Invoke();
                    Logger.Main.Info("结束ReSendModel.Confirm");
                    if (ret?.IsSuccess ?? false)
                    {
                        Logger.Main.Info("ReSendModel.Confirm成功");
                        return 1;
                        
                    }
                    else
                    {
                        Logger.Main.Info("ReSendModel.Confirm失败");
                        ShowAlert(false, "补卡检查", "补卡检查失败:" + ret?.Message);
                        return 0;
                    }
                }
                else
                {
                    ShowAlert(false, "补卡检查", "补卡检查流程异常，请重新操作！");
                   
                    return -1;
                }
            }).ContinueWith(ret =>
            {
                if (ret.Result==1)
                {
                    Next();
                }
                else
                {
                    Navigate(A.Home);
                }
            });

        }


        public virtual void ModifyNameCmd()
        {
            Name = "";
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { Name = p; },
                KeyAction = p =>
                {
                    StartTimer();
                    if (p == KeyType.CloseKey)
                        ShowMask(false);
                }
            }, 0.1, pt => { ShowMask(false); });
        }

        public static string InputValue = "";
        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            ModifyNameCommand.Execute(null);
            switch (ChoiceModel.Business)
            {
                case Business.补卡:
                    InputName = "患者姓名";
                    break;
            }
            Hint = InputName;
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return true;
        }

        #region Binding

        private string _name;
        private string _hint = "就诊人信息";
        
        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }
        private string _inputName = "提示内容";

        public string InputName
        {
            get { return _inputName; }
            set
            {
                _inputName = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        

        #endregion Binding
        
    }
}