using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.Register
{
    public interface IRegTypesModel : IModel
    {
        //string SelectRegTypeId { get; set; }
        string SelectRegTypeName { get; set; }

        RegTypeDto SelectRegType { get; set; }
    }

    public class DefaultRegTypesModel : ModelBase, IRegTypesModel
    {
        //public string SelectRegTypeId { get; set; }
        public string SelectRegTypeName { get; set; }

        public RegTypeDto SelectRegType { get; set; }
    }
}