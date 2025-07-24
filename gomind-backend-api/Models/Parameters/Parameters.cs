using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static gomind_backend_api.Models.Examination.Examination;

namespace gomind_backend_api.Models.Parameters
{
    public class Parameters
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("unit_of_measure")]
        public string? UnitOfMeasure { get; set; }
    }
    public class ParametersRangeReference
    {      
        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("unit_of_measure")]
        public string? UnitOfMeasure { get; set; }
    }

    public class ParameterRequest
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [Required]
        [JsonPropertyName("unit_of_measure")]
        public string UnitOfMeasure { get; set; }
    }

    public class ResultParameterRangeReference
    {
        [JsonPropertyName("parameter")]
        public ParametersRangeReference Parameter { get; set; }

        [JsonPropertyName("analysis")]
        public Analysis Analysis { get; set; }
    }

}
