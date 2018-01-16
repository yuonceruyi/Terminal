using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.InfoQuery
{
    public  interface IQueryChoiceModel:IModel
    {
      InfoQueryTypeEnum InfoQueryType { get; set; }
    }

    public class QueryChoiceModel:ModelBase,IQueryChoiceModel
    {
        public InfoQueryTypeEnum InfoQueryType { get; set; }
    }
}
