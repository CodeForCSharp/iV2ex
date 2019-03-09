using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using iV2EX.Model;

namespace iV2EX.GetData
{
    class DomParse
    {
        public static int ParseMaxPage(IHtmlDocument dom)
        {
            if (dom == null) return 0;
            var strong = dom.QuerySelector("strong.gray");
            var maxNumber = int.Parse(strong?.TextContent ?? "0");
            return maxNumber % 20 == 0 ? maxNumber / 20 : maxNumber / 20 + 1;
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
                        $"{node.QuerySelector("span.topic_info").TextContent.Split('•')[2].Trim()}";
                }
                return topic;
            });
        }
    }
}
