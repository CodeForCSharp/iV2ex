using System;
using Microsoft.UI.Xaml;
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
            NotifyData.LoadingStateChanged += isLoading =>
            {
                if (isLoading)
                {
                    if (NotifyData.Count == 0)
                    {
                        LoadingRing.IsActive = true;
                        LoadingRing.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LoadingBar.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    LoadingRing.IsActive = false;
                    LoadingRing.Visibility = Visibility.Collapsed;
                    LoadingBar.Visibility = Visibility.Collapsed;
                }
            };
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
