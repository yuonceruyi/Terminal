namespace YuanTu.YuHangSecondHospital.NativeService.Dto
{
    /// <summary>
    /// Ԥ������Ӧ���
    /// </summary>
    public class PerRegisterPay
    {
        /// <summary>
        /// ��������
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// �����ܺ�(Ԫ)
        /// </summary>
        public string TotoalPay { get; set; }
        /// <summary>
        /// ʵ�ʸ���(Ԫ)
        /// </summary>
        public string ActualPay { get; set; }
        /// <summary>
        /// �Żݽ��(Ԫ)
        /// </summary>
        public string DiscountPay { get; set; }
        /// <summary>
        /// ҽ�����(Ԫ)
        /// </summary>
        public string HealthCarePay { get; set; }
        /// <summary>
        /// Ժ���˻����(Ԫ)
        /// </summary>
        public string HospitalBalance { get; set; }
        /// <summary>
        /// �������(Ԫ)
        /// </summary>
        public string CitizenCardBalance { get; set; }
        /// <summary>
        /// Ԥ����ID
        /// </summary>
        public string PreRegisterId { get; set; }
    }
}