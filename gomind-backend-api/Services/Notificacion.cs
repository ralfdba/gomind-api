using gomind_backend_api.Models.Utils;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static gomind_backend_api.Models.Appointments.Appointments;

namespace Services
{
    public class Notificacion : INotificacion
    {        
        private readonly IEnvioCorreoService _envioCorreoService;
        public Notificacion(IEnvioCorreoService envioCorreoService)
        {
            _envioCorreoService = envioCorreoService;           
        }

        public Status EnvioCodigoVerificacion(string codigoVerificacion, Destinatario destinatario)
        {
            var result = new Status();
           
            try
            {   
                List<Destinatario> destinatarios = new List<Destinatario>() { destinatario };
                List<Reemplazar> reemplazos = new List<Reemplazar>();                
                reemplazos.Add(new Reemplazar() { TextoBuscar = "[CODIGO]", TextoReemplazar = codigoVerificacion.ToString() });
                
                _envioCorreoService.Enviar(reemplazos, $"Tu código de acceso a Gomind: {codigoVerificacion}", destinatarios, "envio-codigo.html");
                
                result.IsOK = true;
            }
            catch (Exception ex)
            {
                result.IsOK = false;
                result.Mensaje = ex.Message;
            }

            return result;
        }

        public Status EnvioRecordatorioAgendamiento(AppointmentsConfirmedNotifier appointment, Destinatario destinatario)
        {
            var result = new Status();
            string fechaFormateada = $"{appointment.DayName}, {appointment.DayNumber} de {appointment.MonthName}";
            string modalidad = $"{appointment.Product} / {appointment.HealthProvider}";
            try
            {
                List<Destinatario> destinatarios = new List<Destinatario>() { destinatario };
                List<Reemplazar> reemplazos = new List<Reemplazar>();
                reemplazos.Add(new Reemplazar() { TextoBuscar = "[NOMBRE_USUARIO]", TextoReemplazar = appointment.UserFullName });
                reemplazos.Add(new Reemplazar() { TextoBuscar = "[FECHA]", TextoReemplazar = fechaFormateada });
                reemplazos.Add(new Reemplazar() { TextoBuscar = "[HORA]", TextoReemplazar = appointment.Time });
                reemplazos.Add(new Reemplazar() { TextoBuscar = "[MODALIDAD]", TextoReemplazar = modalidad });

                _envioCorreoService.Enviar(reemplazos, $"Recordatorio de tu cita médica, {appointment.ScheduleDay}", destinatarios, "recordatorio-agendamiento.html");

                result.IsOK = true;
            }
            catch (Exception ex)
            {
                result.IsOK = false;
                result.Mensaje = ex.Message;
            }

            return result;
        }
    }
}
