using gomind_backend_api.Models.Company;
using gomind_backend_api.Models.User;
using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Login
{
    public class Login
    {
        public class LoginRequest
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }
        }

        public class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("user")]
            public UserInfo User { get; set; }

            [JsonPropertyName("company")]
            public CompanyInfo Company { get; set; }
        }
    }
   
}
