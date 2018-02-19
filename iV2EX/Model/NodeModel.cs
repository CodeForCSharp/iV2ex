using Newtonsoft.Json;

namespace iV2EX.Model
{
    public class NodeModel
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        public int Topics { get; set; }

        [JsonProperty("header")] public string Header { get; set; }

        [JsonProperty("footer")] public string Footer { get; set; }

        public string Image { get; set; }

        public string IsCollect { get; set; }

        public string Cover { get; set; }
    }
}