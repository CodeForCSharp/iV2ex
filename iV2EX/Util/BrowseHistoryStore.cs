using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using iV2EX.Model;

namespace iV2EX.Util
{
    [JsonSerializable(typeof(List<BrowseHistoryItem>))]
    internal partial class BrowseHistoryJsonContext : JsonSerializerContext
    {
    }

    internal static class BrowseHistoryStore
    {
        private const int MaxItems = 200;
        private const string FileName = "browse_history.json";
        private static readonly SemaphoreSlim Gate = new(1, 1);

        public static async Task AddAsync(TopicModel topic)
        {
            if (topic == null || topic.Id <= 0 || string.IsNullOrEmpty(topic.Title)) return;
            try
            {
                await MutateAsync(items =>
                {
                    items.RemoveAll(x => x.Id == topic.Id);
                    items.Insert(0, new BrowseHistoryItem
                    {
                        Id = topic.Id,
                        Title = topic.Title,
                        Username = topic.Member?.Username,
                        Image = topic.Member?.Image,
                        NodeName = topic.NodeName,
                        ViewedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    });
                    if (items.Count > MaxItems)
                        items.RemoveRange(MaxItems, items.Count - MaxItems);
                });
            }
            catch
            {
            }
        }

        public static async Task<List<BrowseHistoryItem>> GetAllAsync()
        {
            await Gate.WaitAsync();
            try
            {
                return await ReadAsync();
            }
            finally
            {
                Gate.Release();
            }
        }

        public static Task RemoveAsync(int id) => MutateAsync(items => items.RemoveAll(x => x.Id == id));

        public static Task ClearAsync() => MutateAsync(items => items.Clear());

        private static async Task MutateAsync(Action<List<BrowseHistoryItem>> mutate)
        {
            await Gate.WaitAsync();
            try
            {
                var items = await ReadAsync();
                mutate(items);
                await WriteAsync(items);
            }
            finally
            {
                Gate.Release();
            }
        }

        private static async Task<List<BrowseHistoryItem>> ReadAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    FileName, CreationCollisionOption.OpenIfExists);
                var json = await FileIO.ReadTextAsync(file);
                if (string.IsNullOrWhiteSpace(json)) return new List<BrowseHistoryItem>();
                return JsonSerializer.Deserialize(json, BrowseHistoryJsonContext.Default.ListBrowseHistoryItem)
                       ?? new List<BrowseHistoryItem>();
            }
            catch
            {
                return new List<BrowseHistoryItem>();
            }
        }

        private static async Task WriteAsync(List<BrowseHistoryItem> items)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                FileName, CreationCollisionOption.ReplaceExisting);
            var json = JsonSerializer.Serialize(items, BrowseHistoryJsonContext.Default.ListBrowseHistoryItem);
            await FileIO.WriteTextAsync(file, json);
        }
    }
}
