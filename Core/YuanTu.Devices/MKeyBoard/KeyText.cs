using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.MKeyBoard
{
    public class KeyText
    {
        public int KeyLength { get; set; }

        public KeyEnum KeyOrder { get; set; }

        public KeyText()
        {
            
        }

        public KeyText(int keyLength, KeyEnum keyOrder)
        {
            KeyLength = keyLength;
            KeyOrder = keyOrder;
        }
    }
}
