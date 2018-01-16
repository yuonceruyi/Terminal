using System;
using System.Windows.Media;
using Prism.Commands;

namespace YuanTu.Consts.Models
{
    public class Info
    {
        public virtual string TemplateKey { get; set; }
        public string Title { get; set; }
        public int No { get; set; }
        public string SubTitle { get; set; }
        public object Tag { get; set; }

        public override string ToString()
        {
            return $"[{No}] {Title}";
        }

        public DelegateCommand<Info> ConfirmCommand { get; set; }

        public string DisableText { get; set; } = "即将开放";
    }

    public class InfoCard : Info
    {
        public Uri IconUri { get; set; }
    }

    public class InfoIcon : Info
    {
        public Uri IconUri { get; set; }

        public Color Color { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class InfoPay : InfoIcon
    {
        public bool ShowHint { get; set; }
        public string HintText { get; set; }
    }

    public class InfoMore : Info
    {
        public string Type { get; set; }

        public decimal? Amount { get; set; }

        /// <summary>
        /// 存放额外的排班信息
        /// </summary>
        public string Extends { get; set; }

        public bool IsEnabled { get; set; } = true;
    }

    public class InfoType : InfoIcon
    {
        public string Remark { get; set; }
    }

    public class InfoAppt : Info
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string Department { get; set; }
        public string Doctor { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class InfoDoc : Info
    {
        public Uri IconUri { get; set; }
        public string Rank { get; set; }
        public decimal? Amount { get; set; }
        public int? Remain { get;set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class InfoTime : Info
    {
        public string Time { get; set; }
    }

    public class InfoHouseReport : Info
    {
        public string Date { get; set; }
        public string Result { get; set; }
        public Uri ImageSource { get; set; }
    }

    public class InfoHospital : Info
    {
        public string IconUri { get; set; }
        public string CorpTags { get; set; }
        public string Address { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}