using System.Collections.Generic;

namespace iV2EX.Model
{
    public class TopicModel
    {
        public string CreateDate { get; set; }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        public int Replies { get; set; }

        public MemberModel Member { get; set; }

        public string NodeName { get; set; }

        public string LastUsername { get; set; }

        public string LastReply { get; set; }

        public string Collect { get; set; }

        public List<TopicModel> Postscript { get; set; }
    }
}