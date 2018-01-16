using System;
using System.Collections.Generic;
using YuanTu.ShengZhouHospital.ISO8583.CPUCard;

namespace YuanTu.ShengZhouHospital.ISO8583.Data
{
    public class Message
    {
        public int Length { get; set; }

        public string TPDU { get; set; }

        public string Head { get; set; }

        public string MessageType { get; set; }

        public string Bitmap { get; set; }

        public Dictionary<int, FieldData> Fields { get; } = new Dictionary<int, FieldData>();

        public FieldData this[int id]
        {
            get
            {
                if (Fields?.ContainsKey(id)??false)
                {
                return Fields[id];

                }
                return default(FieldData);
            }
        }

        /// <summary>
        /// 48��TLV����
        /// </summary>
        public Dictionary<int, TlvPackageData> Packages { get; set; }


        /// <summary>
        /// 55������
        /// </summary>
        public Dictionary<int, CPUTlv> ICPackages { get; set; }


        /// <summary>
        /// 62��TLV����
        /// </summary>
        public List<Tuple<int, TlvPackageData>> PackageList { get; set; }


        /// <summary>
        /// 62�� ���� ��Կ ���ط��ر��
        /// </summary>
        public bool HasMore { get; set; }
    }
}