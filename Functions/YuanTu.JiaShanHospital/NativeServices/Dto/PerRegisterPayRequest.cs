namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    /// <summary>
    /// Ԥ��������
    /// </summary>
    public class PerRegisterPayRequest: RequestBase
    {
       
        /// <summary>
        /// �Ű�ID
        /// </summary>
        public string PaiBanId { get; set; }
        /// <summary>
        /// �������־
        /// </summary>
        public DayTimeFlag TimeFlag { get; set; }
        /// <summary>
        /// ֧����ʽ
        /// </summary>
        public PayMedhodFlag PayFlag { get; set; }
       

        public override int BussinessType { get; set; } = 3;
    }
}