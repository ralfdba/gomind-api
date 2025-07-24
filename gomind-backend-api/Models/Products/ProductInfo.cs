using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Products
{
    public class ProductInfo
    {
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
