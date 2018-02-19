using System.Collections.Generic;
using iV2EX.Model;

namespace iV2EX.TupleModel
{
    public class NodeInGroup
    {
        public string Key { get; set; }
        public List<NodeModel> NodeContent { get; set; }
    }
}