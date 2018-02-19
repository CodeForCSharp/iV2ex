using System.Collections.Generic;

namespace iV2EX.TupleModel
{
    public class PagesBaseModel<T>
    {
        public int Pages { get; set; }

        public IEnumerable<T> Entity { get; set; }
    }
}