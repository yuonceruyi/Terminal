using System;
using System.Linq;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;
using YuanTu.ZheJiangHospital.HIS;
using YuanTu.ZheJiangHospital.ICBC;

namespace YuanTu.ZheJiangHospital.Component.Auth.ViewModels
{
    public class SiCardViewModel : Default.Component.Auth.ViewModels.SiCardViewModel
    {
        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        public override void OnSet()
        {
            BackUri = ResourceEngine.GetImageResourceUri("社保卡机器");
            CardUri = ResourceEngine.GetImageResourceUri("动画素材_社保卡");
        }

        [Dependency]
        public IAuthModel Auth { get; set; }
        [Dependency]
        public IAccountModel Account { get; set; }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void Confirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在读卡，请稍候...");

                try
                {
                    var result = Z9.Open();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "读卡失败", "读卡器连接失败");
                        return;
                    }
                    var resetResult = Z9.Reset();
                    if (!resetResult.IsSuccess)
                    {
                        ShowAlert(false, "读卡失败", "未找到卡, 请先插卡");
                        return;
                    }
                }
                finally
                {
                    Z9.Close();
                }

                var create = ChoiceModel.Business == Business.建档;

                string type = string.Empty;
                switch (CardModel.CardType)
                {
                    case CardType.身份证:
                        type = "1";
                        break;

                    case CardType.市医保卡:
                        type = "2";
                        break;

                    case CardType.省医保卡:
                        type = "3";
                        break;
                }
                if (create)
                {
                    var req = new Req建档读卡()
                    {
                        医保类别 = type
                    }; 
                    var res = DataHandler.RunExe<Res建档读卡>(req);

                    if (!res.Success)
                    {
                        ShowAlert(false, "读卡失败", $"读卡失败:{res.Message}");
                        return;
                    }
                    Auth.Res建档读卡 = res;

                    Next();
                }
                else
                {
                    var req =  new Req读卡()
                    {
                        医保类别 = type
                    };
                    var res = DataHandler.RunExe<Res读卡>(req);

                    if (!res.Success)
                    {
                        ShowAlert(false, "读卡失败", $"读卡失败:{res.Message}");
                        return;
                    }
                    Auth.Res读卡 = res;

                    GetPatientInfo(res.门诊号);
                }
            });
        }

        private void GetPatientInfo(string patientId)
        {
            var result = DataHandler.GetPatient(patientId);

            if (!result.IsSuccess)
            {
                ShowAlert(false, "查询个人信息失败", result.Message);
                return;
            }
            var infos = result.Value;
            if (infos.Count == 0)
            {
                ShowAlert(false, "查询个人信息失败", "未找到对应信息 请先建档");
                return;
            }
            if (infos.Count > 1)
            {
                ShowAlert(false, "查询个人信息失败", "多条信息无法区分");
                return;
            }
            Auth.Info = infos.First();

            Account.HasAccount = false;
            var accountResult = DataHandler.GetAccount(Auth.Info.IDNO);
            if (accountResult.IsSuccess)
            {
                var accounts = accountResult.Value;
                if (accounts.Any())
                {
                    var valid = accounts.Where(a => a.KHBZ == "1").ToList();
                    if (valid.Count == 1)
                    {
                        Account.HasAccount = true;
                        Account.AccountId = $"{valid[0].BRXM}^{valid[0].SFZH}";
                        Account.AccountNo = valid[0].XNZH;
                    }
                }
            }

            if (Account.HasAccount)
            {
                Account.Res查询虚拟账户余额 = null;
                Account.Req查询虚拟账户余额 = new Req查询虚拟账户余额()
                {

                    Chanel = "1",
                    AccountNo = Account.AccountNo,
                    AccountId = Account.AccountId,

                    OperId = FrameworkConst.OperatorId,
                    DeviceInfo = FrameworkConst.OperatorId,
                    TradeSerial = BalanceDeal.TradeSerial,

                    Rsv1 = "",
                    Rsv2 = "",
                };

                var qResult = PConnection.Handle<Res查询虚拟账户余额>(Account.Req查询虚拟账户余额);
                if (qResult.IsSuccess)
                {
                    var res = qResult.Value;
                    Account.Res查询虚拟账户余额 = res;
                    Account.Balance = Convert.ToDecimal(res.Remain);
                }
                else
                {
                    Account.Balance = -1;
                }
                //Account.Res查询虚拟账户余额 = null;
                //Account.Req查询虚拟账户余额 = new IcbcRequest()
                //{
                //    Chanel = "1",
                //    AccountNo = Account.AccountNo,
                //    AccountId = Account.AccountId,

                //    OperId = FrameworkConst.OperatorId,
                //    DeviceInfo = FrameworkConst.OperatorId,
                //    TradeSerial = BalanceDeal.TradeSerial,

                //    Rsv1 = "",
                //    Rsv2 = "",
                //};
                //var res = PDll.Query(Account.Req查询虚拟账户余额);
                //if (res.Success)
                //{
                //    Account.Res查询虚拟账户余额 = res;
                //    Account.Balance = res.Balance;
                //}
                //else
                //{
                //    Account.Balance = -1;
                //}
            }

            Next();
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试门诊号");
            if (!ret.IsSuccess)
                return;
            DoCommand(lp => { GetPatientInfo(ret.Value); });
        }
    }
}