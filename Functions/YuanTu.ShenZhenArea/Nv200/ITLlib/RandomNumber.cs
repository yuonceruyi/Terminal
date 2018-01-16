using System.Security.Cryptography;

namespace YuanTu.ShenZhenArea.Nv200.ITLlib
{
    internal class RandomNumber
    {
        public const ulong MAX_RANDOM_INTEGER = 2147483648;
        public const ulong MAX_PRIME_NUMBER = 2147483648;
        private readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public ulong GenerateRandomNumber()
        {
            ulong num = 0;
            var data = new byte[8];
            rngCsp.GetBytes(data);
            for (byte index = 0; (int) index < 8; ++index)
                num += (ulong) data[index] << (8 * index);
            return num;
        }

        public ulong GeneratePrime()
        {
            var n = GenerateRandomNumber() % 2147483648UL;
            if (((long) n & 1L) == 0L)
                ++n;
            while (MillerRabin(n, 5) == primality.COMPOSITE)
                n += 2UL;
            return n;
        }

        private primality MillerRabin(ulong n, ushort trials)
        {
            for (ushort index = 0; (int) index < (int) trials; ++index)
            {
                var a = GenerateRandomNumber() % (n - 3UL) + 2UL;
                if (SingleMillerRabin(n, a) == primality.COMPOSITE)
                    return primality.COMPOSITE;
            }
            return primality.PSEUDOPRIME;
        }

        private primality SingleMillerRabin(ulong n, ulong a)
        {
            ushort num1 = 0;
            var y = n - 1UL;
            while (((long) y & 1L) == 0L)
            {
                ++num1;
                y >>= 1;
            }
            if (num1 == 0)
                return primality.COMPOSITE;
            var num2 = XpowYmodN(a, y, n);
            if ((long) num2 == 1L || (long) num2 == (long) n - 1L)
                return primality.PSEUDOPRIME;
            for (ushort index = 1; (int) index < (int) num1; ++index)
            {
                num2 = num2 * num2 % n;
                if ((long) num2 == 1L)
                    return primality.COMPOSITE;
                if ((long) num2 == (long) n - 1L)
                    return primality.PSEUDOPRIME;
            }
            return primality.COMPOSITE;
        }

        public ulong XpowYmodN(ulong x, ulong y, ulong N)
        {
            var num1 = x;
            ulong num2 = 1;
            while ((long) y != 0L)
            {
                if (((long) y & 1L) != 0L)
                    num2 = num2 * num1 % N;
                num1 = num1 * num1 % N;
                y >>= 1;
            }
            return num2;
        }

        private enum primality
        {
            COMPOSITE,
            PSEUDOPRIME
        }
    }
}