using gomind_backend_api.Models.Products;
using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Company
{
    public class CompanyInfo
    {
        [JsonPropertyName("company_id")]
        public int CompanyId { get; set; }
        public ProductInfo[] Products { get; set; }
    }
}
