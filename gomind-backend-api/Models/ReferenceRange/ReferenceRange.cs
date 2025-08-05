using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.ReferenceRange
{
    // Enumeración para los tipos de condición permitidos
    public enum ConditionType
    {
        UNDEFINED,
        RANGE,
        LESS_THAN,
        LESS_THAN_EQUAL,
        GREATER_THAN,
        GREATER_THAN_EQUAL,
        EQUAL
    }
    public class ReferenceRange
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parameter_id")]
        public int ParameterId { get; set; }

        [JsonPropertyName("key_result")]
        public string KeyResult { get; set; }

        [JsonPropertyName("min_value")]
        public decimal? MinValue { get; set; }

        [JsonPropertyName("max_value")]
        public decimal? MaxValue { get; set; }

        [JsonPropertyName("condition_type")]
        public ConditionType ConditionType { get; set; } = ConditionType.RANGE;

        [JsonPropertyName("condition_value")]
        public decimal? ConditionValue { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("min_age")]
        public int? MinAge { get; set; }

        [JsonPropertyName("max_age")]
        public int? MaxAge { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; } = true;

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ReferenceRangeRequest
    {
        [Required]
        [JsonPropertyName("parameter_id")]       
        [Range(1, int.MaxValue, ErrorMessage = "El ID del parámetro debe ser un número positivo.")]        
        public int ParameterId { get; set; }

        [JsonPropertyName("key_result")]
        public string KeyResult { get; set; } = "Valor";

        [Required]
        [JsonPropertyName("condition_type")]
        [Range(1, 6)]
        [EnumDataType(typeof(ConditionType), ErrorMessage = "Tipo de condición inválido.")]
        public ConditionType ConditionType { get; set; } = ConditionType.RANGE;

        [JsonPropertyName("min_value")]
        public decimal? MinValue { get; set; }
        
        [JsonPropertyName("max_value")]
        public decimal? MaxValue { get; set; }        
       
        [JsonPropertyName("condition_value")]
        public decimal? ConditionValue { get; set; }
                
        [JsonPropertyName("gender")]
        public string? Gender { get; set; }
        
        [JsonPropertyName("min_age")]            
        public int? MinAge { get; set; }
        
        [JsonPropertyName("max_age")]           
        public int? MaxAge { get; set; }

        [Required]
        [JsonPropertyName("active")]
        public bool Active { get; set; } = true;

        [Required]
        [JsonPropertyName("notes")]
        public string Notes { get; set; }       
    }
}
