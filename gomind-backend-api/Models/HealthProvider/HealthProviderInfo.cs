using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.HealthProvider
{
    public class HealthProviderInfo
    {
        [JsonPropertyName("health_provider_id")]
        public int HealthProviderId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
