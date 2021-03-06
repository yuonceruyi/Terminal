﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using YuanTu.Consts.EventModels;
using Prism.Commands;
using YuanTu.Consts.Services;
using System.Windows.Media;
using System.Reflection;
using YuanTu.Consts;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Models
{
    public class TopBottomModel : YuanTu.Core.Models.TopBottomModel
    {
        public TopBottomModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {

            var resource = GetInstance<IResourceEngine>();
            LogoGroup = new List<ImageSource>
            {
                //resource.GetImageResource("Logo_APP"),
                //resource.GetImageResource("Logo_公众号"),
                resource.GetImageResource("Logo_银行"),
                resource.GetImageResource("Logo_远图")
            };
            MainLogo = resource.GetImageResource("MainLogo");
            HomeUri = resource.GetImageResourceUri("按钮图标_主页");
            BackUri = resource.GetImageResourceUri("按钮图标_返回");

            var assembly = Assembly.GetEntryAssembly().GetName();
            WorkVersion = $"{FrameworkConst.OperatorId}  V{assembly.Version} Beta";
            MainTitle = $"医疗自助服务系统——{FrameworkConst.OperatorId}";
            NotificMessage = FrameworkConst.NotificMessage;
        }
    }
}
