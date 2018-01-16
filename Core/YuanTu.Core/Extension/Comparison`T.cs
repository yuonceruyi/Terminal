using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Core.Extension
{
    public static class Comparison<T>
    {
        public static IComparer<T> CreateComparer<V>(Func<T, V> keySelector)
        {
            return new CommonComparer<V>(keySelector);
        }

        public static IComparer<T> CreateComparer<V>(Func<T, V> keySelector, IComparer<V> comparer)
        {
            return new CommonComparer<V>(keySelector, comparer);
        }

        private class CommonComparer<V> : IComparer<T>
        {
            private readonly IComparer<V> _comparer;
            private readonly Func<T, V> _keySelector;

            public CommonComparer(Func<T, V> keySelector, IComparer<V> comparer)
            {
                _keySelector = keySelector;
                _comparer = comparer;
            }

            public CommonComparer(Func<T, V> keySelector)
                : this(keySelector, Comparer<V>.Default)
            {
            }

            public int Compare(T x, T y)
            {
                return _comparer.Compare(_keySelector(x), _keySelector(y));
            }
        }
    }
}
