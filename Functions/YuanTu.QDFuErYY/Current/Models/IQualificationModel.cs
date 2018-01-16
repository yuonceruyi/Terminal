using System.Collections.Generic;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.QDFuErYY.Current.Models;

namespace YuanTu.QDFuErYY.Current.Models
{
    public class res获取执业资格信息
    {
        public List<执业资格记录> data { get; set; }
    }

    public interface IQualificationModel : IModel
    {
        res获取执业资格信息 Res获取执业资格信息 { get; set; }

        List<执业资格记录> 当前执业资格信息 { get; set; }
    }

    public class QualificationModel: ModelBase,IQualificationModel
    {
        public res获取执业资格信息 Res获取执业资格信息 { get; set; }

        public List<执业资格记录> 当前执业资格信息 { get; set; }
    }
}

