using System;
using Microsoft.Practices.Unity;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Devices.CardReader;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;
using YuanTu.ZheJiangHospital.HIS;
using YuanTu.ZheJiangHospital.ICBC;

namespace YuanTu.ZheJiangHospital.Component.Auth.ViewModels
{
    public class IDCardViewModel : Default.Component.Auth.ViewModels.IDCardViewModel
    {
        public IDCardViewModel(IIdCardReader[] idCardReaders) : base(idCardReaders)
        {
        }

        [Dependency]
        public IAuthModel Auth { get; set; }
        [Dependency]
        public IAccountModel Account { get; set; }

        protected override void OnGetInfo(string idCardNo)
        {
            DoCommand(ctx =>
            {
                if (ChoiceModel.Business == Business.建档)
                {
                    Navigate(A.CK.Info);
                    return;
                }
                var result = DataHandler.GetPatient自费(idCardNo);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息 请先到门诊窗口建档", debugInfo: result.Exception?.Message);
                    Preview();
                    return;
                }
                if (result.Value == null || result.Value.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息 请先到门诊窗口建档", debugInfo: result.Exception?.Message);
                    Preview();
                    return;
                }
                //if (result.Value.Count > 1)
                //{
                //    ShowAlert(false, "病人信息查询", "找到多条记录", debugInfo: result.Exception?.Message);
                //    StartRead();
                //    return;
                //}
                Auth.Info = result.Value.First();
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
            });
        }
    }
}