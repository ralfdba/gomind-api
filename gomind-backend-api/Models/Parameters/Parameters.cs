using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
        public required List<KeyResultDetail> KeysResults { get; set; } = new();
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
        public required List<KeyResultDetail> KeysResults { get; set; }
    }
    public class KeyResultDetail
    {
        [JsonPropertyName("key")]
        public required string Key { get; set; }

        [JsonPropertyName("invalid_ranges")]
        public List<string>? InvalidRanges { get; set; }
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


    #region Parameters Result V2
    public class ExaminationAnalysis
    {
        [JsonPropertyName("metadata")]
        public AnalysisMetadata Metadata { get; set; } = new();

        [JsonPropertyName("parameters_found")]
        public List<AnalysisParameter> ParametersFound { get; set; } = new();

        [JsonPropertyName("parameters_out_of_range")]
        public List<AnalysisParameter> ParametersOutOfRange { get; set; } = new();
    }

    public class AnalysisMetadata
    {
        [JsonPropertyName("job_id")]
        public string JobId { get; set; } = string.Empty;

        [JsonPropertyName("analysis_date")]
        public string AnalysisDate { get; set; } = string.Empty;

        [JsonPropertyName("parameters_found_count")]
        public int ParametersFoundCount { get; set; }

        [JsonPropertyName("parameters_out_of_range_count")]
        public int ParametersOutOfRangeCount { get; set; }
    }

    public class AnalysisParameter
    {        

        [JsonPropertyName("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("unit_of_measure")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [JsonPropertyName("analysis")]
        public List<AnalysisMetric> Analysis { get; set; } = new();
    }

    public class AnalysisMetric
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("reference_ranges")]
        public List<string> ReferenceRanges { get; set; } = new();
    }

    public class RawParameter
    {
        public string Parameter { get; set; }
        public string KeyResult { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public List<string> ReferenceRanges { get; set; } = new();
    }

    public class ParameterConfig
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("invalid_ranges")]
        public List<string>? InvalidRanges { get; set; }
    }

    public static class RangeEvaluator
    {
        public static bool IsValueValid(decimal value, List<string> referenceRanges, List<ParameterConfig>? configs)
        {
            if (referenceRanges == null || !referenceRanges.Any()) return true;

            var invalidPrefixes = configs?
                .SelectMany(c => c.InvalidRanges ?? new List<string>())
                .Select(r => r.ToLowerInvariant().Trim())
                .ToList() ?? new List<string>();

            bool foundNormalRange = false;
            bool valueIsInNormalRange = false;

            foreach (var rangeStr in referenceRanges)
            {
                var lowerRange = rangeStr.ToLowerInvariant();
                var parts = lowerRange.Split(':');
                string label = parts.Length > 1 ? parts[0].Trim() : "";
                string rangeCondition = parts.Last().Trim();

                // 1. CHEQUEO DE RIESGOS CONFIGURADOS (Prioridad máxima)
                if (invalidPrefixes.Any(p => label.Contains(p)))
                {
                    if (IsValueMatchingCondition(value, rangeCondition))
                    {
                        return false; 
                    }
                }

                // 2. CHEQUEO DE NORMALIDAD
                if (label.Contains("normal") || string.IsNullOrEmpty(label))
                {
                    foundNormalRange = true;
                    if (IsValueMatchingCondition(value, rangeCondition))
                    {
                        valueIsInNormalRange = true;
                    }
                }
            }
            
            // Si se encuentra un rango normal y el valor NO está en él -> Invalido
            if (foundNormalRange && !valueIsInNormalRange)
            {
                return false;
            }

            return true;
        }

        private static bool IsValueMatchingCondition(decimal value, string conditionStr)
        {
            try
            {
                // Limpieza robusta
                var cleanCond = conditionStr.ToLower()
                    .Replace("%", "")
                    .Replace("g/dl", "")
                    .Replace("mg/dl", "")
                    .Replace("mm/hora", "")
                    .Replace("x10^3/mm3", "")
                    .Replace("x10^12/ul", "")
                    .Trim();

                // 1. Rango con guion (ej: "11.5 - 14.5")
                if (cleanCond.Contains("-"))
                {
                    var parts = cleanCond.Split('-');
                    if (parts.Length == 2)
                    {
                        decimal min = ParseDecimal(parts[0].Trim());
                        decimal max = ParseDecimal(parts[1].Trim());
                        return value >= min && value <= max;
                    }
                }

                // 2. Operadores de comparación (Importante el orden de >= antes de >)
                if (cleanCond.Contains(">="))
                    return value >= ParseDecimal(cleanCond.Replace(">=", ""));

                if (cleanCond.Contains("<="))
                    return value <= ParseDecimal(cleanCond.Replace("<=", ""));

                if (cleanCond.Contains(">"))
                    return value > ParseDecimal(cleanCond.Replace(">", ""));

                if (cleanCond.Contains("<"))
                    return value < ParseDecimal(cleanCond.Replace("<", ""));

                // 3. Caso de valor único (ej: "120")
                if (decimal.TryParse(cleanCond, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal singleValue))
                {
                    return value == singleValue;
                }
            }
            catch
            {                
                return false;
            }

            return false;
        }
        private static decimal ParseDecimal(string input)
        {
            return decimal.Parse(input.Trim(), CultureInfo.InvariantCulture);
        }
    }

    #endregion
}
