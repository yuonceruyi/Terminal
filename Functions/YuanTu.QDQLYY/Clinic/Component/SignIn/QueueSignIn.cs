using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CardReader;
using YuanTu.QDArea.QingDaoSiPay;
using YuanTu.QDArea.QueueSignIn;

namespace YuanTu.QDQLYY.Clinic.Component.SignIn
{
    public interface ISignInService : IService
    {

    }
    public class QueueSignIn : ViewModelBase, ISignInService
    {
        public QueueSignIn()
        {            
            IRFCardReader[] rfCardReaders = GetInstance<IRFCardReader[]>();
            rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
            IIcCardReader[] icCardReaders = GetInstance<IIcCardReader[]>();
            icCardReader = icCardReaders.FirstOrDefault(p => p.DeviceId == "HuaDa_IC");
          
            IEventAggregator eventAggregator = GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(ViewIsChanged);
        }
        
        private bool isHome,connect;
        public override string Title => throw new NotImplementedException();
        IRFCardReader rfCardReader;
        protected IIcCardReader icCardReader;
        public string ServiceName { get; }               

        string cardNo,oldCardNo;
        CardType cardType;
     
        protected virtual void ViewIsChanged(ViewChangeEvent eveEvent)
        {
            if (!connect)
            {
                Init();
            }
            var engine = NavigationEngine;
            isHome = engine.IsHome(eveEvent.To);
            if (isHome)
            {
                Task.Run(() => { StartReadCard(); });               
            }
        }

         void Init()
        {
            var result = SignInService.Init();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "初始化", "初始化失败\n" + result.Message);
            }
            else
            {
                this.View = Application.Current.MainWindow;
                connect = true;
            }
        }
        protected virtual void StartReadCard()
        {
            string idNo = string.Empty, name = string.Empty;

            while (isHome)
            {
                var rest = CheckRfCard();
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    if (cardNo == oldCardNo)
                    {  
                        Thread.Sleep(300);
                        continue;
                    }
                    cardNo = rest;
                    cardType = CardType.就诊卡;
                    SignIn();
                    break;
                }
                if (CheckSiCard())
                {
                    var sign = Function.ReadCard(ref idNo, ref name);
                    //if (sign != 0)
                    //{
                    //    ShowConfirm("读卡",
                    //         $"医保卡 读卡失败" + Function.ErrMsg,
                    //      cb =>
                    //      {
                    //          StartReadCard();
                    //      });
                    //    Logger.Device.Info("医保卡 读卡失败");
                    //    break;
                    //}
                    //else
                    {
                        if (cardNo == oldCardNo)
                        {
                            Thread.Sleep(300);
                            continue;
                        }
                        cardNo = idNo;
                        cardType = CardType.社保卡;
                        SignIn();
                        break;
                    }
                }
                else
                {
                    oldCardNo = string.Empty;
                }
                Thread.Sleep(300);
            }          
        }
      
        private string CheckRfCard()
        {
            string track = null;
            var ret = rfCardReader.Connect();
            if (!ret.IsSuccess)
            {
                return track;
            }
            var rest = rfCardReader.GetCardId();
            if (rest.IsSuccess)
            {
                track = BitConverter.ToUInt32(rest.Value, 0).ToString();
            }

            return track;
        }

        private bool CheckSiCard()
        {
            try
            {
                icCardReader.Connect();
                var result = icCardReader.PowerOn(SlotNo.大卡座);
                if (!result.IsSuccess)
                {
                    return false;
                }
            }
            finally
            {
                try
                {
                    icCardReader.DisConnect();
                }
                catch
                {

                }
            }
            return true;
        }

        private void SignIn()
        {
            DoCommand(lp =>
            {              
                lp.ChangeText("正在查询排班信息，请稍候...");
                oldCardNo = cardNo;
                var list = SignInService.QueryQueue(cardNo, cardType);
                if ((list?.Count??0) == 0)
                {
                    ShowAlert(false, "签到失败", "获取队列信息失败\n"+ SignInService.Msg);
                }
                else if (list.Count == 1)
                { 
                    var TakeNo = SignInService.TakeNo(cardNo, cardType, list[0].queueCode);
                    if (TakeNo==null)
                    {
                        ShowAlert(false, "签到", "签到失败\n" + SignInService.Msg);
                    }
                    else
                    {                         
                        ShowAlert(true, "签到成功", TakeNo.username+"\n"+TakeNo.orderNoTag+"号");
                    }
                }
                else
                {
                    var CardModel = GetInstance<ICardModel>();
                    var Queues = GetInstance<IQueues>();
                    CardModel.CardNo = cardNo;
                    CardModel.CardType = cardType;
                    Queues.list = list;
                    Next();
                }
               
                Task.Run(() => { StartReadCard(); });                
            });
        }
        protected virtual void Next()
        {        
            NavigationEngine.JumpAfterFlow(null, null,
                new FormContext(AInner.QueneSelect_Context, AInner.PDJH.Select), "排队叫号签到");
        }
    }
}
