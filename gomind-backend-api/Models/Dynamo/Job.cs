using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Dynamo
{
    #region Status Job
    public static class JobStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";

        public static readonly Dictionary<string, string> StatusMap = new()   
        {        
            { Pending, "Pendiente" },       
            { Completed, "Completado" }    
        };

    }
    #endregion

    #region Mapeo a tabla job
    [DynamoDBTable("job")] 
    public class Job
    {
        [DynamoDBHashKey] 
        public string id { get; set; }

        [DynamoDBProperty]
        public int file_type { get; set; }        

        [DynamoDBProperty]
        public string job_status { get; set; }
        
        [DynamoDBProperty]
        public DateTime created_at { get; set; }

        [DynamoDBProperty]
        public DateTime updated_at { get; set; }

        [DynamoDBProperty]
        public int user_id { get; set; }

        [DynamoDBProperty]
        public string bucket_name { get; set; }

        [DynamoDBProperty]
        public string key_upload { get; set; }

        [DynamoDBProperty]
        public string? key_result { get; set; }

        [DynamoDBProperty]
        public bool success { get; set; }

        [DynamoDBProperty]
        public string? error_message { get; set; } 
        

    }
    #endregion

    #region Response
    public class JobDetailResponse
    {       
        [JsonPropertyName("file_url")]
        public string? FileUrl { get; set; }

        [JsonPropertyName("file_type")]
        public int FileType { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }
    public class JobResponse
    {
        [JsonPropertyName("job_id")]
        public string JobId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        [JsonPropertyName("response")]
        public JobDetailResponse? Response { get; set; }
    }
    #endregion
}
