using System.Text.Json.Serialization;

namespace AiSuite.Models.DTOs
{
    public class ModelMetadataDto
    {
        [JsonPropertyName("baseModel")]
        public string BaseModel { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public ModelDetailDto Model { get; set; } = new();
    }
}