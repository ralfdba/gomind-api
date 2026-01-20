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
                _logger.LogInformation("Iniciando consulta de citas (Producer) a las: {Time}", DateTimeOffset.Now);

                var appointments = await _bl.GetAppointmentsCurrentHourAsync();

                if (appointments != null && appointments.Any())
                {
                    _logger.LogInformation("Se encontraron {Count} citas. Procesando registros individuales...", appointments.Count);

                    foreach (var appo in appointments)
                    {           
                        //Incorporar metodo para publicar el SQS
                        _logger.LogInformation("Procesando Cita ID: {AppointmentId}", appo.AppointmentId);                        
                    }

                    _logger.LogInformation("Finalizó el procesamiento de la lista actual.");
                }
                else
                {
                    _logger.LogInformation("No se encontraron citas para la hora actual.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar el listado de AppointmentsConfirmedProducer.");
            }
        }
    }
}
