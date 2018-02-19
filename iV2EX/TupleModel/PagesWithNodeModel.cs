using iV2EX.Model;

namespace iV2EX.TupleModel
{
    public class PagesWithNodeModel<T> : PagesBaseModel<T>
    {
        public NodeModel Node { get; set; }
    }
}