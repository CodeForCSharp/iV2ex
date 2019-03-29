using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using AngleSharp.Html.Parser;

namespace iV2EX.Views
{
    public partial class PeopleTopicView
    {
        private List<IDisposable> _events;
        public PeopleTopicView()
        {
            InitializeComponent();
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(PeopleTopicsList, nameof(PeopleTopicsList.ItemClick))
                .Select(x => x.EventArgs.ClickedItem as TopicModel)
                .ObserveOnDispatcher()
                .Subscribe(x => PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), x.Id));
            NotifyData.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetFavoriteTopics(NotifyData.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                return new PagesBaseModel<TopicModel>
                {
                    Pages = DomParse.ParseMaxPage(dom),
                    Entity = DomParse.ParseTopics(dom) ?? new List<TopicModel>()
                };
            };

            _events = new List<IDisposable> { click };
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            _events.ForEach(x => x.Dispose());
        }

        private IncrementalLoadingCollection<TopicModel> NotifyData { get; } = new IncrementalLoadingCollection<TopicModel>();
    }
}