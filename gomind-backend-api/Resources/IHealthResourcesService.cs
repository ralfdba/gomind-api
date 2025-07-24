using gomind_backend_api.Models.Health;
using System.Reflection;
using System.Text.Json;

namespace gomind_backend_api.Resources
{
    public interface IHealthResourcesService
    {
        Task<AllHealthResources> GetAllResourcesAsync();
    }

    // Services/HealthResourcesService.cs
    public class HealthResourcesService : IHealthResourcesService
    {
        private AllHealthResources _cachedResources;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<AllHealthResources> GetAllResourcesAsync()
        {
            if (_cachedResources != null)
            {
                return _cachedResources;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_cachedResources == null)
                {
                    // Lógica para leer el archivo JSON desde "Resources"
                    string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string jsonFilePath = Path.Combine(assemblyLocation, "Resources", "Health_Resources.json");

                    if (!File.Exists(jsonFilePath))
                    {
                        jsonFilePath = Path.Combine(assemblyLocation, "Health_Resources.json"); // Fallback
                        if (!File.Exists(jsonFilePath))
                        {
                            throw new FileNotFoundException($"El archivo Health_Resources.json no fue encontrado en: {Path.Combine(assemblyLocation, "Resources")} ni en {assemblyLocation}.");
                        }
                    }

                    string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                    _cachedResources = JsonSerializer.Deserialize<AllHealthResources>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Console.WriteLine("Archivo Health_Resources.json cargado exitosamente en memoria.");
                }
            }
            finally
            {
                _semaphore.Release();
            }
            return _cachedResources;
        }
    }
}
