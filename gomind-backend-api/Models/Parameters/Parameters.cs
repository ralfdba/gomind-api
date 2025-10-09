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
       
        [JsonPropertyName("keys_results")]
        public List<string>? KeysResults { get; set; }
    }
    public class ParameterRequest
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; }
       
        [JsonPropertyName("unit_of_measure")]
        public string? UnitOfMeasure { get; set; }

        [Required]
        [JsonPropertyName("keys_results")]
        public List<string> KeysResults { get; set; }
    }
    public class ResultParameterRangeReference
    {
        [JsonPropertyName("parameter")]
        public ParametersAnalysis Parameter { get; set; }

        [JsonPropertyName("analysis")]
        public Analysis Analysis { get; set; }
    }

    #region DTOs Obtener Analisis de Parametros
    public class AnalysisResult
    {
        [JsonPropertyName("parameter")]
        public ParametersAnalysis Parameter { get; set; }

        [JsonPropertyName("analysis")]
        public AnalysisDetails Analysis { get; set; }
    }
    public class ParametersAnalysis
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("unit_of_measure")]
        public string? UnitOfMeasure { get; set; }
    }    
    public class AnalysisDetails
    {
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("reference_ranges")]
        public List<ReferenceRangeAnalysis> ReferenceRanges { get; set; } = new List<ReferenceRangeAnalysis>();

        [JsonPropertyName("results")]
        public List<ResultDetail> Results { get; set; } = new List<ResultDetail>();
    }
    public class ReferenceRangeAnalysis
    {
        // Las propiedades MinValue y MaxValue son opcionales según el condition_type
        [JsonPropertyName("condition_type")]
        public string ConditionType { get; set; }

        [JsonPropertyName("condition_value")]
        public decimal? ConditionValue { get; set; }

        [JsonPropertyName("min_value")]
        public decimal? MinValue { get; set; }

        [JsonPropertyName("max_value")]
        public decimal? MaxValue { get; set; }

        [JsonPropertyName("key_result")]
        public string KeyResult { get; set; }
    }
    public class ResultDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } 

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; } 
    }
    public class SpParameterResult
    {
        // Propiedades de parameter_results
        public int parameter_result_id { get; set; }
        public string file_key { get; set; }
        public decimal value { get; set; }
        public string analysis_results { get; set; }
        public int reference_range_id { get; set; }

        // Propiedades de reference_ranges
        public decimal? min_value { get; set; } 
        public decimal? max_value { get; set; } 
        public int condition_type { get; set; } 
        public decimal? condition_value { get; set; } 
        public string reference_range_key_result { get; set; }       

        // Propiedades de condition_types
        public string condition_type_description { get; set; }

        // Propiedades de parameters
        public int parameter_id { get; set; }
        public string parameter_name { get; set; }
        public string parameter_description { get; set; }
        public string unit_of_measure { get; set; }
        public string parameter_uuid { get; set; } 
    }

    #endregion

    #region Parameters Result 
    public class ParameterResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("file_key")]
        public string FileKey { get; set; }

        [JsonPropertyName("value")]
        public Decimal Value { get; set; }

        [JsonPropertyName("analysis_results")]
        public string? AnalysisResults { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("reference_range_id")]
        public int ReferenceRangeId { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("parameter_id")]
        public int ParameterId { get; set; }

        [JsonPropertyName("reference_range_min")]
        public string? ReferenceRangeMin { get; set; }

        [JsonPropertyName("reference_range_max")]
        public string? ReferenceRangeMax { get; set; }
    }
    #endregion

}
