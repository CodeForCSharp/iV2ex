using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using iV2EX.Model;
using iV2EX.TupleModel;
using WebApiClient;

namespace iV2EX.GetData
{
    internal static class V2ExManager
    {
        private const string ApiBase = "https://www.v2ex.com";
        private const string ApiNodes = "/api/nodes/all.json";

        public static async Task<IEnumerable<NodeModel>> GetNodes()
        {
            var client = HttpApiClient.Create<IV2exApi>();
            return await client.GetNodes();
        }

        public static CollectedListModel[] GetCollectedTab()
        {
            return new[]
            {
                new CollectedListModel {Text = "技术", Name = "tech"},
                new CollectedListModel {Text = "创意", Name = "creative"},
                new CollectedListModel {Text = "好玩", Name = "play"},
                new CollectedListModel {Text = "Apple", Name = "apple"},
                new CollectedListModel {Text = "酷工作", Name = "jobs"},
                new CollectedListModel {Text = "交易", Name = "deals"},
                new CollectedListModel {Text = "城市", Name = "city"},
                new CollectedListModel {Text = "问与答", Name = "qna"},
                new CollectedListModel {Text = "最热", Name = "hot"},
                new CollectedListModel {Text = "全部", Name = "all"},
                new CollectedListModel {Text = "R2", Name = "r2"},
                new CollectedListModel {Text = "节点", Name = "nodes"},
                new CollectedListModel {Text = "关注", Name = "members"}
            };
        }

        public static int ParseMaxPage(IHtmlDocument dom)
        {
            if (dom == null) return 0;
            var strong = dom.QuerySelector("strong.gray");
            var maxNumber = int.Parse(strong?.TextContent ?? "0");
            return maxNumber % 20 == 0 ? maxNumber / 20 : maxNumber / 20 + 1;
        }

        public static async Task<IEnumerable<TopicModel>> GetOnePageTopics(string nodeName, int page)
        {
            if (page < 0) return new List<TopicModel>();
            var client = ApiClient.Client;
            var html = await client.GetTopicsWithPageN(nodeName, page);
            if (string.IsNullOrEmpty(html)) return new List<TopicModel>();
            return new HtmlParser().Parse(html).GetElementById("TopicsNode").Children.Select(node =>
            {
                var hrefs = node.QuerySelectorAll("a");
                var imgs = node.QuerySelector("img.avatar");
                var topic = new TopicModel
                {
                    Title = hrefs[1].TextContent,
                    Member = new MemberModel
                    {
                        Username = hrefs[2].TextContent,
                        Image = $"http:{imgs.GetAttribute("src")}"
                    },
                    Id = int.Parse(node.ClassName.Split('_').Last())
                };
                if (hrefs.Length == 5)
                {
                    topic.LastUsername = $"最后回复者 :{hrefs[3].TextContent}";
                    topic.Replies = int.Parse(hrefs[4].TextContent);
                    var last = node.GetElementsByClassName("small fade").First().TextContent.Split('•')[1].Trim();
                    if (last.Length > 12)
                    {
                        var timeSpan = DateTime.Now - DateTime.Parse(last.Insert(10, " "));
                        last = $"{(int) timeSpan.TotalDays}天";
                    }

                    topic.LastReply = $"时间 : {last.Trim()}";
                }

                return topic;
            });
        }

        public static async Task<IEnumerable<TopicModel>> GetTopicsFromTab(string tab)
        {
            var client = ApiClient.Client;
            var html = await client.GetTopicsWithTab(tab);
            var dom = new HtmlParser().Parse(html);
            return ParseTopics(dom);
        }

        public static async Task<IEnumerable<NodeModel>> GetFavoriteNodes()
        {
            var client = ApiClient.Client;
            var html = await client.GetFavoriteNodes();
            if (string.IsNullOrEmpty(html)) return new List<NodeModel>();
            return new HtmlParser().Parse(html).GetElementById("MyNodes").GetElementsByClassName("grid_item").Select(
                child =>
                {
                    var strs = child.TextContent.Split(' ');
                    return new NodeModel
                    {
                        Id = int.Parse(child.Id.Replace("n_", "")),
                        Name = child.GetAttribute("href").Replace("/go/", ""),
                        Image = $"http:{child.QuerySelector("img").GetAttribute("src")}",
                        Title = string.Join("", strs.Take(strs.Length - 1)),
                        Topics = int.Parse(strs.Last())
                    };
                });
        }

        public static async Task<PagesBaseModel<TopicModel>> GetFavoriteTopicsAndPages(int page)
        {
            if (page < 0) return new PagesBaseModel<TopicModel> {Pages = 0, Entity = new List<TopicModel>()};
            var client = ApiClient.Client;
            var html = await client.GetFavoriteTopics(page);
            return GetTopicsAndPages(html);
        }

        public static async Task<PagesBaseModel<TopicModel>> GetFollowerTopicsAndPages(int page)
        {
            if (page < 0) return new PagesBaseModel<TopicModel> {Pages = 0, Entity = new List<TopicModel>()};
            var client = ApiClient.Client;
            var html = await client.GetFollowerTopics(page);
            return GetTopicsAndPages(html);
        }

        private static PagesBaseModel<TopicModel> GetTopicsAndPages(string html)
        {
            if (string.IsNullOrEmpty(html))
                return new PagesBaseModel<TopicModel> {Pages = 0, Entity = new List<TopicModel>()};
            var dom = new HtmlParser().Parse(html);
            return new PagesBaseModel<TopicModel>
            {
                Pages = ParseMaxPage(dom),
                Entity = ParseTopics(dom) ?? new List<TopicModel>()
            };
        }

        public static IEnumerable<TopicModel> ParseTopics(INonElementParentNode dom)
        {
            return dom.GetElementById("Main").GetElementsByClassName("cell item").Select(node =>
            {
                var hrefs = node.QuerySelectorAll("a");
                var imgs = node.QuerySelector("img.avatar");
                var topic = new TopicModel
                {
                    Title = hrefs[1].TextContent,
                    NodeName = hrefs[2].TextContent,
                    Member = new MemberModel
                    {
                        Username = hrefs[3].TextContent,
                        Image = $"http:{imgs.GetAttribute("src")}"
                    },
                    Id = int.Parse(hrefs[1].GetAttribute("href").Split('/', '#')[2])
                };
                if (hrefs.Length == 6)
                {
                    topic.LastUsername = $"{hrefs[4].TextContent}";
                    topic.Replies = int.Parse(hrefs[5].TextContent);
                    topic.LastReply =
                        $"{node.GetElementsByClassName("small fade")[0].TextContent.Split('•')[2].Trim()}";
                }

                return topic;
            });
        }

        public static async Task<PagesBaseModel<NotificationModel>> GetNotificationsAndPages(int page)
        {
            var client = ApiClient.Client;
            var html = await client.GetNotifications(page);
            if (string.IsNullOrEmpty(html))
                return new PagesBaseModel<NotificationModel> {Pages = 0, Entity = new List<NotificationModel>()};
            var dom = new HtmlParser().Parse(html);
            return new PagesBaseModel<NotificationModel>
            {
                Pages = GetNotificationPages(dom),
                Entity = GetPeopleNotifications(dom)
            };
        }

        public static IEnumerable<NotificationModel> GetPeopleNotifications(IHtmlDocument dom)
        {
            return dom.GetElementById("Main").GetElementsByClassName("cell").Where(node => node.Id != null).Select(
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
        }

        private static int GetNotificationPages(INonElementParentNode dom)
        {
            var header = dom.GetElementById("Main").QuerySelector("div.header");
            var messages = int.Parse(header.QuerySelector("strong.gray")?.TextContent ?? "0");
            return messages % 10 != 0 ? messages / 10 + 1 : messages / 10;
        }

        public static async Task<Tuple<TopicModel, IEnumerable<ReplyModel>>> GetRepliesAndTopicContent(int id)
        {
            var client = ApiClient.Client;
            var html = await client.GetRepliesAndTopicContent(id, 1);
            if (string.IsNullOrEmpty(html))
                return Tuple.Create(new TopicModel(), new List<ReplyModel>().AsEnumerable());
            var main = new HtmlParser().Parse(html).GetElementById("Main");
            var numberString = new Regex("[0-9]{1,} 回复").IsMatch(html)
                ? new Regex("[0-9]{1,} 回复").Match(html).Value.Replace(" 回复", "")
                : "0";
            var maxReply = int.Parse(numberString);
            var node = main.QuerySelector("div.header");
            var topic = new TopicModel
            {
                Id = id,
                Member = new MemberModel
                {
                    Image = $"http:{node.QuerySelector("img").GetAttribute("src")}",
                    Username = node.QuerySelector("a").GetAttribute("href").Split('/')[2]
                },
                Title = node.QuerySelector("h1").TextContent,
                Url = $"{ApiBase}/t/{id}",
                Postscript = main.QuerySelectorAll("div.subtle").Select(subtle => new TopicModel
                {
                    Content = subtle.QuerySelector("div.topic_content").InnerHtml,
                    LastReply = subtle.QuerySelector("span.fade").TextContent
                }).ToList(),
                Collect = main.TextContent.Contains("加入收藏") ? "加入\n收藏" : "已\n收藏",
                Content = main.QuerySelector("div.topic_content")?.InnerHtml,
                Replies = maxReply,
                CreateDate = node.QuerySelector("small.gray").TextContent.Split('·')[1]
            };
            if (maxReply == 0) return Tuple.Create(topic, new List<ReplyModel>().AsEnumerable());
            var replies = GetReply(topic, html);
            return Tuple.Create(topic, replies);
        }

        public static async Task<IEnumerable<ReplyModel>> GetReplies(TopicModel topic, int page)
        {
            var client = ApiClient.Client;
            return GetReply(topic, await client.GetRepliesAndTopicContent(topic.Id, page));
        }

        private static IEnumerable<ReplyModel> GetReply(TopicModel topic, string html)
        {
            if (string.IsNullOrEmpty(html)) return new List<ReplyModel>();
            return new HtmlParser().Parse(html).GetElementById("Main").QuerySelectorAll("table")
                .Where(table => table.ParentElement.Id != null).Select(table =>
                {
                    var spans = table.QuerySelectorAll("span");
                    return new ReplyModel
                    {
                        Id = int.Parse(table.ParentElement.Id.Replace("r_", "")),
                        Avater = $"http:{table.QuerySelector("img").GetAttribute("src")}",
                        Username = table.QuerySelector("strong").TextContent,
                        Content = table.QuerySelector("div.reply_content").InnerHtml.Trim(),
                        Thanks = spans.Length == 3 ? int.Parse(spans[2].TextContent.Replace("♥ ", "")) : 0,
                        Floor = $"#{spans[0].TextContent}",
                        ReplyDate = spans[1].TextContent,
                        IsLz = topic.Member.Username == table.QuerySelector("strong").TextContent
                    };
                });
        }

        public static async Task<PersonCenterModel> GetUserInformation()
        {
            var client = ApiClient.Client;
            var html = await client.GetMainPage();
            if (string.IsNullOrEmpty(html)) return new PersonCenterModel();
            var right = new HtmlParser().Parse(html).GetElementById("Rightbar");
            var tables = right.QuerySelectorAll("table");
            var spans = tables[1].QuerySelectorAll("span.bigger");
            return new PersonCenterModel
            {
                Member = new MemberModel
                {
                    Image = "http:" + tables[0].QuerySelector("img").GetAttribute("src"),
                    Username = tables[0].QuerySelector("span.bigger").TextContent
                },
                CollectedNodes = spans[0].TextContent,
                CollectedTopics = spans[1].TextContent,
                NoticePeople = spans[2].TextContent,
                Money = right.QuerySelector("div#money").QuerySelector("a").InnerHtml,
                Notifications = right.QuerySelector("a[href='/notifications']").TextContent.Split(' ').FirstOrDefault(),
                IsNotChecked = right.QuerySelector("a[href='/mission/daily']") != null
            };
        }

        //public static async Task<PagesWithAccessModel<TopicModel>> GetTopicByUsername(string username, int page)
        //{
        //    var client = ApiClient.Client;
        //    var html = await client.GetTopicsByUsername(username, page);
        //    if (string.IsNullOrEmpty(html))
        //        return new PagesWithAccessModel<TopicModel>
        //        {
        //            IsAccess = true,
        //            Pages = 0,
        //            Entity = new List<TopicModel>()
        //        };
        //    if (html.Contains("主题列表被隐藏"))
        //        return new PagesWithAccessModel<TopicModel>
        //        {
        //            IsAccess = false,
        //            Pages = 0,
        //            Entity = new List<TopicModel>()
        //        };
        //    var dom = new HtmlParser().Parse(html);
        //    var pages = GetMaxPage(dom);
        //    var topics = dom.GetElementById("Main").GetElementsByClassName("cell item").Select(node =>
        //    {
        //        var hrefs = node.QuerySelectorAll("a");
        //        var topic = new TopicModel
        //        {
        //            Title = hrefs[0].TextContent,
        //            NodeName = hrefs[1].TextContent,
        //            Member = new MemberModel
        //            {
        //                Username = hrefs[2].TextContent
        //            },
        //            Id = int.Parse(new Regex("/t/[0-9]*").Match(hrefs[0].GetAttribute("href")).Value.Replace("/t/", ""))
        //        };
        //        if (hrefs.Length == 5)
        //        {
        //            topic.LastUsername = $"最后回复者 :{hrefs[3].TextContent}";
        //            topic.Replies = int.Parse(hrefs[4].TextContent);
        //            var last = node.GetElementsByClassName("small fade")[0].TextContent.Split('•')[2].Trim();
        //            last = last.Contains("最后回复") ? "" : last;
        //            topic.LastReply = $"时间 : {last.Trim()}";
        //        }
        //        return topic;
        //    });
        //    return new PagesWithAccessModel<TopicModel>
        //    {
        //        Pages = pages,
        //        IsAccess = true,
        //        Entity = topics
        //    };
        //}

        //public static async Task<PagesBaseModel<NotificationModel>> GetRepliesByUsername(string username, int page)
        //{
        //    var client = ApiClient.Client;
        //    var html = await client.GetRepliesByUsername(username, page);
        //    if (string.IsNullOrEmpty(html))
        //        return new PagesBaseModel<NotificationModel> {Pages = 0, Entity = new List<NotificationModel>()};
        //    var dom = new HtmlParser().Parse(html);
        //    var main = dom.GetElementById("Main");
        //    var nodes = main.QuerySelectorAll("div.dock_area");
        //    var replies = main.QuerySelectorAll("div.reply_content");
        //    var pages = GetMaxPage(dom);
        //    var notifications = nodes.Select(
        //        (node, i) =>
        //        {
        //            var href = node.QuerySelector("a");
        //            var span = node.QuerySelector("span");
        //            return new NotificationModel
        //            {
        //                Title = href.TextContent,
        //                Content = replies[i].TextContent,
        //                ReplyDate = span.TextContent
        //            };
        //        });
        //    return new PagesBaseModel<NotificationModel>
        //    {
        //        Pages = pages,
        //        Entity = notifications
        //    };
        //}

        public static async Task<MemberModel> GetMember(string username)
        {
            var client = ApiClient.Client;
            var html = await client.GetMemberInformation(username);
            if (string.IsNullOrEmpty(html)) return new MemberModel();
            var cell = new HtmlParser().Parse(html).GetElementById("Main").QuerySelector("div.cell");
            var inputs = cell.QuerySelectorAll("input");
            return !inputs.Any()
                ? new MemberModel
                {
                    Image = $"http:{cell.QuerySelector("img").GetAttribute("src")}",
                    Username = cell.QuerySelector("h1").TextContent
                }
                : new MemberModel
                {
                    Image = $"http:{cell.QuerySelector("img").GetAttribute("src")}",
                    Notice = ApiBase + inputs[0]?.GetAttribute("onclick").Split('\'')[1],
                    IsNotice = inputs[0]?.GetAttribute("value"),
                    Block = ApiBase + inputs[1]?.GetAttribute("onclick").Split('\'')[3],
                    IsBlock = inputs[1]?.GetAttribute("value"),
                    Username = cell.QuerySelector("h1").TextContent
                };
        }

        public static async Task<PagesWithNodeModel<TopicModel>> GetPageNodeAndTopics(string nodeName)
        {
            var client = ApiClient.Client;
            var html = await client.GetTopicsWithPageN(nodeName, 1);
            if (string.IsNullOrEmpty(html))
                return new PagesWithNodeModel<TopicModel>
                {
                    Pages = 0,
                    Node = new NodeModel(),
                    Entity = new List<TopicModel>()
                };
            var dom = new HtmlParser().Parse(html);
            var header = dom.GetElementById("Main").QuerySelector("div.header");
            var node = new NodeModel
            {
                Topics = Convert.ToInt32(header.QuerySelector("strong.gray").TextContent),
                IsCollect = header.QuerySelector("a").TextContent,
                Cover = header.QuerySelector("img") == null
                    ? "ms-appx:///Assets/default.png"
                    : $"http:{header.QuerySelector("img").GetAttribute("src")}"
            };
            var topics = dom.GetElementById("TopicsNode").Children.Select(topicDom =>
            {
                var hrefs = topicDom.QuerySelectorAll("a");
                var imgs = topicDom.QuerySelector("img.avatar");
                var topic = new TopicModel
                {
                    Title = hrefs[1].TextContent,
                    Member = new MemberModel
                    {
                        Username = hrefs[2].TextContent,
                        Image = $"http:{imgs.GetAttribute("src")}"
                    },
                    Id = int.Parse(topicDom.ClassName.Split('_').Last())
                };
                if (hrefs.Length == 5)
                {
                    topic.LastUsername = $"最后回复者 :{hrefs[3].TextContent}";
                    topic.Replies = int.Parse(hrefs[4].TextContent);
                    var last = topicDom.GetElementsByClassName("small fade").First().TextContent.Split('•')[1].Trim();
                    if (last.Length > 12)
                    {
                        var timeSpan = DateTime.Now - DateTime.Parse(last.Insert(10, " "));
                        last = $"{(int) timeSpan.TotalDays}天";
                    }

                    topic.LastReply = $"时间 : {last.Trim()}";
                }

                return topic;
            });
            return new PagesWithNodeModel<TopicModel>
            {
                Pages = node.Topics % 20 == 0 ? node.Topics / 20 : node.Topics / 20 + 1,
                Node = node,
                Entity = topics
            };
        }

        //public static async Task<PagesBaseModel<MoneyModel>> GetMoneyDetail(int page)
        //{
        //    var client = ApiClient.Client;
        //    var html = await client.GetMoneyDetail(page);
        //    if (string.IsNullOrEmpty(html))
        //        return new PagesBaseModel<MoneyModel> {Entity = new List<MoneyModel>(), Pages = 0};
        //    var dom = new HtmlParser().Parse(html);
        //    var pages = int.Parse(dom.QuerySelector("input.page_input").GetAttribute("max"));
        //    var moneys = dom.QuerySelector("table.data").QuerySelectorAll("tr").Skip(1).Select(e =>
        //    {
        //        var tds = e.QuerySelectorAll("td");
        //        return new MoneyModel
        //        {
        //            Time = tds[0].TextContent,
        //            Type = tds[1].TextContent,
        //            Spend = tds[2].TextContent.Replace(".0", ""),
        //            Desc = tds[4].TextContent
        //        };
        //    });
        //    return new PagesBaseModel<MoneyModel> {Entity = moneys, Pages = pages};
        //}

        //public static async Task<LoginModel> GetLoginData()
        //{
        //    var html = await Client.GetHtmlBody($"{ApiBase}/signin");
        //    if (string.IsNullOrEmpty(html)) return null;
        //    var page = new HtmlParser().Parse(html);
        //    var form = page.QuerySelector("form[action='/signin']");
        //    var inputs = form.QuerySelectorAll("input");
        //    return new LoginModel
        //    {
        //        UName = inputs[0]?.GetAttribute("name"),
        //        PName = inputs[1]?.GetAttribute("name"),
        //        CName = inputs[2]?.GetAttribute("name"),
        //        Once = inputs[3]?.GetAttribute("value"),
        //        CImage = $"{ApiBase}/_captcha?once={inputs[3]?.GetAttribute("value")}"
        //    };
        //}
    }
}