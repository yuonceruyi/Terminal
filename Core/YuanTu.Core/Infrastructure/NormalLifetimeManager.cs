using System;
using Microsoft.Practices.Unity;

namespace YuanTu.Core.Infrastructure
{
    public class NormalLifetimeManager : LifetimeManager
    {
        private readonly ScopeManager _manager;
        private object _locVal = null;
        private Guid _currentSeed;

        public NormalLifetimeManager(ScopeManager manager)
        {
            _manager = manager;
            _currentSeed = _manager.Seed;
        }

        public override object GetValue()
        {
            if (_manager.Seed != _currentSeed)
            {
                RemoveValue();
                _currentSeed = _manager.Seed;
            }
            return _locVal;
        }

        public override void SetValue(object newValue)
        {
            _locVal = newValue;
        }

        public override void RemoveValue()
        {
            _locVal = null;
        }
    }
}