using Application.Interfaces.Services.EmailServices;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;

namespace Infrastructure.Services.SmtpEmailService
{
    public class EmailServices(IOptions<SmtpSettings> smtpSettings) : IEmailServices
    {
        #region Configuracion de variables locales
        private readonly string _smtpServer = smtpSettings.Value.Server;
        private readonly int _smtpPort = smtpSettings.Value.Port;
        private readonly string _smtpEmail = smtpSettings.Value.SenderEmail;
        private readonly string _smtpPassword = smtpSettings.Value.Password;
        #endregion

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            #region Configurar el correo
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Bookme", _smtpEmail));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            #region Asignar el cuerpo html del correo a enviar
            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();
            #endregion
            #endregion

            #region Enviar el correo por servicio smtp
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            Console.WriteLine($"INTENTANDO AUTENTICAR CON: Usuario: '{_smtpEmail}', Password: '{_smtpPassword}'");

            await client.AuthenticateAsync(_smtpEmail, _smtpPassword); // autenticacion para enviar el correo
            await client.SendAsync(emailMessage); // envia el mail
            await client.DisconnectAsync(true); // desconecta el servicio
            #endregion
            
        }

        public async Task SendVerificationEmailAsync(string toEmail, string verificationCode, int expirationTime)
        {
            #region Ruta que contiene el HTML a renderizar en el cuerpo del correo
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Directory.GetParent(baseDirectory).Parent!.Parent!.Parent!.Parent!.FullName;
            string bodyFilePath = Path.Combine(projectRoot, "Infrastructure", "Services", "SmtpEmailService", "EmailTemplates", "VerificationCode.html");

            if (!File.Exists(bodyFilePath))
            {
                throw new FileNotFoundException($"Buscando en: {bodyFilePath}");
            }

            #endregion

            #region Asignar valores del codigo y tiempo de expiracion al html
            string body = await File.ReadAllTextAsync(bodyFilePath);
            body = body.Replace("{{CODE}}", verificationCode);
            body = body.Replace("{{EXPIRATION_TIME}}", expirationTime.ToString());
            #endregion

            //Enviar email
            await SendEmailAsync(toEmail, "Verify your account with BookMe", body);
        }
    }
}
