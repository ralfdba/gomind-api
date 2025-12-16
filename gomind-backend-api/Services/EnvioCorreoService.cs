using gomind_backend_api.Models.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Services
{
    public class EnvioCorreoService : IEnvioCorreoService
    {
        private readonly ILogger<EnvioCorreoService> _logger;
        private IConfiguration _configuration;
        private string _smtpServer;
        private string _smtpUser;
        private string _smtpPassword;
        private readonly CorreoFromOptions _correoFromOptions;
        private string _pathTemplates;
        private List<string> _correoQA;
        private bool _esQA;
        private string _contentRootPath;

        public EnvioCorreoService(ILogger<EnvioCorreoService> logger, IWebHostEnvironment env, IConfiguration configuration, IOptions<CorreoFromOptions> correoFromOptions)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"]!;
            _configuration = configuration;
            _smtpUser =  configuration.GetValue<string>("SMTP_USER") ?? configuration.GetValue<string>("EmailSettings:SMTP_USER")!;
            _smtpPassword = configuration.GetValue<string>("SMTP_PASSWORD") ?? configuration.GetValue<string>("EmailSettings:SMTP_PASSWORD")!;
            _correoFromOptions = correoFromOptions.Value;
            _pathTemplates = configuration["EmailSettings:PathTemplates"]!;
            _correoQA = configuration["EmailSettings:ToEnQA"]!.Split(',').ToList();
            _esQA = configuration["EmailSettings:EnQA"]!.ToLower() == "true";
            _contentRootPath = env.ContentRootPath;
            _logger = logger;
        }

        public void Enviar(
            List<Reemplazar> listaReemplazo,
            string correoAsunto,
            List<Destinatario> listaDestinatarios,
            string nombreTemplate,
            List<Attachment>? attachments = null,
            string CC = "",
            string BCC = "")
        {
            var rutaTemplate = Path.Combine(_contentRootPath, _pathTemplates + "-" + nombreTemplate);
            var archInfo = new FileInfo(rutaTemplate);
            if (!archInfo.Exists) {
                rutaTemplate = Path.Combine(_contentRootPath, _pathTemplates, nombreTemplate);
            }

            try
            {               
                string? fromEmail = _correoFromOptions.Correo["Gomind"];
                Enviar(fromEmail, listaReemplazo, correoAsunto, listaDestinatarios, rutaTemplate, attachments, CC, BCC);
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en envio de correo:{_smtpUser}, {correoAsunto},error:{ex.Message}");
                throw;
            }
        }

        private void Enviar(
            string from,
            List<Reemplazar> listaReemplazo,
            string correoAsunto,
            List<Destinatario> listaDestinatarios,
            string rutaTemplate,
            List<Attachment>? attachments = null,
            string CC = "",
            string BCC = "")
        {
            string To = "";
            try
            {
               
                StreamReader strrTemplate = new StreamReader(rutaTemplate);
                string htmlBody = strrTemplate.ReadToEnd();
                strrTemplate.Close();

                if (listaReemplazo != null)
                {
                    foreach (Reemplazar Item in listaReemplazo)
                    {
                        htmlBody = htmlBody.Replace(Item.TextoBuscar, Item.TextoReemplazar);
                    }
                }

                if (this._esQA)
                {
                    To = String.Join(",", _correoQA);
                    correoAsunto = "QA: " + correoAsunto;
                }
                else
                {
                    To = String.Join(",", listaDestinatarios.Select(c => c.Correo));
                }

                EnviarEmail(_smtpServer, To, from, CC, BCC, correoAsunto, htmlBody, attachments);
            }
            catch
            {
                throw;
            }
        }

        private void EnviarEmail(
            string smtpServer,
            string eTo,
            string eFrom,
            string eCc,
            string eBcc,
            string eSubject,
            string htmlBody,
            List<Attachment>? attachments = null)
        {
            MailMessage message = new MailMessage();
            SmtpClient client = new SmtpClient();
            string[] HostPort = smtpServer.Split(':');

            try
            {
                string[] ListTo = eTo.Split(',');
                for (int i = 0; i < ListTo.Length; i++)
                    message.To.Add(ListTo[i]);

                if (eCc != "")
                {
                    string[] ListCc = eCc.Split(',');
                    for (int i = 0; i < ListCc.Length; i++)
                        message.CC.Add(ListCc[i]);
                }
                if (eBcc != "")
                {
                    string[] ListBcc = eBcc.Split(',');
                    for (int i = 0; i < ListBcc.Length; i++)
                        message.Bcc.Add(ListBcc[i]);
                }
                message.From = new MailAddress(eFrom);
                message.IsBodyHtml = true;
                message.Subject = eSubject;
                message.Body = htmlBody;
                client.Host = HostPort[0];
                if (HostPort.Length > 1)
                {
                    client.Port = Convert.ToInt32(HostPort[1]);
                }

                #region Tiene Credencial
                if (!String.IsNullOrEmpty(_smtpUser)) {
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    var credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPassword);
                    client.EnableSsl = true;
                    client.Credentials = credentials;
                }
                #endregion

                #region Adjuntos
                if (attachments != null)
                {
                    foreach (Attachment attach in attachments)
                    {
                        message.Attachments.Add(attach);
                    }
                }
                #endregion

                client.Send(message);
            }
            catch 
            {
                throw;
            }
        }
    }
}