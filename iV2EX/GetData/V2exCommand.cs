using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using iV2EX.Model;
using WebApiClient;
using WebApiClient.Attributes;

namespace iV2EX.GetData
{
    [Timeout(20000)]
    [Header("Accept-Language", "zh-CN")]
    [Header("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586")]
    [HttpHost("https://www.v2ex.com")]
    public interface IV2exApi : IHttpApiClient
    {
        [HttpGet("/mission/daily")]
        ITask<string> GetCheckInInformation();

        [HttpGet]
        ITask<string> CheckIn([Url] string url, [Header("Referer")] string referer);

        [HttpPost("/signin")]
        [Header("Referer", "https://www.v2ex.com/signin")]
        ITask<string> SignIn([HttpContent] FormUrlEncodedContent content);

        [HttpGet("/signin")]
        ITask<string> GetSignInInformation();

        [HttpGet("/")]
        ITask<string> GetMainPage();

        [HttpGet("/t/{id}")]
        ITask<string> GetTopicInformation(int id);

        [HttpPost("/t/{id}")]
        ITask<string> ReplyTopic([Header("Referer")] string referer, [HttpContent] FormUrlEncodedContent content,
            int id);

        [HttpGet("/go/{nodeName}")]
        ITask<string> GetNodeInformation(string nodeName);

        [HttpGet]
        ITask<string> OnlyGet([Url] string url);

        [HttpPost("/new/{nodeName}")]
        ITask<string> NewTopic([Header("Referer")] string referer, [HttpContent] FormUrlEncodedContent content,
            string nodeName);

        [HttpGet("/api/nodes/all.json")]
        [JsonReturn]
        ITask<List<NodeModel>> GetNodes();

        [HttpGet("/go/{nodeName}")]
        ITask<string> GetTopicsWithPageN(string nodeName, int p);

        [HttpGet("/")]
        ITask<string> GetTopicsWithTab(string tab);

        [HttpGet("/my/nodes")]
        ITask<string> GetFavoriteNodes();

        [HttpGet("/my/topics")]
        ITask<string> GetFavoriteTopics(int p);

        [HttpGet("/my/following")]
        ITask<string> GetFollowerTopics(int p);

        [HttpGet("/notifications")]
        ITask<string> GetNotifications(int p);

        [HttpGet("/t/{id}")]
        ITask<string> GetRepliesAndTopicContent(int id, int p);

        [HttpGet("/member/{username}/topics")]
        ITask<string> GetTopicsByUsername(string username, int p);

        [HttpGet("/member/{username}/replies")]
        ITask<string> GetRepliesByUsername(string username, int p);

        [HttpGet("/member/{username}")]
        ITask<string> GetMemberInformation(string username);

        [HttpGet("/balance")]
        ITask<string> GetMoneyDetail(int p);

        [HttpGet]
        ITask<Stream> GetStream([Url] string url);

        [HttpGet("/settings")]
        ITask<string> GetSettingInformation();
    }

    internal enum CheckInStatus
    {
        Success = 0,
        Failure = 1,
        Gone = 2
    }

    internal enum SignInStatus
    {
        UsernameEmpty = 0,
        PasswordEmpty = 1,
        CaptchaEmpty = 2,
        NetworkError = 3,
        UsernameOrPasswordError = 4,
        Success = 5
    }

    internal enum ReplyStatus
    {
        Success = 0,
        Failure = 1,
        Ban = 2,
        TextEmpty = 3
    }

    internal enum WrritenStatus
    {
        Success = 0,
        Failure = 1,
        TitleEmpty = 2,
        TitleLonger = 3,
        BodyEmpty = 4,
        BodyLonger = 5,
        NotExistNode = 6
    }

    internal static class V2ExCommand
    {
        private const string ApiBase = "https://www.v2ex.com";

        //public static async Task<int> LoginWithUsername(LoginModel data) //0=>成功 1=>密码错误 2=>网络连接失败
        //{
        //    var pramas = new Dictionary<string, string>
        //    {
        //        {"next", "/"},
        //        {data.UName, data.Username},
        //        {"once", data.Once},
        //        {data.PName, data.Password},
        //        {data.CName, data.Captcha}
        //    };
        //    var request = new HttpRequestMessage
        //    {
        //        Headers = { {"Referer", ApiBase + "/signin" },{"Origin",ApiBase}},
        //        Content = new FormUrlEncodedContent(pramas),
        //        RequestUri = new Uri(ApiBase + "/signin"),
        //        Method = HttpMethod.Post
        //    };
        //    try
        //    {
        //        var response = await Client.Center.SendAsync(request);
        //        var resopnseText = await response.Content.ReadAsStringAsync();
        //        if (resopnseText.Contains("用户名和密码无法匹配"))
        //            return 1;
        //        return resopnseText.Contains("登出") ? 0 : 1;
        //    }
        //    catch
        //    {
        //        return 2;
        //    }
        //}

        //public static async Task<int> PeopleCheckIn() //0=>失败 1=>成功 2=>签过
        //{
        //    var html = await Client.GetHtmlBody($"{ApiBase}/mission/daily");
        //    if (string.IsNullOrEmpty(html)) return 0;
        //    var input = new HtmlParser().Parse(html).GetElementById("Main").QuerySelector("input");
        //    var href = input.GetAttribute("onclick").Replace("location.href = '", "").Replace("';", "").Trim();
        //    var url = ApiBase + href;
        //    if (url.Contains("/balance")) return 2;
        //    var request = new HttpRequestMessage
        //    {
        //        Headers = {{"Origin", ApiBase}, {"Referer", url}},
        //        Method = HttpMethod.Get,
        //        RequestUri = new Uri(url),
        //    };
        //    try
        //    {
        //        var response = await Client.Center.SendAsync(request);
        //        return response.IsSuccessStatusCode ? 1 : 0;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        public static async Task<int> ReplyCreateWithTopicId(int id, string content)
        {
            var client = ApiClient.Client;
            try
            {
                var html = await client.GetTopicInformation(id);
                var once = new HtmlParser().Parse(html).QuerySelector("input[name='once']").GetAttribute("value");
                var pramas = new Dictionary<string, string>
                {
                    {"content", content},
                    {"once", once}
                };
                var text = await client.ReplyTopic($"{ApiBase}/t{id}", new FormUrlEncodedContent(pramas), id);
                return text.Contains("你回复过于频繁了") ? 2 : 1;
            }
            catch
            {
                return 0;
            }
        }

        public static async Task<bool> FavNodeWithNodeName(string nodeName)
        {
            var client = ApiClient.Client;
            try
            {
                var html = await client.GetNodeInformation(nodeName);
                var url = GetFavUrlFromResponse(html);
                await client.OnlyGet($"{ApiBase}{url}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> FavTopicWithTopicId(int id)
        {
            var client = ApiClient.Client;
            try
            {
                var html = await client.GetTopicInformation(id);
                var url = GetFavUrlFromResponse(html);
                await client.OnlyGet($"{ApiBase}{url}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetFavUrlFromResponse(string response)
        {
            var regexFav = new Regex("<a href=\"(.*)\">加入收藏</a>");
            var regexUnFav = new Regex("<a href=\"(.*)\">取消收藏</a>");
            if (regexFav.IsMatch(response))
                return regexFav.Match(response).Groups[1].Value;
            return regexUnFav.IsMatch(response) ? regexUnFav.Match(response).Groups[1].Value : null;
        }

        //public static async Task<bool> ThanksReply(int id, string token)
        //{
        //    var response = await Client.Center.PostAsync(new Uri($"{ApiBase}/thank/reply/{id}?t={token}"),
        //        new FormUrlEncodedContent(new Dictionary<string,string>()));
        //    return response.IsSuccessStatusCode;
        //}

        //public static async Task<bool> ThanksTopic(int id, string token)
        //{
        //    var response = await Client.Center.PostAsync(new Uri($"{ApiBase}/thank/topic/{id}?t={token}"),
        //        new FormUrlEncodedContent(new Dictionary<string, string>()));
        //    return response.IsSuccessStatusCode;
        //}

        public static async Task<bool> NoticeUser(string url)
        {
            var client = ApiClient.Client;
            try
            {
                await client.OnlyGet(url);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> BlockUser(string url)
        {
            var client = ApiClient.Client;
            try
            {
                await client.OnlyGet(url);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //public static async Task<bool> DeleteNotification(int id, string token)
        //{
        //    var response = await Client.Center.PostAsync(new Uri($"{ApiBase}/delete/notification/{id}?once={token}"),
        //        new FormUrlEncodedContent(new Dictionary<string, string>()));
        //    return response.IsSuccessStatusCode;
        //}

        public static async Task<bool> CreateTopic(string nodeName, string content, string title)
        {
            var client = ApiClient.Client;
            var url = $"{ApiBase}/new/{nodeName}";
            try
            {
                var html = await client.OnlyGet(url);
                var once = new HtmlParser().Parse(html).QuerySelector("input[name='once']").GetAttribute("value");
                var param = new Dictionary<string, string>
                {
                    {"once", once},
                    {"content", content},
                    {"title", title}
                };
                await client.NewTopic(url, new FormUrlEncodedContent(param), nodeName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsLoginAsync()
        {
            var client = ApiClient.Client;
            try
            {
                var content = await client.GetMainPage();
                return new HtmlParser().Parse(content).GetElementById("Top").TextContent.Contains("登出");
            }
            catch
            {
                return false;
            }

            //return Client.CheckCookie("A2");
        }
    }
}