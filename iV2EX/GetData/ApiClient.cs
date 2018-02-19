using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using iV2EX.Model;
using Newtonsoft.Json;

namespace iV2EX.GetData
{
    internal static class ApiClient
    {
        public const string Host = "https://www.v2ex.com";

        public static HttpClientHandler Handler { get; } = new HttpClientHandler();

        public static HttpClient Client { get; } = new HttpClient(Handler)
        {
            DefaultRequestHeaders =
            {
                { "Accept-Language", "zh-CN"},
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586"}
            },
            Timeout = TimeSpan.FromMilliseconds(200000)
        };

        public static async Task<string> GetCheckInInformation() => await Client.GetStringAsync($"{Host}/mission/daily");

        public static async Task<string> CheckIn(string url, string referer)
        {
            var request = new HttpRequestMessage
            {
                Headers = {{"Referer", referer}},
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            return await Client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public static async Task<string> SignIn(FormUrlEncodedContent content)
        {
            var request = new HttpRequestMessage
            {
                Headers = { { "Referer", $"{Host}/signin" } },
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Host}/signin"),
                Content = content
            };
            return await Client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetSignInInformation() => await Client.GetStringAsync($"{Host}/signin");

        public static async Task<string> GetMainPage() => await Client.GetStringAsync(Host);

        public static async Task<string> GetTopicInformation(int id) => await Client.GetStringAsync($"{Host}/t/{id}");

        public static async Task<string> ReplyTopic(string referer, FormUrlEncodedContent content, int id)
        {
            var request = new HttpRequestMessage
            {
                Headers = { { "Referer", $"{Host}/t/{id}" } },
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Host}/t/{id}"),
                Content = content
            };
            return await Client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetNodeInformation(string nodeName) => await Client.GetStringAsync($"{Host}/go/{nodeName}");

        public static async Task<string> OnlyGet(string url) => await Client.GetStringAsync(url);

        public static async Task<string> NewTopic(string referer, FormUrlEncodedContent content, string nodeName)
        {
            var request = new HttpRequestMessage
            {
                Headers = { { "Referer", $"{Host}/new/{nodeName}" } },
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Host}/new/{nodeName}"),
                Content = content
            };
            return await Client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public static async Task<List<NodeModel>> GetNodes()
        {
            var json = await Client.GetStringAsync($"{Host}/api/nodes/all.json");
            return JsonConvert.DeserializeObject<List<NodeModel>>(json);
        }

        public static async Task<string> GetTopicsWithPageN(string nodeName, int p) => await Client.GetStringAsync($"{Host}/go/{nodeName}?p={p}");

        public static async Task<string> GetTopicsWithTab(string tab) => await Client.GetStringAsync($"{Host}?tab={tab}");

        public static async Task<string> GetFavoriteNodes() => await Client.GetStringAsync($"{Host}/my/nodes");

        public static async Task<string> GetFavoriteTopics(int p) => await Client.GetStringAsync($"{Host}/my/topics?p={p}");

        public static async Task<string> GetFollowerTopics(int p) => await Client.GetStringAsync($"{Host}/my/following?p={p}");

        public static async Task<string> GetNotifications(int p) => await Client.GetStringAsync($"{Host}/notifications?p={p}");

        public static async Task<string> GetRepliesAndTopicContent(int id, int p) => await Client.GetStringAsync($"{Host}/t/{id}?p={p}");

        public static async Task<string> GetTopicsByUsername(string username, int p) => await Client.GetStringAsync($"{Host}/member/{username}/topics?p={p}");

        public static async Task<string> GetRepliesByUsername(string username, int p) => await Client.GetStringAsync($"{Host}/member/{username}/replies?p={p}");

        public static async Task<string> GetMemberInformation(string username) => await Client.GetStringAsync($"{Host}/member/{username}");

        public static async Task<string> GetMoneyDetail(int p) => await Client.GetStringAsync($"{Host}/balance?p={p}");

        public static async Task<Stream> GetStream(string url) => await Client.GetStreamAsync(url);

        public static async Task<string> GetSettingInformation() => await Client.GetStringAsync($"{Host}/settings");

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
}