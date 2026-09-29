namespace iV2EX.Model
{
    public class PersonCenterModel
    {
        public int Gold { get; set; }

        public int Silver { get; set; }

        public int Bronze { get; set; }

        public string Notifications { get; set; }

        public string NoticePeople { get; set; }

        public string CollectedTopics { get; set; }

        public string CollectedNodes { get; set; }

        public MemberModel Member { get; set; }

        public bool IsNotChecked { get; set; }
    }
}