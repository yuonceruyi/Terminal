using System;
using Microsoft.Practices.Unity;
using Prism.Regions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;

namespace YuanTu.Default.House.Component.Create.ViewModels
{
    public class IdCardViewModel : ViewModelBase
    {
        public IIdCardReader _idCardReader;
        private bool _running;

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }
        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public IdCardViewModel(IIdCardReader[] idCardReaders)
        {
            _idCardReader = idCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_HUADA");
        }

        public override string Title { get; } = "刷身份证";

        public override void OnSet()
        {
            TipImage=ResourceEngine.GetImageResourceUri("刷身份证示例_House");
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            Tips = CreateModel.CreateType == CreateType.成人 ? "请刷本人身份证" : "请刷监护人身份证";
            StartReadCard();
        }

        protected virtual void StartReadCard()
        {
            Task.Run(() =>
            {
                try
                {
                    var result = _idCardReader.Connect();
                    if (!result.IsSuccess)
                    {
                        ReportService.读卡器离线(null, ErrorSolution.读卡器离线);
                        ShowAlert(false, "温馨提示", "读卡器离线");
                        return;
                    }
                    result = _idCardReader.Initialize();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", "读卡器初始化失败");
                        return;
                    }

                    _running = true;

                    while (_running)
                    {
                        if (_idCardReader.HasIdCard().IsSuccess)
                        {
                            var res = _idCardReader.GetIdDetail();
                            if (!res.IsSuccess)
                            {
                                ShowAlert(false, "身份证", "读取身份证信息失败,请重新放置身份证");
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                IdCardModel.Name = res.Value.Name;
                                IdCardModel.Sex = res.Value.Sex;
                                IdCardModel.IdCardNo = res.Value.IdCardNo;
                                IdCardModel.Address = res.Value.Address;
                                IdCardModel.Birthday = res.Value.Birthday;
                                IdCardModel.Nation = res.Value.Nation;
                                IdCardModel.GrantDept = res.Value.GrantDept;
                                IdCardModel.ExpireDate = res.Value.ExpireDate;
                                IdCardModel.EffectiveDate = res.Value.EffectiveDate;
                                Logger.Main.Info($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
                                _running = false;
                                ChangeNavigationContent(".");
                                Next();
                            }
                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception ex)
                {
                    _running = false;
                    ShowAlert(false,"温馨提示",$"[读取身份证信息失败] {ex.Message}");
                    Logger.Main.Info($"[读取身份证信息失败] {ex.Message} {ex.StackTrace}");
                }
               
            });
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            _running = false;
            return base.OnLeaving(navigationContext);
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试身份证号和姓名");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value.Replace("\r\n", "\n").Split('\n');
            if (list.Length < 2)
                return;
            IdCardModel.IdCardNo = list[0];
            IdCardModel.Name = list[1];

            IdCardModel.Sex = Convert.ToInt32(IdCardModel.IdCardNo[16]) % 2 == 0 ? Sex.女 : Sex.男;
            
            IdCardModel.Address = "浙江杭州西湖";
            IdCardModel.Birthday = DateTime.Parse(string.Format("{0}-{1}-{2}",
                IdCardModel.IdCardNo.Substring(6, 4), IdCardModel.IdCardNo.Substring(10, 2), IdCardModel.IdCardNo.Substring(12, 2)));
            IdCardModel.Nation = "汉";
            IdCardModel.GrantDept = "远图";
            IdCardModel.ExpireDate = DateTimeCore.Now;
            IdCardModel.EffectiveDate = DateTimeCore.Now.AddYears(10);
            Logger.Main.Debug($"[读取身份证信息成功]{IdCardModel.Name} {IdCardModel.Sex} {IdCardModel.IdCardNo}");
            ChangeNavigationContent(".");
            Navigate(AInner.JD.PatInfo);
        
            base.DoubleClick();
        }

        #region binding
        private string _tips;
        private Uri _tipImage;
        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        public Uri TipImage
        {
            get { return _tipImage; }
            set
            {
                _tipImage = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}