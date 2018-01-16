using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.ShengZhouZhongYiHospital.Component.Auth.Models;
using YuanTu.ShengZhouZhongYiHospital.HisNative.Models;

namespace YuanTu.ShengZhouZhongYiHospital
{
    public static class ConstInner
    {
        public static bool LocalHis = false;

        public static Dictionary<CardType,int>CardTypeMapping=new Dictionary<CardType, int>()
        {
            [CardType.就诊卡]=1, 
            [CardType.社保卡]=2,
        };

        public static RegTypeDto GetRegType(this RegType regType)
        {
            
            return RegTypeDto.GetInfoTypes(
               ServiceLocator.Current.GetInstance<IConfigurationManager>(),
               ServiceLocator.Current.GetInstance<IResourceEngine>(),
               "RegType",null).Select(p=> (RegTypeDto)p.Tag).FirstOrDefault(p=>p.RegType==regType);

        }

        private static Req门诊读卡 暂存读卡Req { get; set; }
        private static Res门诊读卡 暂存读卡Res { get; set; }
        private static req病人信息查询 暂存病人信息查询Req { get; set; }
        private static res病人信息查询 暂存病人信息查询Res { get; set; }
        private static CardType 暂存卡类型 { get; set; }
        private static string 暂存卡号 { get; set; }
        public static void SaveCacheData(Req门诊读卡 req, Res门诊读卡 res, req病人信息查询 reqP, res病人信息查询 resP, CardType ctype,string cardNo)
        {
            暂存读卡Req = req;
            暂存读卡Res = res;
            暂存病人信息查询Req = reqP;
            暂存病人信息查询Res = resP;
            暂存卡类型 = ctype;
            暂存卡号 = cardNo;
        }
        public static Action ClearCacheDataCallback { get; set; }

        public static bool HaveCacheData()
        {
            return 暂存读卡Req != null && 暂存读卡Res != null && 暂存病人信息查询Req != null && 暂存病人信息查询Res != null;
        }
        public static bool FillCacheData(IPatientModel patient, ICardModel cardModel)
        {
            var have = HaveCacheData();
            if (have)
            {
                var pm = patient as PatientInfoModel;
                pm.Req门诊读卡 = 暂存读卡Req;
                pm.Res门诊读卡 = 暂存读卡Res;
                pm.Req病人信息查询 = 暂存病人信息查询Req;
                pm.Res病人信息查询 = 暂存病人信息查询Res;
                cardModel.CardType = 暂存卡类型;
                cardModel.CardNo = 暂存卡号;
            }
            return have;
        }

        public static void ClearCacheData()
        {
            暂存读卡Req = null;
            暂存读卡Res = null;
            暂存病人信息查询Req = null;
            暂存病人信息查询Res = null;
            ClearCacheDataCallback?.Invoke();
        }
    }
}
