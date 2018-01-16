using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.FuYangRMYY.CardReader
{
    public class UnSafeMethods
    {
        private const string DllPathSiCard = "External\\FuYangDeCard\\SiCard.dll";

        [DllImport(DllPathSiCard, EntryPoint = "iOpenPort", CharSet = CharSet.Ansi)]
        public static extern int iOpenPort(StringBuilder outData);

        [DllImport(DllPathSiCard, EntryPoint = "iClosePort", CharSet = CharSet.Ansi)]
        public static extern int iClosePort(StringBuilder outData);

        [DllImport(DllPathSiCard, EntryPoint = "iReadM1CardNum", CharSet = CharSet.Ansi)]
        public static extern int iReadM1CardNum(StringBuilder outData, StringBuilder errMsg);
        /// <summary>
        /// 10 "卡在读卡器中"   20 "卡不在读卡器中" 30 "卡不在读卡器中,插反"  40 "其他错误"
        /// </summary>
        /// <param name="pCardCardStatus"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        [DllImport(DllPathSiCard, EntryPoint = "iGetCardStatus", CharSet = CharSet.Ansi)]
        public static extern int iGetCardStatus(StringBuilder pCardCardStatus, StringBuilder errMsg);

        [DllImport(DllPathSiCard, EntryPoint = "LPub_IC_CertCardInfos", CharSet = CharSet.Ansi)]
        public static extern int LPub_IC_CertCardInfos(string pBmpFile, StringBuilder pName, StringBuilder pSex,
            StringBuilder pNation, StringBuilder pBirth, StringBuilder pAddress, StringBuilder pCertNo,
            StringBuilder pDepartment, StringBuilder pExpire, StringBuilder pErrMsg);

        [DllImport(DllPathSiCard, EntryPoint = "iReadCard", CharSet = CharSet.Ansi)]
        public static extern int iReadCard(string pInputPin, string pFileAddr, StringBuilder pOutDataBuff,StringBuilder pErrMsg);
    }
}