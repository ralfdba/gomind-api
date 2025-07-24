using System.Text.Json.Serialization;

namespace gomind_backend_api.Models.Utils
{
    #region Enum para tipos de Health Evaluation
    public enum HealthEvaluationType
    {
        Physical,
        Emotional,
        Finance,
        Goldberg
    }
    #endregion
    public static class Utils
    {
        #region Calcular IMC
        public static ImcResultResponse CalculateImc(decimal weightKg, decimal heightCm)
        {
            // Validar que la altura sea válida para evitar división por cero
            if (heightCm <= 0)
            {
                throw new ArgumentException("La altura debe ser mayor que cero para calcular el IMC.");
            }

            // Fórmula del IMC: peso (kg) / (altura (m))^2
            // Tu fórmula PHP usa (weight / (height * height)) * 10000.
            // El factor 10000 es porque la altura se pasa en cm y se convierte implícitamente a metros al multiplicar por 10000.
            // Ejemplo: 70 kg / (175 cm * 175 cm) * 10000 = 70 / (1.75 m * 1.75 m)
            decimal imcValue = (weightKg / (heightCm * heightCm)) * 10000m; // Usa 'm' para indicar decimal literal

            
            imcValue = Math.Round(imcValue, 1, MidpointRounding.AwayFromZero);

            ImcCategory category;

            if (imcValue <= 18.5m)
            {
                category = new ImcCategory { Title = "Bajo peso", Color = "e8dc1d" };
            }
            else if (imcValue >= 18.5m && imcValue <= 24.9m)
            {
                category = new ImcCategory { Title = "Peso normal", Color = "03c405" };
            }
            else if (imcValue >= 25.0m && imcValue <= 29.9m)
            {
                category = new ImcCategory { Title = "Sobrepeso", Color = "eb6117" };
            }
            else if (imcValue >= 30.0m && imcValue <= 34.9m)
            {
                category = new ImcCategory { Title = "Obesidad Tipo I", Color = "e90202" };
            }
            else if (imcValue >= 35.0m && imcValue <= 39.9m)
            {
                category = new ImcCategory { Title = "Obesidad Tipo II", Color = "e90202" };
            }
            else // imcValue >= 40.0m
            {
                category = new ImcCategory { Title = "Obesidad Tipo III", Color = "e90202" };
            }

            return new ImcResultResponse
            {
                ImcValue = imcValue,
                Category = category
            };
        }
        #endregion
    }

    #region IMC Entities
    public class ImcResultResponse
    {
        [JsonPropertyName("imc_value")]
        public decimal ImcValue { get; set; }

        [JsonPropertyName("category")]
        public ImcCategory Category { get; set; }
    }

    public class ImcCategory
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }
    }
    #endregion

}
