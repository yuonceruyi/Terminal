using System.Linq;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Auth
{
    public interface IPatientModel : IModel
    {
        /// <summary>
        ///     病人门诊信息查询的入口
        /// </summary>
        req病人信息查询 Req病人信息查询 { get; set; }

        /// <summary>
        ///     病人门诊信息查询的结果
        /// </summary>
        res病人信息查询 Res病人信息查询 { get; set; }

        /// <summary>
        ///     病人门诊信息结果中的索引
        /// </summary>
        int PatientInfoIndex { get; set; }

        /// <summary>
        ///     病人基本信息修改的入口
        /// </summary>
        req病人基本信息修改 Req病人基本信息修改 { get; set; }

        /// <summary>
        ///     病人基本信息修改的结果
        /// </summary>
        res病人基本信息修改 Res病人基本信息修改 { get; set; }

        病人信息 当前病人信息 { get; }

        req住院患者信息查询 Req住院患者信息查询 { get; set; }
        res住院患者信息查询 Res住院患者信息查询 { get; set; }
        住院患者信息 住院患者信息 { get; }

        req诊疗卡密码校验 Req诊疗卡密码校验 { get; set; }
        res诊疗卡密码校验 Res诊疗卡密码校验 { get; set; }
    }

    public class DefaultPatientModel : ModelBase, IPatientModel
    {
        public req病人信息查询 Req病人信息查询 { get; set; }
        public res病人信息查询 Res病人信息查询 { get; set; }
        public int PatientInfoIndex { get; set; }
        public req病人基本信息修改 Req病人基本信息修改 { get; set; }
        public res病人基本信息修改 Res病人基本信息修改 { get; set; }
        public req住院患者信息查询 Req住院患者信息查询 { get; set; }
        public res住院患者信息查询 Res住院患者信息查询 { get; set; }

        public 病人信息 当前病人信息
        {
            get
            {
                if (Res病人信息查询?.data != null && Res病人信息查询.data.Any() && Res病人信息查询.data.Count > PatientInfoIndex)
                {
                    return Res病人信息查询.data[PatientInfoIndex];
                }
                return null;
            }
        }
        public 住院患者信息 住院患者信息
        {
            get
            {
                return Res住院患者信息查询?.data;
            }
        }

        public req诊疗卡密码校验 Req诊疗卡密码校验 { get; set; }
        public res诊疗卡密码校验 Res诊疗卡密码校验 { get; set; }
    }
}