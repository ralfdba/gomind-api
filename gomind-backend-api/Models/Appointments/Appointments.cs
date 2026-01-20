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
        public class AppointmentsConfirmedNotifier
        {
            [JsonPropertyName("id")]
            public int AppointmentId { get; set; } = 0;
            [JsonPropertyName("user_full_name")]
            public string UserFullName { get; set; }
            [JsonPropertyName("user_email")]
            public string UserEmail { get; set; }
            [JsonPropertyName("schedule_day")]
            public string ScheduleDay { get; set; }
            [JsonPropertyName("formatted_date")]
            public string FormattedDate { get; set; }
            [JsonPropertyName("day_name")]
            public string DayName { get; set; }
            [JsonPropertyName("day_number")]
            public string DayNumber { get; set; }
            [JsonPropertyName("month_name")]
            public string MonthName { get; set; }
            [JsonPropertyName("year")]
            public string Year { get; set; }
            [JsonPropertyName("time")]
            public string Time { get; set; }   
            [JsonPropertyName("state")]
            public int StateId { get; set; }
            [JsonPropertyName("state_name")]
            public string StateName { get; set; }
            [JsonPropertyName("health_provider")]
            public string HealthProvider { get; set; }
            [JsonPropertyName("product")]
            public string Product { get; set; }
        }

        public class AppointmentsConfirmedProducer
        {
            [JsonPropertyName("id")]
            public int AppointmentId { get; set; } = 0;          
           
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
