using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using V2EX.Model;
using System.Runtime.Serialization.Json;
using Windows.UI.Xaml.Controls;
using V2EX.Views;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net;
using V2EX.util;
using System.IO;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace V2EX.GetData
{
    class GetData
    {
        public static HttpClient client = new HttpClient();
        const string API_BASE = "http://www.v2ex.com";
        const string API_STATS = "/api/site/stats.json";
        const string API_NODES = "/api/nodes/all.json";
        const string API_NODE = "/api/nodes/show.json";
        const string API_LATEST_TOPICS = "/api/topics/latest.json";
        const string API_HOT_TOPICS = "/api/topics/hot.json";
        const string API_XXX_TOPIC = "/api/topics/show.json";
        const string API_REPLIES = "/api/replies/show.json";
        const string API_MEMBER = "/api/members/show.json";
        public List<NodeModel> GetNodes()
        {
            var nodesStream = GetStream(API_BASE + API_NODES);
            if (nodesStream == null) return new List<NodeModel>();
            var Nodes = ((NodeModel[])new DataContractJsonSerializer(typeof(NodeModel[])).ReadObject(nodesStream)).ToList();
            Nodes.ForEach(Node =>
                {
                    HtmlDocument htmlDoc = new HtmlDocument();
                    if (Node.header != null)
                    {
                        htmlDoc.LoadHtml(Node.header);
                        Node.header = htmlDoc.DocumentNode.InnerText;
                    }
                });
            return Nodes;
        }

        //public List<TopicModel> GetLatestTopics()
        //{
        //    var latestTopicsStream = GetStream(API_BASE + API_LATEST_TOPICS);
        //    if (latestTopicsStream == null) return new List<TopicModel>();
        //    var topics = ((TopicModel[])new DataContractJsonSerializer(typeof(TopicModel[])).ReadObject(latestTopicsStream)).ToList();
        //    topics.ForEach(topic => topic.member.avatar_normal = $"http:{topic.member.avatar_normal}");
        //    return topics;
        //}
        //public List<TopicModel> GetHotTopics()
        //{
        //    var hotTopicsStream = GetStream(API_BASE + API_HOT_TOPICS);
        //    if (hotTopicsStream == null) return new List<TopicModel>();
        //    var topics = ((TopicModel[])new DataContractJsonSerializer(typeof(TopicModel[])).ReadObject(hotTopicsStream)).ToList();
        //    topics.ForEach(topic => topic.member.avatar_normal = $"http:{topic.member.avatar_normal}");
        //    return topics;
        //}
       
        public TopicModel[] GetTopicsByUsername(string username)
        {
            var topicsStream = GetStream(API_BASE + API_XXX_TOPIC + "?username=" + username);
            var topics = (TopicModel[])new DataContractJsonSerializer(typeof(TopicModel[])).ReadObject(topicsStream);
            return topics;
        }
        public NavListModel[] GetLists()
        {
            return new NavListModel[]
            {
                 new NavListModel() { ListSymbol="ViewAll",ListText="聚合话题",DestPage=typeof(CollectedTopicsView)},
                new NavListModel() { ListSymbol="List",ListText="全部节点",DestPage=typeof(SortNodesView)},
                new NavListModel() { ListSymbol="People",ListText="个人中心" ,DestPage=typeof(UserInfoView)}
            };
        }
        public CollectedListModel[] GetCollectedTab()
        {
            return new CollectedListModel[]
            {
           new CollectedListModel() { Text="技术",Name="tech"},
           new CollectedListModel() {Text="创意",Name="creative" },
          new CollectedListModel() { Text="好玩",Name="play"},
          new CollectedListModel() { Text="Apple",Name="apple"},
          new CollectedListModel() { Text="酷工作",Name="jobs"},
          new CollectedListModel() { Text="交易",Name="deals"},
          new CollectedListModel() { Text="城市",Name="city"},
          new CollectedListModel() { Text="问与答",Name="qna"},
          new CollectedListModel() { Text="最热",Name="hot"},
          new CollectedListModel() {Text="全部",Name="all" },
          new CollectedListModel() {Text="R2",Name="r2" },new CollectedListModel() { Text="节点",Name="nodes"},
          new CollectedListModel() { Text="关注",Name="members"},
          
            };
        }
        public string GetHTMLBody(string url)
        {
            try
            {
             return client.GetStringAsync(url).Result;
            }
            catch
            {
                return null;
            }
        }

        public Stream GetStream(string url)
        {
            var html=GetHTMLBody(url);
            if (html == null) return null;
            MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(html));
            return stream;
        }
        public int GetMaxPage(string html)
        {
            Regex regex = new Regex("class=\"fade\">1/[0-9]{1,}");//正则走起，搞出总页数
            Match match = regex.Match(html);
            return int.Parse(match.Value.Replace("class=\"fade\">1/", ""));
        }
        public List<TopicModel> GetOnePageTopics(String nodeName, int page)
        {
            List<TopicModel> topics = new List<TopicModel>();
            HtmlDocument HTMLBody = new HtmlDocument();
            if (page < 0) return topics;
            string body = GetHTMLBody(String.Format("{0}/go/{1}?p={2}", API_BASE, nodeName, page));
            if (body == null) return topics;
            HTMLBody.LoadHtml(body);
            HtmlNode parent = HTMLBody.GetElementbyId("TopicsNode");//处理内部获得数据
            var childrenNodes = parent.ChildNodes;
            foreach (HtmlNode node in childrenNodes)
            {
                TopicModel topic = new TopicModel();
                var hrefs = node.SelectNodes(".//a");
                if (hrefs != null)
                {
                    topic.Title = hrefs[1].InnerText;
                    topic.Member = new MemberModel();
                    topic.Member.Username = hrefs[2].InnerText;
                    if (hrefs.Count == 5)
                    {
                        topic.LastUsername = "最后回复: " + hrefs[3].InnerText;
                        topic.Replies = int.Parse(hrefs[4].InnerText);
                        var spans = node.SelectNodes(".//span");
                        string last = spans[1].InnerText.Replace(" ", "").Split(';')[2].Replace("&nbsp", "").Replace("+08:00", "");
                        if (last.Length > 9)
                        {
                            last = last.Insert(10, " ");
                            TimeSpan timeSpan = DateTime.Now - DateTime.Parse(last);
                            last = string.Format("{0}天", (int)timeSpan.TotalDays);
                        }
                        topic.LastReply = "时间: " + last;
                    }
                    topic.Node = new NodeModel();
                    topic.Node.name = nodeName;
                    Regex regexId = new Regex("t_[0-9]*");
                    string topicId = regexId.Match(node.OuterHtml).Value.Replace("t_", "");
                    topic.Id = int.Parse(topicId);
                    topics.Add(topic);
                }
            }
            return topics;
        }
        public List<TopicModel> GetTopicsFromTab(string tab)
        {
            return GetTopicMethod(GetHTMLBody($"{API_BASE}/?tab={tab}"));
        }
        public string GetOnce(string html)
        {
            if (html == null) return null;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc.DocumentNode.SelectSingleNode(".//input[@name='once']").Attributes["value"].Value;
        }
        public int LoginWithUsername(string username, string password)//0=>成功 1=>密码错误 2=>网络连接失败 
        {
            if (client.DefaultRequestHeaders.Referrer == null)
            {
                client.DefaultRequestHeaders.Add("Referer", API_BASE + "/signin");
            }
            var html = GetHTMLBody(string.Format("{0}/signin", API_BASE));
            if (html == null) return 2;
            string once = GetOnce(html);
            List<KeyValuePair<string, string>> pramas = new List<KeyValuePair<string, string>>();
            pramas.Add(new KeyValuePair<string, string>("next", "/"));
            pramas.Add(new KeyValuePair<string, string>("u", username));
            pramas.Add(new KeyValuePair<string, string>("once", once));
            pramas.Add(new KeyValuePair<string, string>("p", password));
            try
            {
                var response = client.PostAsync(API_BASE + "/signin", new FormUrlEncodedContent(pramas)).Result;
                var resopnseText= response.Content.ReadAsStringAsync().Result;
                if (resopnseText.Contains("用户名和密码无法匹配"))
                {
                    return 1;
                }
                else
                {
                    if (resopnseText.Contains("登出"))
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            catch
            {
                return 2;
            }
        }
        public List<NodeModel> GetFavoriteNodes()
        {
            var html = GetHTMLBody($"{API_BASE}/my/nodes");
            List<NodeModel> nodes = new List<NodeModel>();
            if (html == null) return nodes;
            HtmlDocument htmlBody = new HtmlDocument();
            htmlBody.LoadHtml(html);
            var myNodes = htmlBody.GetElementbyId("MyNodes").SelectNodes(".//a[@class='grid_item']");
            foreach (HtmlNode node in myNodes)
            {
                var Node = new NodeModel();
                Node.id = int.Parse(node.Id.Replace("n_", ""));
                Node.name = node.Attributes["href"].Value.Replace("/go/", "");
                Node.image = "http:"+ node.SelectSingleNode(".//img").Attributes["src"].Value;
                var strs=node.InnerText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Node.title = "";
                for(int i=0;i<strs.Length-1;i++)
                {
                    Node.title += strs[i];
                }
                Node.topics = int.Parse(strs[strs.Length - 1]);
                nodes.Add(Node);
            }
            return nodes;
        }
        public List<TopicModel> GetFavoriteTopics(int page)
        {
            if (page <= 0) return new List<TopicModel>();
            return GetTopicMethod(GetHTMLBody(string.Format("{0}/my/topics?p={1}", API_BASE, page)));
        }
        public List<TopicModel> GetFollowerTopics(int page)
        {
            return GetTopicMethod(GetHTMLBody(string.Format("{0}/my/following?p={1}", API_BASE, page)));
        }
        private List<TopicModel> GetTopicMethod(string html)
        {
            List<TopicModel> topics = new List<TopicModel>();
            if (html == null) return topics;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main = htmlDoc.GetElementbyId("Main");
            var children = main.ChildNodes[3];
            var nodes = children.ChildNodes;
            foreach (HtmlNode node in nodes)
            {
                if (node.NodeType == HtmlNodeType.Text) continue;
                TopicModel topic = new TopicModel();
                var hrefs = node.SelectNodes(".//a");
                var spans = node.SelectNodes(".//span");
                if (spans == null) continue;
                if (spans.Count != 2) continue;
                if (hrefs == null) continue;
                if (hrefs.Count != 4 && hrefs.Count != 6) continue;
                topic.Title = util.SymbolRemove.Remove(hrefs[1].InnerText);
                topic.Node = new NodeModel();
                topic.Node.title = hrefs[2].InnerText;
                topic.Member = new MemberModel();
                topic.Member.Username = hrefs[3].InnerText;
                topic.Member.avatar_normal = "http:"+ hrefs[0].FirstChild.Attributes["src"].Value;
                if (hrefs.Count == 6)
                {
                    topic.LastUsername = "最后回复: " + hrefs[4].InnerText;
                    topic.Replies = int.Parse(hrefs[5].InnerText);
                    string[] strs = spans[1].InnerText.Split(' ');
                    string last = "";
                    for (int i = 0; i < strs.Length; i++)
                    {
                        if (strs[i] == "天前")
                        {
                            last += strs[i - 1] + "天前";
                        }
                        if (strs[i] == "小时")
                        {
                            last += strs[i - 1] + "小时";
                        }
                        if (strs[i] == "分钟前")
                        {
                            last += strs[i - 1] + "分钟前";
                        }
                        if (strs[i] == "几秒前")
                        {
                            last += "几秒前";
                        }
                        if (strs[i] == "刚刚")
                        {
                            last += "刚刚";
                        }
                        if (strs[i] == "置顶")
                        {
                            last += "置顶";
                        }
                    }
                    topic.LastReply = "时间: " + last;
                }
                Regex regexId = new Regex("/t/[0-9]*");
                topic.Id = int.Parse(regexId.Match(node.InnerHtml).Value.Replace("/t/", ""));
                topics.Add(topic);
            }
            return topics;
        }
        public int PeopleCheckIn()//0=>失败 1=>成功 2=>签过
        {
            var html = GetHTMLBody(string.Format("{0}/mission/daily", API_BASE));
            if (html == null) return 0;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main = htmlDoc.GetElementbyId("Main");
            var input = main.SelectSingleNode(".//input");
            if (input == null) return 0;
            var href = input.Attributes["onclick"].Value.Replace("location.href = '", "").Replace("';", "").Trim();
            var url = API_BASE + href;
            if (url.Contains("/balance")) return 2;
            client.DefaultRequestHeaders.Add("Origin", API_BASE);
            if (client.DefaultRequestHeaders.Referrer == null)
            {
                client.DefaultRequestHeaders.Add("Referer", url);
            }
            try
            {
                var response = client.GetAsync(url).Result;
                var result= response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode) return 1; else return 0;
            }
            catch
            {
                return 0;
            }
        }
        public List<NotificationModel> GetPeopleNotifications(string html)
        {
            List<NotificationModel> notifications = new List<NotificationModel>();
            //if (page < 0) return notifications;
            //var html = GetHTMLBody($"{API_BASE}/notifications?p={page}");
            if (html == null) return notifications;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main = htmlDoc.GetElementbyId("Main");
            var nodes = main.SelectNodes(".//div[@class='cell']");
            foreach (HtmlNode node in nodes)
            {
                NotificationModel notification = new NotificationModel();
                if (node.Attributes["id"] == null) continue;
                Regex regexId = new Regex("/t/[0-9]*");
                Regex regexReply = new Regex("#reply[0-9]*");
                notification.Topic = new TopicModel();
                notification.Topic.Id = int.Parse(regexId.Match(node.InnerHtml).Value.Replace("/t/", ""));
                notification.Id = int.Parse(node.Id.Replace("n_", ""));
                var text = SymbolRemove.Remove(node.InnerText.Replace("删除", ""));
                notification.Title = node.SelectSingleNode(".//span[@class='fade']").InnerText;
                notification.ReplyDate = node.SelectSingleNode(".//span[@class='snow']").InnerText;
                notification.Content = node.SelectSingleNode(".//div[@class='payload']").InnerHtml;
                var strs = node.SelectSingleNode(".//a[@class='node']").Attributes["onclick"].Value.Split(new char[] { '(', ',', ')' });
                notification.Token = strs[2].Trim(); 
                notification.ReplyFloor = int.Parse(regexReply.Match(node.InnerHtml).Value.Replace("#reply",""));
                notifications.Add(notification);
            }
            return notifications;
        }
        public List<NotificationModel> GetAllNotifications()
        {
            List<NotificationModel> notifications = new List<NotificationModel>();
            var html = GetHTMLBody($"{API_BASE}/notifications?p=1");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var header = htmlDoc.GetElementbyId("Main").SelectSingleNode(".//div[@class='header']");
            var messages = int.Parse(header.SelectSingleNode(".//strong[@class='gray']").InnerText);
            int maxPage; 
            if(messages%10!=0)
            {
                maxPage = messages / 10 + 1;
            }
            else
            {
                maxPage = messages / 10;
            }
            for (int page = 1; page <= maxPage; page++)
            {
                if (page == 1)
                {
                    notifications.AddRange(GetPeopleNotifications(html));
                }
                else
                {
                    notifications.AddRange(GetPeopleNotifications(GetHTMLBody($"{API_BASE}/notifications?p={page}")));
                    notifications.ForEach(notification => notification.Token = notifications[notifications.Count - 1].Token);
                }
            }
            return notifications;
        }
        public int ReplyCreateWithTopicId(int id, string content)
        {
            var url = $"{API_BASE}/t/{id}";
            var html = GetHTMLBody(url);
            if (html == null) return 0;
            var once = GetOnce(html);
            if (client.DefaultRequestHeaders.Referrer == null)
            {
                client.DefaultRequestHeaders.Add("Referer", url);
            }
            List<KeyValuePair<string, string>> pramas = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("content", content),
                new KeyValuePair<string, string>("once", once)
            };
            try
            {
                var response = client.PostAsync(url, new FormUrlEncodedContent(pramas)).Result;
                var responseString=response.Content.ReadAsStringAsync().Result;
                if(!response.IsSuccessStatusCode)
                {
                    return 0;
                }
                if (responseString.Contains("你回复过于频繁了"))
                {
                    return 2;
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public List<ReplyModel> GetReply(string html)
        {
            List<ReplyModel> replies = new List<ReplyModel>();
            if (html == null) return replies;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            html = htmlDoc.GetElementbyId("Main").OuterHtml;
            htmlDoc.LoadHtml(html);
            Regex regexId = new Regex("r_[0-9]{1,}");
            var replyIds = regexId.Matches(html);
            int i = 1;
            foreach (Match replyId in replyIds)
            {
                ReplyModel reply = new ReplyModel();
                reply.Id = int.Parse(replyId.Value.Replace("r_", ""));
                var replyContent = htmlDoc.GetElementbyId(replyId.Value);
                if (replyContent == null) continue;
                Regex regexToken = new Regex("thankReply((.*))");
                if (regexToken.IsMatch(replyContent.OuterHtml))
                {
                    reply.Token = regexToken.Match(replyContent.OuterHtml).Value.Split('\'')[1];
                }
                var strong = replyContent.SelectSingleNode(".//strong");
                reply.Member = new MemberModel();
                var img = replyContent.SelectSingleNode(".//img");
                reply.Member.avatar_normal = "http:"+img.Attributes["src"].Value;
                reply.Member.Username = strong.InnerText;
                var div = replyContent.SelectSingleNode(".//div[@class='reply_content']");
                reply.Content = div.InnerHtml.Trim();
                var span = replyContent.SelectNodes(".//span");
                if (span.Count == 3)
                {
                    reply.Thanks = int.Parse(span[span.Count - 1].InnerText.Replace("♥ ", ""));
                    reply.ThanksNumber = $"{span[span.Count - 1].InnerText.Replace("♥ ", "")} 赞";
                }
                else
                {
                    reply.Thanks = 0;
                    reply.ThanksNumber = "0 赞";
                }
                replies.Add(reply);
            }
            return replies;
        }
        public Tuple<TopicModel,List<ReplyModel>> GetRepliesAndTopicContent(int id)
        {
            List<ReplyModel> replies = new List<ReplyModel>();
            TopicModel topic = new TopicModel();
            Regex regexMax = new Regex("[0-9]{1,} 回复");
            var html = GetHTMLBody(string.Format("{0}/t/{1}?p={2}", API_BASE, id, 1));
            if (html == null) return new Tuple<TopicModel,List<ReplyModel>>(topic,replies);
            var value = regexMax.Match(html);
            int maxPage;
            if (value != null && value.Value != "")
            {
                int maxReply = int.Parse(value.Value.Replace(" 回复", ""));
                if (maxReply % 100 != 0)
                {
                    maxPage = maxReply / 100 + 1;
                }
                else
                {
                    maxPage = maxReply / 100;
                }
            }
            else
            {
                maxPage = 0;
            }
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main = htmlDoc.GetElementbyId("Main");
            var replyNumber = regexMax.Match(main.OuterHtml).Value.Replace(" 回复", "");
            if(replyNumber!="")
            {
                topic.TopicsNumber = $"{replyNumber}\n回复";
            }
            else
            {
                topic.TopicsNumber = $"{0}\n回复";
            }
            topic.Id = id;
            Regex regexToken = new Regex("thankTopic((.*))");
            topic.Token = regexToken.Match(main.OuterHtml).Value.Split('\'')[1];
            var node = main.SelectSingleNode(".//div[@class='header']");
            topic.Member = new MemberModel();
            topic.Member.avatar_normal = "http:" + main.SelectSingleNode(".//img").Attributes["src"].Value;
            topic.Member.Username = node.SelectSingleNode(".//a").Attributes["href"].Value.Split('/')[2];
            topic.Title = util.SymbolRemove.Remove(node.SelectSingleNode(".//h1").InnerText);
            topic.Url = $"{API_BASE}/t/{id}";
            var content = main.SelectSingleNode("//div[@class='topic_content']");
            var subtles=main.SelectNodes(".//div[@class='subtle']");
            topic.Postscript = new List<TopicModel>();
            if(subtles!=null)
            {
                foreach(HtmlNode subtle in subtles)
                {
                    TopicModel ps = new TopicModel();
                    ps.Content= subtle.SelectSingleNode(".//div[@class='topic_content']").InnerHtml;
                    ps.LastReply = util.SymbolRemove.Remove(subtle.SelectSingleNode(".//span[@class='fade']").InnerText);
                    topic.Postscript.Add(ps);
                }
            }
            if(main.InnerText.Contains("加入收藏"))
            {
                topic.Collect = "加入\n收藏";
            }
            else
            {
                topic.Collect = "已\n收藏";
            }
            if(content != null)
            {
                topic.Content = content.InnerHtml;
            }
            else
            {
                topic.Content = "";
            }
            for (int page = 1; page <= maxPage; page++)
            {
                if (page == 1)
                {
                    replies.AddRange(GetReply(html));
                }
                else
                {
                    replies.AddRange(GetReply(GetHTMLBody(string.Format("{0}/t/{1}?p={2}", API_BASE, id, page))));
                }
            }
            int i = 1;
            replies.ForEach(reply =>reply.Floor = $"{i++}楼");
            return new Tuple<TopicModel,List<ReplyModel>>(topic,replies);
        }
        public bool FavNodeWithNodeName(string nodeName)
        {
            //client.DefaultRequestHeaders.Add("Referrer", API_BASE);
            var html = GetHTMLBody(string.Format("{0}/go/{1}", API_BASE, nodeName));
            var url = GetFavUrlFromResponse(html);
            if(url==null)
            {
                url = GetFavUrlFromEnResponse(html);
            }
            if(url==null)
            {
                return false;
            }
            try
            {
                var response = client.GetAsync(API_BASE + url).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public bool FavTopicWithTopicId(int id)
        {
            var html = GetHTMLBody(string.Format("{0}/t/{1}", API_BASE, id));
            var url = GetFavUrlFromResponse(html);
            if (url == null)
            {
                url = GetFavUrlFromEnResponse(html);
            }
            if (url == null)
            {
                return false;
            }
            try
            {
                var response = client.GetAsync(API_BASE + url).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public string GetFavUrlFromResponse(string response)
        {
            Regex regexFav = new Regex("<a href=\"(.*)\">加入收藏</a>");
            Regex regexUnFav = new Regex("<a href=\"(.*)\">取消收藏</a>");
            if (regexFav.IsMatch(response))
            {
                var url = regexFav.Match(response).Value.Replace("<a href=\"", "").Replace("\" class=\"tb","").Replace("\">加入收藏</a>", "");
                return url;
            }
            if(regexUnFav.IsMatch(response))
            {
                var url = regexUnFav.Match(response).Value.Replace("<a href=\"", "").Replace("\" class=\"tb", "").Replace("\">取消收藏</a>", "");
                return url;
            }
                return null;
        }
        public string GetFavUrlFromEnResponse(string response)
        {
            Regex regexFav = new Regex("<a href=\"(.*)\">Favorite This Node</a>");
            Regex regexUnfav = new Regex("<a href=\"(.*)\">Unfavorite</a>");
            if(regexFav.IsMatch(response))
            {
                var url = regexFav.Match(response).Value.Replace("<a href=\"", "").Replace("\" class=\"tb\">Favorite This Node</a>", "");
                return url;
            }
            if(regexUnfav.IsMatch(response))
            {
                var url = regexUnfav.Match(response).Value.Replace("<a href=\"", "").Replace("\" class=\"tb\">Unfavorite</a>", "");
                return url;
            }
            return null;
        }

        public PersonCenterModel GetUserInformation()
        {
            PersonCenterModel user = new PersonCenterModel();
            var html = GetHTMLBody($"{API_BASE}/my/nodes");
            if (html == null) return user;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var right = htmlDoc.GetElementbyId("Rightbar");
            var tables = right.SelectNodes(".//table");
            user.Member = new MemberModel();
            user.Member.avatar_normal = "http:" + tables[0].SelectSingleNode(".//img").Attributes["src"].Value;
            user.Member.Username = tables[0].SelectSingleNode(".//span[@class='bigger']").InnerText;
            var spans = tables[1].SelectNodes(".//span[@class='bigger']");
            user.Nodes = int.Parse(spans[0].InnerText);
            user.Topics = int.Parse(spans[1].InnerText);
            user.Followers = int.Parse(spans[2].InnerText);
            var kinds=right.SelectSingleNode(".//div[@id='money']").InnerText.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
            int money;
            if(kinds.Length==3)
            {
                money = int.Parse(kinds[0]) * 1000 + int.Parse(kinds[1]) * 100 + int.Parse(kinds[2]);
            }
            else if(kinds.Length==2)
            {
                money = int.Parse(kinds[0]) * 100 + int.Parse(kinds[1]);
            }
            else
            {
                money = int.Parse(kinds[0]);
            }
            user.Money = money;
            user.Notifications = int.Parse(right.SelectSingleNode(".//a[@ href='/notifications']").InnerText.Split(' ')[0]);
            return user;

        }
        public bool ThanksReply(int id,string token)
        {
            var response= client.PostAsync($"{API_BASE}/thank/reply/{id}?t={token}",new FormUrlEncodedContent(new List<KeyValuePair<string,string>>())).Result;
            return response.IsSuccessStatusCode;
        }
        public bool ThanksTopic(int id,string token)
        {
            var response = client.PostAsync($"{API_BASE}/thank/topic/{id}?t={token}", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>())).Result;
            return response.IsSuccessStatusCode;
        }
        public List<TopicModel> GetTopicByUsername(string Username,int Page)
        {
            List<TopicModel> topics = new List<TopicModel>();
            var html=GetHTMLBody($"{API_BASE}/member/{Username}/topics?p={Page}");
            if (html == null) return topics;
            if (html.Contains("主题列表被隐藏"))
            {
                topics.Add(new TopicModel() { Content = "该用户设置隐藏了自己的主题列表",Title="tips:" });
                return topics;
            }
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main=htmlDoc.GetElementbyId("Main");
            var nodes=main.SelectNodes(".//div[@class='cell item']");
            if(nodes==null) return topics;
            foreach(HtmlNode node in nodes)
            {
                TopicModel topic = new TopicModel();
                var hrefs=node.SelectNodes(".//a");
                topic.Title = hrefs[0].InnerText;
                topic.Node = new NodeModel();
                topic.Node.title = hrefs[1].InnerText;
                topic.Member = new MemberModel();
                topic.Member.Username = hrefs[2].InnerText;
                if(hrefs.Count==5)
                {
                    topic.LastUsername = hrefs[3].InnerText;
                    topic.Replies = int.Parse(hrefs[4].InnerText);
                }
                Regex regexId = new Regex("/t/[0-9]*");
                topic.Id = int.Parse(regexId.Match(node.OuterHtml).Value.Replace("/t/",""));
                topics.Add(topic);
            }
            return topics;
        }

        public List<NotificationModel> GetRepliesByUsername(string Username,int Page)
        {
            List<NotificationModel> notifications = new List<NotificationModel>();
            var html = GetHTMLBody($"{API_BASE}/member/{Username}/replies?p={Page}");
            if (html == null) return notifications;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main=htmlDoc.GetElementbyId("Main");
            var nodes=main.SelectNodes(".//div[@class='dock_area']");
            if (nodes == null) return notifications;
            var inners = main.SelectNodes(".//div[@class='inner']");
            for(int i=0;i<nodes.Count-1;i++)
            {
                NotificationModel notification = new NotificationModel();
                notification.Title= nodes[i].SelectSingleNode(".//td").InnerText;
                notification.Content = inners[i].InnerText;
                notifications.Add(notification);
            }
            return notifications;
        }

        public MemberModel GetMember(string Username)
        {
            MemberModel member = new MemberModel();
            var html=GetHTMLBody($"{API_BASE}/member/{Username}");
            if (html == null) return member;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var main=htmlDoc.GetElementbyId("Main");
            var cell=main.SelectSingleNode(".//div[@class='cell']");
            member.avatar_normal = "http:" + cell.SelectSingleNode(".//img").Attributes["src"].Value;
            var inputs=cell.SelectNodes(".//input");
            member.Notice = API_BASE + inputs[0].Attributes["onclick"].Value.Split('\'')[1];
            member.IsNotice = inputs[0].Attributes["value"].Value;
            member.Block = API_BASE + inputs[1].Attributes["onclick"].Value.Split('\'')[3];
            member.IsBlock = inputs[1].Attributes["value"].Value;
            member.Username=cell.SelectSingleNode(".//h1").InnerText;
            return member;
        }
        public bool NoticeUser(string url)
        {
            var response = client.GetAsync(url).Result;
            return response.IsSuccessStatusCode;
        }
        public bool BlockUser(string url)
        {
            var response = client.GetAsync(url).Result;
            return response.IsSuccessStatusCode;
        }
        public bool DeleteNotification(int id,string token)
        {
            var response = client.PostAsync($"{API_BASE}/delete/notification/{id}?once={token}", new FormUrlEncodedContent(new Dictionary<string,string>())).Result;
            return response.IsSuccessStatusCode;
        }
        public bool CreateTopic(string NodeName,string Content,string Title)
        {
            var Url = $"{API_BASE}/new/{NodeName}";
            var Once = GetOnce(GetHTMLBody(Url));
            var Param = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("once",Once),
                new KeyValuePair<string, string>("content",Content),
                new KeyValuePair<string, string>("title",Title)
            };
            if(client.DefaultRequestHeaders.Referrer==null)
            {
                client.DefaultRequestHeaders.Add("Referer", Url);
            }
            var Respones=client.PostAsync(Url, new FormUrlEncodedContent(Param)).Result;
            if (Respones.IsSuccessStatusCode) return true; else return false;
        }
        public NodeModel GetPartNode(string NodeName)
        {
            NodeModel node = new NodeModel();
            var html = GetHTMLBody($"http://www.v2ex.com/go/{NodeName}");
            if (html == null) throw new Exception("获取数据失败");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var header = htmlDoc.GetElementbyId("Main").SelectSingleNode(".//div[@class='header']");
            node.image = "http:" + header.SelectSingleNode(".//img").Attributes["src"].Value;
            node.iscollect = header.SelectSingleNode(".//a").InnerText;
            return node;
        }
    }
}
