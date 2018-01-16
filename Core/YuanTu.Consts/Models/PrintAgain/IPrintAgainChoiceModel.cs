using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;

namespace YuanTu.Consts.Models.PrintAgain
{
    public interface IPrintAgainModel : IModel
    {
        PrintAgainTypeEnum PrintAgainType { get; set; }

        List<凭条记录> Slips { get; set; }
    }

    public class PrintAgainModel : ModelBase, IPrintAgainModel
    {
        public PrintAgainTypeEnum PrintAgainType { get; set; }
        public List<凭条记录> Slips { get; set; }
    }
}
