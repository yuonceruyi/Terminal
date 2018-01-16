using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.ZheJiangZhongLiuHospital.Minghua
{
    public interface IYiBaoCardContent : IModel
    {
        string Birthday { get; set; }
        string Birthplace { get; set; }
        string CardData { get; set; }
        string CardId { get; set; }
        string CardNume { get; set; }
        string CardType { get; set; }
        string CardValidData { get; set; }
        string Fkjg { get; set; }
        string GfVer { get; set; }
        string PId { get; set; }
        string Name { get; set; }
        string Nation { get; set; }
        string Sex { get; set; }
    }

    public class YiBaoCardContent : ModelBase, IYiBaoCardContent
    {
        public string Birthday { get; set; }
        public string Birthplace { get; set; }
        public string CardData { get; set; }
        public string CardId { get; set; }
        public string CardNume { get; set; }
        public string CardType { get; set; }
        public string CardValidData { get; set; }
        public string Fkjg { get; set; }
        public string GfVer { get; set; }
        public string PId { get; set; }
        public string Name { get; set; }
        public string Nation { get; set; }
        public string Sex { get; set; }
    }
}
