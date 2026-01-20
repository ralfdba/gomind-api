using gomind_backend_api.Models.Utils;
using static gomind_backend_api.Models.Appointments.Appointments;

namespace Services
{
    public interface INotificacion
    {
        Status EnvioCodigoVerificacion(string codigoVerificacion, Destinatario destinatario);
        Status EnvioRecordatorioAgendamiento(AppointmentsConfirmedNotifier appointment, Destinatario destinatario);
    }
}