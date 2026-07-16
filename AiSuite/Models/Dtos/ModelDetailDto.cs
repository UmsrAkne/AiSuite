using System.Text.Json.Serialization;

namespace AiSuite.Models.DTOs
{
    public class ModelDetailDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}