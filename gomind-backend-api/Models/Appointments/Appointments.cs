using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Appointments
{
    public class Appointments
    {
        public class AppointmentsRequest
        {
            [JsonPropertyName("user_id")]
            public int UserId { get; set; }

            [JsonPropertyName("product_id")]
            public int ProductId { get; set; }

            [JsonPropertyName("health_provider_id")]
            public int HealthProviderId { get; set; }

            [JsonPropertyName("date_time")]
            public DateTime DateTime { get; set; }
        }

        public class AppointmentsResponse
        {
            [JsonPropertyName("appointment_id")]
            public int AppointmentId { get; set; }

        }
        public class AppointmentsByUser
        {
            [JsonPropertyName("id")]
            public int AppointmentId { get; set; }

            [JsonPropertyName("schedule_day")]
            public DateTime ScheduleDay { get; set; }

            [JsonPropertyName("health_provider")]
            public string? HealthProvider { get; set; }

            [JsonPropertyName("product")]
            public string? Product { get; set; }

        }
    }
}
