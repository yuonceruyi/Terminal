using Prism.Regions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Default.Component.Auth.Dialog.Views;
using System;
using YuanTu.Core.Extension;
using YuanTu.Devices.MKeyBoard;

namespace YuanTu.ChongQingArea.Component.InfoQuery.ViewModels
{
    public class InputViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InputViewModel
    {
        private bool _showFullInputBoard;

        public bool ShowFullInputBoard
        {
            get { return _showFullInputBoard; }
            set { _showFullInputBoard = value; OnPropertyChanged(); }
        }

        public ICommand FullInputShowCommand { get; set; }

        public InputViewModel() : base()
        {
            FullInputShowCommand = new DelegateCommand(FullInputBoardShow);
        }

        /// <summary>
        /// 重写键盘输入提交事件  ——— 医改药品价格变动信息查询以及医改项目价格变动信息查询
        /// </summary>
        protected override void Confirm()
        {
            if ((PinCode?.Length ?? 0) < 2 || string.IsNullOrWhiteSpace(PinCode))
            {
                ShowAlert(false, "温馨提示", "请至少输入两位字符");
                return;
            }
            var tempCode = PinCode;
            ShowFullInputBoard = false;
            ChangeNavigationContent(tempCode);
            PinCode = null;

            DoCommand(ctx =>
            {
                ctx.ChangeText("正在查询，请稍候...");
                if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品变动价格查询)
                {
                    MedicineModel.Req药品项目查询 = new req药品项目查询
                    {
                        pinyinCode = tempCode.ToUpper(),
                        extend = "yg",
                    };
                    MedicineModel.Res药品项目查询 = DataHandlerEx.药品项目查询(MedicineModel.Req药品项目查询);

                    if (MedicineModel.Res药品项目查询.success)
                    {
                        if (MedicineModel.Res药品项目查询.data == null || MedicineModel.Res药品项目查询.data.Count == 0
                            || MedicineModel.Res药品项目查询.data.All(a => (a.producer.IndexOf("自制") >= 0 &&  a.type == null ) && a.price == a.extend ))
                        {
                            ShowAlert(false, "药品信息查询", "未查询到相关药品的信息(列表为空)");
                            ShowFullInputBoard = false;
                            return;
                        }
                        Produce(MedicineModel.Res药品项目查询);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "药品信息查询", "未查询到相关药品的信息", debugInfo: MedicineModel.Res药品项目查询.msg);
                    }
                }
                else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.项目变动价格查询)
                {
                    ChargeItemsModel.Req收费项目查询 = new req收费项目查询
                    {
                        pinyinCode = tempCode.ToUpper(),
                        extend = "yg",
                    };

                    ChargeItemsModel.Res收费项目查询 = DataHandlerEx.收费项目查询(ChargeItemsModel.Req收费项目查询);

                    if (ChargeItemsModel.Res收费项目查询.success)
                    {
                        if (ChargeItemsModel.Res收费项目查询.data == null || ChargeItemsModel.Res收费项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息(列表为空)");
                            ShowFullInputBoard = false;
                            return;
                        }
                        Produce(ChargeItemsModel.Res收费项目查询);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息", debugInfo: ChargeItemsModel.Res收费项目查询.msg);
                    }
                }
                else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询)
                {
                    MedicineModel.Req药品项目查询 = new req药品项目查询
                    {
                        pinyinCode = tempCode.ToUpper(),
                        extend = "",
                    };
                    MedicineModel.Res药品项目查询 = DataHandlerEx.药品项目查询(MedicineModel.Req药品项目查询);
                    if (MedicineModel.Res药品项目查询.success)
                    {
                        if (MedicineModel.Res药品项目查询.data == null || MedicineModel.Res药品项目查询.data.Count == 0
                            || MedicineModel.Res药品项目查询.data.All(a=>a.type == null ))
                        {
                            ShowAlert(false, "药品信息查询", "未查询到相关药品的信息(列表为空)");
                            ShowFullInputBoard = false;
                            return;
                        }
                        Produce(MedicineModel.Res药品项目查询);
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
                        pinyinCode = tempCode.ToUpper(),
                        extend="",
                    };
                    ChargeItemsModel.Res收费项目查询 = DataHandlerEx.收费项目查询(ChargeItemsModel.Req收费项目查询);
                    if (ChargeItemsModel.Res收费项目查询.success)
                    {
                        if (ChargeItemsModel.Res收费项目查询.data == null || ChargeItemsModel.Res收费项目查询.data.Count == 0)
                        {
                            ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息(列表为空)");
                            ShowFullInputBoard = false;
                            return;
                        }

                        Produce(ChargeItemsModel.Res收费项目查询);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "诊疗项查询", "未查询到相关诊疗项的信息", debugInfo: ChargeItemsModel.Res收费项目查询.msg);
                    }
                }
            });
        }


        private void Produce(res药品项目查询 res)
        {
            res.data.ForEach(p =>
            {
                p.extend = (decimal.Parse(p.extend.BackNotNullOrEmpty("0")) * 100).ToString(CultureInfo.InvariantCulture);
                p.price = (decimal.Parse(p.price) * 100).ToString(CultureInfo.InvariantCulture);
            });
        }
        private void Produce(res收费项目查询 res)
        {
            res.data.ForEach(p =>
            {
                p.extend = (decimal.Parse(p.extend.BackNotNullOrEmpty("0")) * 100).ToString(CultureInfo.InvariantCulture);
                p.price = (decimal.Parse(p.price) * 100).ToString(CultureInfo.InvariantCulture);
            });
        }


        /// <summary>
        ///重写 键盘输入页面提示信息显示
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            HideNavigating = true;
            PinCode = null;
            ShowFullInputBoard = false;

            if (TimeOut == 0)
            {
                ShowFullInputBoard = true;
            }

            if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询)
            {
                QueryType = "药品首字母";
                Hint = "药品查询";
                QueryTips = "请在下方输入药品首字母进行查询";
            }
            else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.项目查询)
            {
                QueryType = "诊疗项首字母";
                Hint = "诊疗项查询";
                QueryTips = "请在下方输入诊疗项首字母进行查询";
            }
            else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品变动价格查询)
            {

                QueryType = "医改药品首字母";
                Hint = "医改药品查询";
                QueryTips = "请在下方输入医改药品首字母进行查询";

            }
            else if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.项目变动价格查询)
            {
                QueryType = "医改诊疗项首字母";
                Hint = "医改诊疗项查询";
                QueryTips = "请在下方输入医改诊疗项首字母进行查询";

            }


            //    QueryType = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询 ? "药品首字母：" : "诊疗项首字母：";
            //Hint = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询 ? "药品查询" : "诊疗项查询";
            //QueryTips = QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.药品查询
            //    ? "请输入您要查询的药品信息的首字母"
            //    : "请在下方输入诊疗项首字母进行查询";
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return base.OnLeaving(navigationContext);
        }

        private void FullInputBoardShow()
        {
            ShowFullInputBoard = true;
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { PinCode = p; },
                KeyAction = p =>
                {
                    StartTimer();
                    if (p == KeyType.CloseKey)
                    {
                        ShowMask(false);
                        ShowFullInputBoard = false;
                    }
                }
            }, 0.1, pt =>
                {
                    ShowMask(false);
                    ShowFullInputBoard = true;

                });
        }
    }
}
