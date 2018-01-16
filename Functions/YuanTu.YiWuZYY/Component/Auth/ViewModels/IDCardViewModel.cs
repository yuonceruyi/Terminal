using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.YiWuZYY.Component.Auth.ViewModels
{
    public class IDCardViewModel : YuanTu.Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_900");
        }


        protected override void StartRead()
        {
            Task.Run(() =>
            {
                try
                {
                    if (!_idCardReader.Connect().IsSuccess)
                    {
                        ReportService.身份证读卡器离线(null, ErrorSolution.身份证读卡器离线);
                        ShowAlert(false, "友好提示", "身份证读卡器打开失败");
                        return;
                    }

                    if (!_idCardReader.Initialize().IsSuccess)
                    {
                        ShowAlert(false, "友好提示", "身份证读卡器初始化失败");
                        return;
                    }
                    _working = true;
                    while (_working)
                    {
                        if (!_idCardReader.HasIdCard().IsSuccess)
                        {
                            Thread.Sleep(300);
                            continue;
                        }

                        var result = _idCardReader.GetIdDetail();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "身份证", "读取身份证信息失败,请重新放置身份证");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            if (!(CardModel.ExternalCardInfo??"").Contains("补全信息"))
                            {
                                CardModel.CardNo = result.Value.IdCardNo;
                            }
                            IdCardModel.Name = result.Value.Name;
                            IdCardModel.Sex = result.Value.Sex;
                            IdCardModel.IdCardNo = result.Value.IdCardNo;
                            IdCardModel.Address = result.Value.Address;
                            IdCardModel.Birthday = result.Value.Birthday;
                            IdCardModel.Nation = result.Value.Nation;
                            IdCardModel.GrantDept = result.Value.GrantDept;
                            IdCardModel.ExpireDate = result.Value.ExpireDate;
                            IdCardModel.EffectiveDate = result.Value.EffectiveDate;
                            Logger.Main.Info($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
                            if (_working)
                                OnGetInfo(IdCardModel.IdCardNo);
                            _working = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[读取身份证信息失败]{ex.Message + ex.StackTrace}");
                    ShowAlert(false, "身份证", "读取身份证信息失败", debugInfo: ex.Message);
                }
                finally
                {
                    _idCardReader.UnInitialize();
                    _idCardReader.DisConnect();
                }
            });
        }

        protected override void OnGetInfo(string idCardNo)
        {
            if ((CardModel.ExternalCardInfo ?? "") == "补全信息_成人")
            {
                var req = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = idCardNo,
                    cardType = ((int) CardType.身份证).ToString(),
                    patientName = IdCardModel.Name
                };
                var res = DataHandlerEx.病人信息查询(req);
                if (res.success && res.data.Any())
                {
                    ShowAlert(false, "信息重复", "该身份证已经存有档案，请到窗口处理！");
                    Navigate(A.Home);
                }
                else
                {

                    Navigate(A.CK.Info);
                }
            }
            else if ((CardModel.ExternalCardInfo ?? "") == "补全信息_监护人")
            {
                Navigate(A.CK.InfoEx);
            }
            else
            {
                base.OnGetInfo(idCardNo);

            }
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
            if (!(CardModel.ExternalCardInfo ?? "").Contains("补全信息"))
            {
                CardModel.CardNo = list[0];
            }
           
            IdCardModel.Name = list[1];
            IdCardModel.IdCardNo = list[0];
            IdCardModel.Sex = Convert.ToInt32(IdCardModel.IdCardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
           
            IdCardModel.Address = "浙江杭州西湖";
            IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
                IdCardModel.IdCardNo.Substring(6, 4), IdCardModel.IdCardNo.Substring(10, 2), IdCardModel.IdCardNo.Substring(12, 2)));
            IdCardModel.Nation = "汉";
            IdCardModel.GrantDept = "远图";
            IdCardModel.ExpireDate = DateTimeCore.Now;
            IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
            Logger.Main.Debug($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
            OnGetInfo(IdCardModel.IdCardNo);
            //base.DoubleClick();
        }
    }
}
