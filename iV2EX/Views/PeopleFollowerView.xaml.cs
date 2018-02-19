using System;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using AngleSharp.Parser.Html;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;

namespace iV2EX.Views
{
    public sealed partial class PeopleFollowerView
    {
        public PeopleFollowerView()
        {
            InitializeComponent();
            var client = ApiClient.Client;
            NotifyData.LoadDataTask = async count =>
            {
                var html = await client.GetFollowerTopics(NotifyData.CurrentPage);
                var dom = new HtmlParser().Parse(html);
                return new PagesBaseModel<TopicModel>
                {
                    Pages = V2ExManager.ParseMaxPage(dom),
                    Entity = V2ExManager.ParseTopics(dom)
                };
            };
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(PeopleFollowerList, nameof(PeopleFollowerList.ItemClick))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var item = x.EventArgs.ClickedItem as TopicModel;
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
                });
        }

        private IncrementalLoadingCollection<TopicModel> NotifyData { get; } =
            new IncrementalLoadingCollection<TopicModel>();
    }
}