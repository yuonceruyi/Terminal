using System;

namespace YuanTu.Core.Infrastructure
{
    public class ScopeManager
    {
        public Guid Seed { get; private set; }

        public void InitializeLifeScope()
        {
            Seed = Guid.NewGuid();
            GC.Collect();
        }
    }
}