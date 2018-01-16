using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.FuYangRMYY.CardReader;
using YuanTu.FuYangRMYY.Component.Auth.Models;
using YuanTu.FuYangRMYY.HisNative;
using YuanTu.FuYangRMYY.Managers;

namespace YuanTu.FuYangRMYY.Component.Auth.ViewModels
{
    public class SiCardViewModel : YuanTu.Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public override void OnSet()
        {
            base.OnSet();
            SiOperatorManager.StartMonitor(null);//初始化全局监听器
        }

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders)
            : base(icCardReaders, rfCpuCardReaders)
        {
        }

        public override void Confirm()
        {
            //var errMsg = new StringBuilder(1024);
            //var ret = UnSafeMethods.iOpenPort(errMsg);
            //if (ret != 0)
            //{
            //    Logger.Device.Error($"[{Title}]连接异常，接口返回值:{ret},错误内容:{errMsg}");
            //    ShowAlert(false, "打开读卡器失败", "社保读卡器连接失败", debugInfo: errMsg.ToString());
            //    //return Result.Fail("身份证读卡器连接异常");
            //}
            //var output = new StringBuilder(1024);
            //ret = UnSafeMethods.iGetCardStatus(output, errMsg);
            //ret = UnSafeMethods.iReadCard("123456", "MF|EF05|01|02|03|04|05|06|07|$", output, errMsg);
            //ret = UnSafeMethods.iReadCard("123456", "MF|EF06|01|02|03|04|05|06|07|$", output, errMsg);
            //UnSafeMethods.iClosePort(errMsg);

            //base.Confirm();
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            RunLoop();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _working = false;
            return base.OnLeaving(navigationContext);
        }

        protected bool _working = false;

        protected void RunLoop()
        {
            Task.Run(() =>
            {
                var errMsg = new StringBuilder(1024);
                var ret = UnSafeMethods.iOpenPort(errMsg);
                if (ret != 0)
                {
                    Logger.Device.Error($"[{Title}]连接异常，接口返回值:{ret},错误内容:{errMsg}");
                    ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                    ShowAlert(false, "打开读卡器失败", "社保读卡器连接失败", debugInfo: errMsg.ToString());
                    return;
                    //return Result.Fail("身份证读卡器连接异常");
                }
                _working = true;
                var havecard = false;
                while (_working)
                {
                    try
                    {
                        havecard = false;
                        Thread.Sleep(300);
                        var output = new StringBuilder(1024);
                        ret = UnSafeMethods.iGetCardStatus(output, errMsg);
                        if (ret == 0 && output.ToString() == "10")
                        {
                            havecard = true;
                            _working = false;
                            DoCommand(lp =>
                            {
                                lp.ChangeText("正在读取社保卡内信息，请稍后...");
                                try
                                {
                                    //var input = "MF|EF05|01|02|03|04|05|06|07|$";
                                    //ret = UnSafeMethods.iReadCard("123456",input, output,errMsg);
                                    //Logger.Net.Info($"[社保读卡]状态:{ret} 入参:{input} 返回:{output} 错误消息:{errMsg}");
                                    //    //MF|EF05|341200D156000005024575A7AEB8AF87|3|2.00|915600000234120034010A0C|20150711|20250711|KD5578904|$
                                    //if (ret != 0)
                                    //{
                                    //    ShowAlert(false,"读卡失败","读卡失败，请按提示重新插入社保卡！",debugInfo:errMsg.ToString(),extend:new AlertExModel()
                                    //    {
                                    //        HideCallback = aht =>
                                    //        {
                                    //            if (aht==AlertHideType.ButtonClick)
                                    //            {
                                    //                RunLoop();
                                    //            }
                                    //            else
                                    //            {
                                    //                Navigate(A.Home);
                                    //            }
                                    //        }
                                    //    });
                                    //    return;
                                    //}
                                    //var cardno = output.ToString().Split('|')[8];//我相信专业
                                    //input = "MF|EF06|01|02|03|04|05|06|07|$";
                                    //ret = UnSafeMethods.iReadCard("123456", input, output,errMsg);
                                    //Logger.Net.Info($"[社保读卡]状态:{ret} 入参:{input} 返回:{output} 错误消息:{errMsg}");
                                    ////MF|EF06|341202198604141525|王倩倩||2|01|||$
                                    //if (ret!=0)
                                    //{
                                    //    ShowAlert(false, "读卡失败", "读卡失败，请按提示动画重新插入社保卡！", debugInfo: errMsg.ToString(), extend: new AlertExModel()
                                    //    {
                                    //        HideCallback = aht =>
                                    //        {
                                    //            if (aht == AlertHideType.ButtonClick)
                                    //            {
                                    //                RunLoop();
                                    //            }
                                    //            else
                                    //            {
                                    //                Navigate(A.Home);
                                    //            }
                                    //        }
                                    //    });
                                    //    return;
                                    //}
                                    //var cardmodel = (CardModel as CardModel);
                                    //var arr = output.ToString().Split('|');
                                    //cardmodel.SiCardInfo=new SiCardInfo()
                                    //{
                                    //    Name = arr[3],
                                    //    IdNo = arr[2],
                                    //    Sex = arr[5]=="1"?Sex.男 : Sex.女,
                                    //    Birth = (arr[2].Length==18)?(DateTime.ParseExact(arr[2].Substring(6,8),"yyyyMMdd",null)):DateTime.MinValue,
                                    //};
                                    UnSafeMethods.iClosePort(errMsg);
                                    var content = HisInsuranceService.ReadCard();
                                    if (!content)
                                    {
                                        ShowAlert(false, "读卡失败", "读卡失败，请按提示重新插入社保卡！", debugInfo: content.Message, extend: new AlertExModel()
                                        {
                                            HideCallback = aht =>
                                            {
                                                if (aht == AlertHideType.ButtonClick)
                                                {
                                                    RunLoop();
                                                }
                                                else
                                                {
                                                    Navigate(A.Home);
                                                }
                                            }
                                        });
                                        return;
                                    }
                                    var cardmodel = (CardModel as CardModel);
                                    cardmodel.SiCardInfo = new SiCardInfo();
                                    cardmodel.SiCardInfo.社保读卡 = content.Value;
                                    var cardno = Debugger.IsAttached ? content.Value.卡号 : content.Value.个人编号;
                                    OnGetInfo(lp, cardno);

                                }
                                catch(Exception ex)
                                {

                                    Logger.Main.Error($"[社保读卡]读卡过程中发生异常，{ex.Message}");
                                }
                                finally
                                {
                                    UnSafeMethods.iClosePort(errMsg);
                                }
                               
                            });
                        }
                    }
                    catch (Exception)
                    {

                    }


                }
                if (!havecard) //有卡时，由读卡逻辑内部处理
                {
                    UnSafeMethods.iClosePort(errMsg);

                }


            });
        }


        protected virtual void OnGetInfo(LoadingProcesser lp,string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效", extend: new Consts.Models.AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == AlertHideType.ButtonClick)
                                RunLoop();
                        }
                });
                return;
            }
            lp.ChangeText("正在查询病人信息，请稍后...");
            PatientModel.Req病人信息查询 = new req病人信息查询
            {
                Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                cardNo = cardNo,
                cardType = ((int)CardModel.CardType).ToString()
            };
            PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

            if (PatientModel.Res病人信息查询.success)
            {
                if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)", extend: new Consts.Models.AlertExModel
                    {
                        HideCallback =
                        p =>
                        {
                            if (p == Consts.Models.AlertHideType.ButtonClick)
                                RunLoop();
                        }
                    });

                    return;
                }
                CardModel.CardNo = cardNo;
                Next();
            }else if (!DataHandler.UnKnowErrorCode.Contains(PatientModel.Res病人信息查询.code))//常规病人信息查询失败
            {
                lp.ChangeText("获取最新信息，请稍后...");
               
                var cardmodel = (CardModel as CardModel);
                CardModel.ExternalCardInfo = "建档";
                CardModel.CardNo = cardNo;

                IdCardModel.IdCardNo = cardmodel.SiCardInfo.IdNo;
                IdCardModel.Address = cardmodel.SiCardInfo.社保读卡.公司名称;
                IdCardModel.Name = cardmodel.SiCardInfo.Name;
                IdCardModel.Sex = cardmodel.SiCardInfo.Sex;
                IdCardModel.Birthday = cardmodel.SiCardInfo.Birth;
                Next();
            }
            else
            {
                ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg, extend: new Consts.Models.AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == Consts.Models.AlertHideType.ButtonClick)
                                RunLoop();
                        }
                });

            }
        }


        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试卡号");
            if (ret.IsSuccess)
            {
                DoCommand(lp =>
                {
                    _working = false;
                    OnGetInfo(lp, ret.Value);
                });
            }
        }
    }
}
