using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Devices.CardReader;

namespace YuanTu.HuNanHangTianHospital.Card
{
    /// <summary>
    /// M1卡读写
    /// </summary>
    public class M1DispenserRW
    {
        private static byte[] KeyValue = new byte[6] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        private static IRFCardDispenser rfCardDispenser;
        public static IRFCardDispenser RfCardDispenser
        {
            get
            {
                return rfCardDispenser;
            }

            set
            {
                rfCardDispenser = value;
            }
        }

        /// <summary>
        /// 读取位置 第二扇区，第0块 密码ffffffffffff,编码格式ff078069
        /// </summary>
        public static void ReadCommChunk0(out string CardNo)
        {
            CardNo = string.Empty;
            byte[] data;
            Result<byte[]> result = rfCardDispenser.ReadBlock(2, 0, false, KeyValue);
            if (!result.IsSuccess)
            {
                return;
            }
            data = result.Value;
            CardNo = Encoding.Default.GetString(data,0,16);
        }
    }
}
