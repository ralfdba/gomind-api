using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Examination
{  
    public class Examination
    {
        public class ExaminationRequest
        {     
            [FromForm(Name = "file_type")]
            [Range (1,2)]
            public int FileType { get; set; }

            [FromForm(Name = "file")]
            public IFormFile File { get; set; }
        }
        public class ExaminationResponse
        {
            [JsonPropertyName("job_id")]
            public string JobId { get; set; }

            [JsonPropertyName("file_url")]
            public string FileUrl { get; set; }
        }
        public class AnalysisJobStatusResponse
        {
            [JsonPropertyName("job_id")]
            public string JobId { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; }
        }
        public class AnalysisRequest
        {
            [Required]
            [JsonPropertyName("result_id")]
            public int ResultId { get; set; }

            [Required]
            [JsonPropertyName("ai_recommendation")]
            public string AiRecommendation { get; set; }
        }       
        public class ParameterPlane
        {
            public string Nombre { get; set; }
            public string KeyResult { get; set; }
            public decimal Dato { get; set; }
        }
        public class Analysis
        {
            [JsonPropertyName("value")]
            public decimal Value { get; set; }

            [JsonPropertyName("results")]
            public List<string> Results { get; set; } = new();
        }

        public class AnalysisSaveResponse
        {
            public int NewRecommendationId { get; set; }

        }

    }
}
