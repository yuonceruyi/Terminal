using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.VirtualHospital.Component.WaiYuan.Models
{
    public interface IWaiYuanModel:IModel
    {
        string PatientName { get;}
        string CardNo { get; set; }
        string IDCardNo { get; }
        string Phone { get; set; }
        string Password { get; set; }
        string Address { get; set; }
        req病人信息查询 Req病人信息查询_外院 { get; set; }
        病人信息 病人信息_外院 { get; set; }
    }

    public class WaiYuanModel : ModelBase,IWaiYuanModel
    {
        private string _cardNo;
        private string _idCardNo;
        private string _phone;
        private string _password;
        private string _address;
        private string _patientName;

        public string PatientName => 病人信息_外院?.name;

        public string CardNo
        {
            get { return _cardNo; }
            set { _cardNo = value; OnPropertyChanged();}
        }

        public string IDCardNo => 病人信息_外院?.idNo;

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; OnPropertyChanged(); }
        }

        public req病人信息查询 Req病人信息查询_外院 { get; set; }

        public 病人信息 病人信息_外院 { get; set; }
    }
}
