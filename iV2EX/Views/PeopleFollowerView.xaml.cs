using System;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using AngleSharp.Html.Parser;

namespace iV2EX.Views
{
    public sealed partial class PeopleFollowerView
    {
        public PeopleFollowerView()
        {
            InitializeComponent();
            NotifyData.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetFollowerTopics(NotifyData.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                return new PagesBaseModel<TopicModel>
                {
                    Pages = DomParse.ParseMaxPage(dom),
                    Entity = DomParse.ParseTopics(dom)
                };
            };
            PeopleFollowerList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TopicModel item)
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
            };
        }

        private IncrementalLoadingCollection<TopicModel> NotifyData { get; } = new IncrementalLoadingCollection<TopicModel>();
    }
}
