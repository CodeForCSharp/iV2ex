using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
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
        public IncrementalLoadingCollection<NotificationModel> NotifyData { get; } = new IncrementalLoadingCollection<NotificationModel>();
        public PeopleNotificationView()
        {
            InitializeComponent();
            NotificationList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is NotificationModel item)
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), new Tuple<int, int>(item.Topic.Id, item.ReplyFloor));
            };
            NotifyData.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetNotifications(NotifyData.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                var header = dom.GetElementById("Main").QuerySelector("div.header");
                var messages = int.Parse(header.QuerySelector("strong.gray")?.TextContent ?? "0");
                var pages = messages % 10 != 0 ? messages / 10 + 1 : messages / 10;
                var notifications = dom.QuerySelectorAll("#notifications > div.cell")
                    .Where(node => node.Id != null).Select(
                        node =>
                        {
                            var topicHref = node.QuerySelector("a.topic-link")?.GetAttribute("href") ?? "";
                            var linkPieces = topicHref.Split(new[] { '/', '#' }, StringSplitOptions.RemoveEmptyEntries);
                            var topicId = linkPieces.Length > 1 ? int.Parse(linkPieces[1]) : 0;
                            var replyFloor = 0;
                            if (linkPieces.Length > 2)
                                int.TryParse(linkPieces[2].Replace("reply", ""), out replyFloor);
                            return new NotificationModel
                            {
                                Topic = new TopicModel
                                {
                                    Id = topicId
                                },
                                Member = new MemberModel
                                {
                                    Image = node.QuerySelector("img")?.GetAttribute("src")
                                },
                                Id = int.Parse(node.Id.Replace("n_", "")),
                                Title = node.QuerySelector("span.fade")?.TextContent,
                                ReplyDate = node.QuerySelector("span.snow")?.TextContent,
                                Content = node.QuerySelector("div.payload")?.TextContent.Trim(),
                                ReplyFloor = replyFloor
                            };
                        });
                return new PagesBaseModel<NotificationModel>
                {
                    Pages = pages,
                    Entity = notifications
                };
            };
        }
    }
}
