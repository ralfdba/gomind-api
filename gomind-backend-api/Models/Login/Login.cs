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
            public required string Email { get; set; }

            [JsonPropertyName("password")]
            public required string Password { get; set; }
        }

        public class AuthRequestByEmail
        {
            [JsonPropertyName("email")]
            public required string Email { get; set; }            
        }
        public class LoginRequestAuthCode
        {
            [JsonPropertyName("email")]
            public required string Email { get; set; }

            [JsonPropertyName("auth_code")]
            public required int AuthCode { get; set; }
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
