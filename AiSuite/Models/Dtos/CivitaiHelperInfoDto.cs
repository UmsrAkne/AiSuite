using System.Text.Json.Serialization;

namespace AiSuite.Models.DTOs
{
    public class CivitaiHelperInfoDto
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("activation text")]
        public string ActivationText { get; set; } = string.Empty;
    }
}