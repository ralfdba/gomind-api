using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.User
{
    public class UserInfo
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonIgnore]
        [JsonPropertyName("company_id")]
        public int CompanyId { get; set; }
    }

    public class UserExist
    {
        [JsonPropertyName("user_exist")]
        public bool Exist { get; set; }        
        public string? Message { get; set; }
    }
}
