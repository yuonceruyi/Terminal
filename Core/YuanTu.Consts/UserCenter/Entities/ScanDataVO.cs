using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class ScanDataVO
    {
        public String url { get; set; }

        public String uuid { get; set; }

        public int timeout { get; set; } = 120;

        public int status { get; set; }

        public DateTime? gmtCreate { get; set; }

        /// <summary>
        /// 就诊人id
        /// </summary>
        public long patientId { get; set; }

        public List<PatientVO> patientList { get; set; }
    }
}
