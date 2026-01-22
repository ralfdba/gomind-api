using Amazon.SQS;
using Amazon.SQS.Model;
using gomind_backend_api.BL;
using System.Text.Json;
using static gomind_backend_api.Models.Appointments.Appointments;

namespace gomind_backend_api.Services
{
    public class SQSConsumerWorker : BackgroundService
    {
        private readonly ILogger<SQSConsumerWorker> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;
        private readonly IServiceProvider _serviceProvider;

        public SQSConsumerWorker(
            ILogger<SQSConsumerWorker> logger,
            IAmazonSQS sqsClient,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _sqsClient = sqsClient;
            _queueUrl = configuration["AWS:SQS:QueueUrl"];
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQS Consumer Worker iniciado esperando mensajes...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Long Polling
                    var receiveRequest = new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 5,
                        WaitTimeSeconds = 20 
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

                    if (response.Messages != null) 
                    {
                        foreach (var message in response.Messages)
                        {
                            await ProcessMessageAsync(message, stoppingToken);
                            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                        }
                    }
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensajes de SQS.");
                    await Task.Delay(5000, stoppingToken); 
                }
            }
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken ct)
        {
            try
            {
                // 1. Deserializar el ID del mensaje enviado por el Productor (Cron)
                var messageData = JsonSerializer.Deserialize<AppointmentsConfirmedProducer>(message.Body);

                if (messageData == null || messageData.AppointmentId <= 0)
                {
                    _logger.LogWarning("Mensaje SQS vacío o ID inválido.");
                    return;
                }

                _logger.LogInformation("--- CONSUMIDOR: Procesando Cita ID: {Id} ---", messageData.AppointmentId);

                // 2. Crear un Scope manual para resolver el servicio Scoped (BL)
                using (var scope = _serviceProvider.CreateScope())
                {
                    var bl = scope.ServiceProvider.GetRequiredService<BL.BL>();

                    // 3. Ejecutar el método que ya tienes en la BL                    
                    var result = await bl.GetUpcomingAppointmentByIdAsync(messageData.AppointmentId);

                    if (result != null)
                    {
                        _logger.LogInformation("Notificación procesada para: {User} ({Email})", result.UserFullName, result.UserEmail);
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró detalle para la Cita ID: {Id}", messageData.AppointmentId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando el mensaje de la cita en el Worker.");              
                throw;
            }
        }
    }
}
