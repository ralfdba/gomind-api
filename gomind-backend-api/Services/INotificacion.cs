using gomind_backend_api.Models.Utils;

namespace Services
{
    public interface INotificacion
    {
        Status EnvioCodigoVerificacion(string codigoVerificacion, Destinatario destinatario);
    }
}