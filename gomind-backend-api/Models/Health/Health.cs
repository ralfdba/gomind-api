using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Health
{
    public class Health
    {
        public class HealthProfileRequest
        {
            [Required]
            [JsonPropertyName("user_id")]
            public int UserId { get; set; }
            
            [JsonPropertyName("physical")]
            public PhysicalData? Physical { get; set; }

            [JsonPropertyName("emotional")]
            public EmotionalData? Emotional { get; set; }
            
            [JsonPropertyName("finance")]
            public FinanceData? Finance { get; set; }
            
            [JsonPropertyName("goldberg")]
            public GoldbergData? Goldberg { get; set; }
        }     

        public class HealthEvaluationResponse
        {           
            [JsonPropertyName("user_id")]
            public int UserId { get; set; }

            [JsonPropertyName("physical")]
            public HealthPhysicalData? Physical { get; set; }

            [JsonPropertyName("emotional")]
            public HealthEmotionalData? Emotional { get; set; }

            [JsonPropertyName("finance")]
            public HealthFinanceData? Finance { get; set; } 

            [JsonPropertyName("goldberg")]
            public HealthGoldbergData? Goldberg { get; set; } 
        }
    }
    #region Request
    public class PhysicalData
    {
        [JsonPropertyName("weight")]
        public int q1 { get; set; }

        [JsonPropertyName("height")]
        public int q2 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q3")]        
        public int q3 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q4")]
        public int q4 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q5")]
        public int q5 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q6")]
        public int q6 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q7")]
        public int q7 { get; set; }

        [Range(0, 5)]
        [JsonPropertyName("q8")]
        public int? q8 { get; set; } 
        
        [Range(0, 5)]
        [JsonPropertyName("q9")]
        public int? q9 { get; set; } 

        [Range(0, 5)]
        [JsonPropertyName("q10")]
        public int? q10 { get; set; } 
    }

    public class EmotionalData
    {
        [Range(1, 5)]
        [JsonPropertyName("stress_level")]
        public int q1 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q2")]
        public int q2 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q3")]
        public int q3 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q4")]
        public int q4 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q5")]
        public int q5 { get; set; } 

        [Range(1, 5)]
        [JsonPropertyName("q6")]
        public int q6 { get; set; } 

        [Range(1, 5)]
        [JsonPropertyName("q7")]
        public int q7 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q8")]
        public int q8 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q9")]
        public int q9 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q10")]
        public int q10 { get; set; }
    }

    public class FinanceData
    {
        [Range(1, 5)]
        [JsonPropertyName("savings")]
        public int q1 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("debt")]
        public int q2 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q3")]
        public int q3 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q4")]
        public int q4 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q5")]
        public int q5 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q6")]
        public int q6 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q7")]
        public int? q7 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q8")]
        public int? q8 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q9")]
        public int? q9 { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("q10")]
        public int? q10 { get; set; }

    }

    public class GoldbergData
    {
        [Range(0, 3)]
        [JsonPropertyName("q1")]
        public int q1 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q2")]
        public int q2 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q3")]
        public int q3 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q4")]
        public int q4 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q5")]
        public int q5 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q6")]
        public int q6 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q7")]
        public int q7 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q8")]
        public int q8 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q9")]
        public int q9 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q10")]
        public int q10 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q11")]
        public int q11 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q12")]
        public int q12 { get; set; }

        [Range(0, 3)]
        [JsonPropertyName("q13")]
        public int? q13 { get; set; }
    }
    #endregion

    #region Response

    #region Generic Items
    public class ItemDetails
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("desc")]
        public string Desc { get; set; }
        [JsonPropertyName("advice")]
        public string Advice { get; set; }
        [JsonPropertyName("color")]
        public string Color { get; set; }
        [JsonPropertyName("face")]
        public string Face { get; set; }
    }
    public class AllHealthResources
    {
        [JsonPropertyName("Physical")]
        public PhysicalJsonSection Physical { get; set; }
        [JsonPropertyName("Emotional")]
        public EmotionalJsonSection Emotional { get; set; }
        [JsonPropertyName("Finance")]
        public FinanceJsonSection Finance { get; set; }
        [JsonPropertyName("Goldberg")]
        public GoldbergJsonSection Goldberg { get; set; }
    }
    public class PhysicalJsonSection
    {
        [JsonPropertyName("Item1")]
        public ItemDetails Item1 { get; set; }
        [JsonPropertyName("Item2")]
        public ItemDetails Item2 { get; set; }
        [JsonPropertyName("Item3")]
        public ItemDetails Item3 { get; set; }
        [JsonPropertyName("Item4")]
        public ItemDetails Item4 { get; set; }
    }
    public class EmotionalJsonSection
    {
        [JsonPropertyName("Item1")]
        public ItemDetails Item1 { get; set; }
        [JsonPropertyName("Item2")]
        public ItemDetails Item2 { get; set; }
        [JsonPropertyName("Item3")]
        public ItemDetails Item3 { get; set; }
        [JsonPropertyName("Item4")]
        public ItemDetails Item4 { get; set; }
    }
    public class FinanceJsonSection
    {
        [JsonPropertyName("Item1")]
        public ItemDetails Item1 { get; set; }
        [JsonPropertyName("Item2")]
        public ItemDetails Item2 { get; set; }
        [JsonPropertyName("Item3")]
        public ItemDetails Item3 { get; set; }
        [JsonPropertyName("Item4")]
        public ItemDetails Item4 { get; set; }
    }
    public class GoldbergJsonSection
    {
        [JsonPropertyName("Item1")]
        public ItemDetails Item1 { get; set; }
        [JsonPropertyName("Item2")]
        public ItemDetails Item2 { get; set; }
        [JsonPropertyName("Item3")]
        public ItemDetails Item3 { get; set; }
        [JsonPropertyName("Item4")]
        public ItemDetails Item4 { get; set; }
    }
    #endregion

    #region Physical

    public class PhysicalGenericData
    {
        [JsonPropertyName("weight")] 
        public int Weight { get; set; }

        [JsonPropertyName("height")] 
        public int Height { get; set; }

        [JsonPropertyName("imc")] 
        public ImcResultResponse Imc { get; set; }

        [JsonPropertyName("result")] 
        public int Result { get; set; }

        [JsonPropertyName("item_details")]
        public ItemDetails ItemDetails { get; set; }
    }

    public class HealthPhysicalData
    {
        [JsonPropertyName("initial_data")]
        public PhysicalGenericData? InitialData { get; set; }

        [JsonPropertyName("current_data")]
        public PhysicalGenericData? CurrentData { get; set; }
    }
    #endregion

    #region Emotional

    public class EmotionalGenericData
    {     
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("item_details")]
        public ItemDetails ItemDetails { get; set; }
    }

    public class HealthEmotionalData
    {
        [JsonPropertyName("initial_data")]
        public EmotionalGenericData InitialData { get; set; }

        [JsonPropertyName("current_data")]
        public EmotionalGenericData CurrentData { get; set; }
    }
    #endregion

    #region Finance

    public class FinanceGenericData
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("item_details")]
        public ItemDetails ItemDetails { get; set; }
    }

    public class HealthFinanceData
    {
        [JsonPropertyName("initial_data")]
        public FinanceGenericData InitialData { get; set; }

        [JsonPropertyName("current_data")]
        public FinanceGenericData CurrentData { get; set; }
    }
    #endregion

    #region Goldberg

    public class GoldbergGenericData
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("item_details")]
        public ItemDetails ItemDetails { get; set; }
    }

    public class HealthGoldbergData
    {
        [JsonPropertyName("initial_data")]
        public GoldbergGenericData InitialData { get; set; }

        [JsonPropertyName("current_data")]
        public GoldbergGenericData CurrentData { get; set; }
    }
    #endregion

    #endregion
}
