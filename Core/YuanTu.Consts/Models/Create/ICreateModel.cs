using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Create
{
    public interface ICreateModel : IModel
    {
        CreateType CreateType { get; set; }
        string Phone { get; set; }
        req病人领卡查询 Req病人领卡查询 { get; set; }
        res病人领卡查询 Res病人领卡查询 { get; set; }
        req病人建档发卡 Req病人建档发卡 { get; set; }
        res病人建档发卡 Res病人建档发卡 { get; set; }
    }

    public class CreateModel : ModelBase, ICreateModel
    {
        /// <summary>
        /// 默认成人办卡
        /// </summary>
        public CreateType CreateType { get; set; } = CreateType.成人;

        public string Phone { get; set; }
        public req病人领卡查询 Req病人领卡查询 { get; set; }
        public res病人领卡查询 Res病人领卡查询 { get; set; }
        public req病人建档发卡 Req病人建档发卡 { get; set; }
        public res病人建档发卡 Res病人建档发卡 { get; set; }
    }
}