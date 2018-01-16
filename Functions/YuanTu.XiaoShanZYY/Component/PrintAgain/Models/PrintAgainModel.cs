using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.PrintAgain.Models
{
    public interface IPrintAgainModel : IModel
    {
        Res补打查询 Res补打查询 { get; set; }
    }

    internal class PrintAgainModel : ModelBase, IPrintAgainModel
    {
        public Res补打查询 Res补打查询 { get; set; }
    }
}