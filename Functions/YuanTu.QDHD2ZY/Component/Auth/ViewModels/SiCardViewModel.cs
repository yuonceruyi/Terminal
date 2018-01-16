using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Consts.Gateway;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Enums;
using YuanTu.Core.Log;
using YuanTu.QDArea.QingDaoSiPay;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;
using YuanTu.Default.Tools;

namespace YuanTu.QDHD2ZY.Component.Auth.ViewModels
{
    class SiCardViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.SiCardViewModel
    {
        protected string personNo;//个人编号
        protected string unitName;//单位名称

        public SiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {

        }

        public override void Confirm()
        {
            try
            {
                #region 读卡

                _icCardReader.Connect();
                var result = _icCardReader.PowerOn(SlotNo.大卡座);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "医保读卡", "未检测到社保卡片，请确认朝向是否正确，是否完全插入卡槽？");
                    return;
                }
                name = string.Empty;
                var sign = Function.ReadCardUnionExtend(ref idNo, ref name,ref personNo,ref unitName);
                if (sign != 0)
                {
                    Logger.Device.Info("医保卡 读卡失败");
                    ShowAlert(false, "医保读卡", "医保读卡失败" + Function.ErrMsg);
                    return;
                }

                #endregion 读卡

                var innerModel = (CardModel as Models.CardModel);

                innerModel.PersonNo = personNo;
                innerModel.UnitName = unitName;

                DoCommand(myAction);
            }
            finally
            {
                _icCardReader.DisConnect();
            }
        }
        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试医保卡号和姓名");
            if (!ret.IsSuccess)
            {
                return;
            }
            name = "医保卡激活";

            string[] input = ret.Value.Replace("\r\n", "\n").Split('\n');
            idNo = input[0];
            if (input.Length > 1)
            {
                name = input[1];
            }
                      
            string personNo = string.Empty;            
            var sign = Function.ReadCard(idNo,ref personNo);
            if (sign)
            {
                var innerModel = (CardModel as Models.CardModel);
                innerModel.PersonNo = personNo;
            }
            DoCommand(myAction);
        }
    }
}
