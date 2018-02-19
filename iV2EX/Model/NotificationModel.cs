namespace iV2EX.Model
{
    public class NotificationModel
    {
        public MemberModel Member { get; set; }

        public int Id { get; set; }

        public TopicModel Topic { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public int ReplyFloor { get; set; }

        public string ReplyDate { get; set; }
    }
}