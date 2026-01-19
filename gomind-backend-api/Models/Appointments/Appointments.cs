using System.Text.Json.Serialization;
using static gomind_backend_api.BL.BL;

namespace gomind_backend_api.Models.Appointments
{
    public class Appointments
    {
        public class AppointmentsRequest
        {      
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
            public string ScheduleDay { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("health_provider")]
            public string? HealthProvider { get; set; }

            [JsonPropertyName("product")]
            public string? Product { get; set; }

        }       
        public class AppointmentDetail
        {
            [JsonPropertyName("id")]
            public int AppointmentId { get; set; } = 0;
            [JsonPropertyName("user_id")]
            public int UserId { get; set; }
            [JsonPropertyName("schedule_day")]
            public string ScheduleDay { get; set; }
            [JsonPropertyName("state")]
            public string State { get; set; } 
            [JsonPropertyName("health_provider")]
            public string HealthProvider { get; set; }
            [JsonPropertyName("product")]
            public string Product { get; set; }
        }
        public class AppointmentsConfirmedByUsers
        {
            [JsonPropertyName("id")]
            public int AppointmentId { get; set; } = 0;
            [JsonPropertyName("user_id")]
            public int UserId { get; set; }
            [JsonPropertyName("user_email")]
            public string UserEmail { get; set; }
            [JsonPropertyName("schedule_day")]
            public string ScheduleDay { get; set; }
            [JsonPropertyName("state")]
            public int StateId { get; set; }
            [JsonPropertyName("state_name")]
            public string StateName { get; set; }
            [JsonPropertyName("health_provider")]
            public string HealthProvider { get; set; }
            [JsonPropertyName("product")]
            public string Product { get; set; }
        }

        public static class AppointmentExtensions
        {
            private static readonly Dictionary<AppointmentState, string> StateNames = new()
            {
                { AppointmentState.Solicitado, "Solicitado" },
                { AppointmentState.Confirmada, "Cita confirmada" },
                { AppointmentState.Cancelada, "Cita cancelada" }
            };

            public static string GetStateName(int stateId)
            {
                var state = (AppointmentState)stateId;
                return StateNames.TryGetValue(state, out var name) ? name : "Desconocido";
            }
        }

    }
}
