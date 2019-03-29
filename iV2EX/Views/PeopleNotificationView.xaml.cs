using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using AngleSharp.Html.Parser;
using System.Collections.Generic;

namespace iV2EX.Views
{
    public partial class PeopleNotificationView
    {
        private List<IDisposable> _events;

        public IncrementalLoadingCollection<NotificationModel> NotifyData { get; } = new IncrementalLoadingCollection<NotificationModel>();
        public PeopleNotificationView()
        {
            InitializeComponent();
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(NotificationList, nameof(NotificationList.ItemClick))
                .Select(x => x.EventArgs.ClickedItem as NotificationModel)
                .ObserveOnDispatcher()
                .Subscribe(x => PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), new Tuple<int, int>(x.Topic.Id, x.ReplyFloor)));
            NotifyData.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetNotifications(NotifyData.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                var header = dom.GetElementById("Main").QuerySelector("div.header");
                var messages = int.Parse(header.QuerySelector("strong.gray")?.TextContent ?? "0");
                var pages = messages % 10 != 0 ? messages / 10 + 1 : messages / 10;
                var notifications = dom.GetElementById("Main").GetElementsByClassName("cell")
                    .Where(node => node.Id != null).Select(
                        node =>
                        {
                            var hrefs = node.QuerySelectorAll("a");
                            var linkPieces = hrefs[2].GetAttribute("href").Split('/', '#');
                            return new NotificationModel
                            {
                                Topic = new TopicModel
                                {
                                    Id = int.Parse(linkPieces[2])
                                },
                                Member = new MemberModel
                                {
                                    Image = $"http:{node.QuerySelector("img").GetAttribute("src")}"
                                },
                                Id = int.Parse(node.Id.Replace("n_", "")),
                                Title = node.QuerySelector("span.fade").TextContent,
                                ReplyDate = node.QuerySelector("span.snow").TextContent,
                                Content = node.QuerySelector("div.payload")?.TextContent,
                                ReplyFloor = int.Parse(linkPieces[3].Replace("reply", ""))
                            };
                        });
                return new PagesBaseModel<NotificationModel>
                {
                    Pages = pages,
                    Entity = notifications
                };
            };

            _events = new List<IDisposable> { click };
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            _events.ForEach(x => x.Dispose());
        }
    }
}