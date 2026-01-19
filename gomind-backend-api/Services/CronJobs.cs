using gomind_backend_api.BL;
using Quartz;
using System.Text.Json;

namespace gomind_backend_api.Services
{
    [DisallowConcurrentExecution]
    public class CronJobs : IJob
    {
        private readonly ILogger<CronJobs> _logger;
        private readonly BL.BL _bl;
        public CronJobs(ILogger<CronJobs> logger, BL.BL bl)
        {
            _logger = logger;
            _bl = bl;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando consulta de citas a las: {Time}", DateTimeOffset.Now);
                
                var appointments = await _bl.GetAppointmentsCurrentHourAsync();

                if (appointments != null && appointments.Any())
                {                    
                    string jsonResponse = JsonSerializer.Serialize(appointments, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    _logger.LogInformation("Citas encontradas ({Count}): {Data}", appointments.Count, jsonResponse);
                }
                else
                {
                    _logger.LogInformation("No se encontraron citas para la hora actual.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas en el CronJob.");
            }
        }
    }
}
