using gomind_backend_api.Models.Utils;
using System.Net.Mail;

namespace Services
{
    public interface IEnvioCorreoService
    {
        void Enviar(List<Reemplazar> listaReemplazo, string correoAsunto, List<Destinatario> listaDestinatarios, string rutaTemplate, List<Attachment>? attachments = null, string CC = "", string BCC = "");
    }
}