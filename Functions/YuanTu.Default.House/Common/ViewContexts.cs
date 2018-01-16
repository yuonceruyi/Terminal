using System;
using System.Collections.Generic;

namespace YuanTu.Default.House.Common
{
    public static class ViewContexts
    {
        public class ViewContext
        {
            public string Address { get; set; }
            public string Title { get; set; }
            public Type Type { get; set; }
        }

        public static List<ViewContext> ViewContextList { get; set; } = new List<ViewContext>();
    }
}