using System;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
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
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(PeopleFollowerList, nameof(PeopleFollowerList.ItemClick))
                .Select(x => x.EventArgs.ClickedItem as TopicModel)
                .ObserveOnDispatcher()
                .Subscribe(x => PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), x.Id));
            this.Unloaded += (s, e) =>
            {
                click.Dispose();
            };
        }

        private IncrementalLoadingCollection<TopicModel> NotifyData { get; } = new IncrementalLoadingCollection<TopicModel>();
    }
}