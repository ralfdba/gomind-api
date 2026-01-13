using gomind_backend_api.Models.Utils;
using System.Threading.Tasks;

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
                
                _envioCorreoService.Enviar(reemplazos, "Código de autenticación acceso Gomind", destinatarios, "envio-codigo.html");
                
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
