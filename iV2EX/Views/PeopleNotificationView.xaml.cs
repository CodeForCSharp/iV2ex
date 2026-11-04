using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

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
                var pages = ParseNotificationPages(dom);
                var notifications = dom.QuerySelectorAll("#notifications > div.cell")
                    .Where(node => !string.IsNullOrEmpty(node.Id) && node.Id.StartsWith("n_")).Select(
                        node =>
                        {
                            var topicLink = node.QuerySelector("a.topic-link");
                            var topicHref = topicLink?.GetAttribute("href") ?? "";
                            var linkPieces = topicHref.Split(new[] { '/', '#' }, StringSplitOptions.RemoveEmptyEntries);
                            var topicId = linkPieces.Length > 1 && int.TryParse(linkPieces[1], out var parsedId)
                                ? parsedId
                                : 0;
                            var replyFloor = 0;
                            if (linkPieces.Length > 2)
                                int.TryParse(linkPieces[2].Replace("reply", ""), out replyFloor);
                            var avatar = node.QuerySelector("img.avatar");
                            return new NotificationModel
                            {
                                Topic = new TopicModel
                                {
                                    Id = topicId,
                                    Title = topicLink?.TextContent
                                },
                                Member = new MemberModel
                                {
                                    Username = avatar?.GetAttribute("alt"),
                                    Image = avatar?.GetAttribute("src")
                                },
                                Id = int.TryParse(node.Id.AsSpan(2), out var notificationId) ? notificationId : 0,
                                Title = node.QuerySelector("span.gray")?.TextContent?.Trim(),
                                ReplyDate = node.QuerySelector("span.snow")?.TextContent?.Trim(),
                                Content = node.QuerySelector("div.payload")?.TextContent?.Trim(),
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

        // Notifications are 50 per page. Prefer the pager's max so a site-side page size
        // change does not send incremental loading past the last real page.
        private static int ParseNotificationPages(IHtmlDocument dom)
        {
            var pageInput = dom.QuerySelector("input.page_input");
            if (int.TryParse(pageInput?.GetAttribute("max"), out var maxFromInput) && maxFromInput > 0)
                return maxFromInput;

            var lastPageLink = 0;
            foreach (var link in dom.QuerySelectorAll(".ps_container a.page_normal, .ps_container a.page_current"))
            {
                if (int.TryParse(link.TextContent, out var n) && n > lastPageLink)
                    lastPageLink = n;
            }
            if (lastPageLink > 0)
                return lastPageLink;

            var totalText = dom.QuerySelector("#Main div.header strong.gray")?.TextContent
                            ?? dom.QuerySelector("div.header strong.gray")?.TextContent;
            if (int.TryParse(totalText, out var total) && total > 0)
                return (total + 49) / 50;

            return 1;
        }
    }
}
