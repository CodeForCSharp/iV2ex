using System;
using System.Collections.Generic;

namespace iV2EX.Util
{
    public class PageInformation
    {
        public string From { get; set; }
        public string To { get; set; }
        public Type PageType { get; set; }
        public Dictionary<string, object> State { get; set; }
    }
}