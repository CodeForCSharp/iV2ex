using System.Text.Json.Serialization;

namespace iV2EX.Model
{
    public class NodeModel
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("topics")] public int Topics { get; set; }

        [JsonPropertyName("header")] public string Header { get; set; }

        [JsonPropertyName("footer")] public string Footer { get; set; }

        public string Image { get; set; }

        public string IsCollect { get; set; }

        public string Cover { get; set; }
    }
}