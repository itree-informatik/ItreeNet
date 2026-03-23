using Azure;
using Azure.Communication.Email;
using ItreeNet.Data.Models;
using ItreeNet.Interfaces;
using ILogger = Serilog.ILogger;

namespace ItreeNet.Services
{
    public class MailService : IMailService
    {
        private readonly string _connectionString;
        private const string Sender = "DoNotReply@itree.ch";
        private readonly ILogger _logger;
        private readonly bool _isProduction;
        private readonly IMitarbeiterService _mitarbeiterService;
        private readonly IProjektService _projektService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly List<EmailAddress> _projectRecipients = new();

        public MailService(IConfiguration configuration, ILogger logger, IWebHostEnvironment environment, IMitarbeiterService mitarbeiterService, IProjektService projektService, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("Mail")!;
            _isProduction = environment.IsProduction();
            _mitarbeiterService = mitarbeiterService;
            _projektService = projektService;
            _backgroundTaskQueue = backgroundTaskQueue;

            var projectRecipients = configuration.GetSection("ProjectRecipients").Get<List<string>>();
            if (projectRecipients == null)
            {
                throw new InvalidDataException("No ProjectRecipients in configuration found!");
            }

            projectRecipients.ForEach(x =>
            {
                _projectRecipients.Add(new EmailAddress(x, ExtractNameFromEmail(x)));
            });
        }

        public async Task SendBookingNotificationAsync(Projekt project, decimal prozent)
        {
            var sendMail = false;

            // zwischen 80 und 89
            if (prozent >= 80 && prozent <= 89)
            {
                sendMail = !project.EmailGesendet80;

                project.EmailGesendet80 = true;
                project.EmailGesendet90 = false;
                project.EmailGesendet100 = false;
            }
            // zwischen 90 und 99
            else if (prozent >= 90 && prozent <= 99)
            {
                sendMail = !project.EmailGesendet90;

                project.EmailGesendet80 = true;
                project.EmailGesendet90 = true;
                project.EmailGesendet100 = false;
            }
            // über 100
            else if (prozent >= 100)
            {
                sendMail = !project.EmailGesendet100;

                project.EmailGesendet80 = true;
                project.EmailGesendet90 = true;
                project.EmailGesendet100 = true;
            }

            await _projektService.SaveSingleAsync(project);

            if (sendMail)
            {
                var emailSubject = $"ItreeNet: Projektbudget {project.Bezeichnung} auf {prozent}%";
                var emailContent = new EmailContent(emailSubject)
                {
                    Html = CreateBookingMessage(project, prozent)
                };

                var recipients = new EmailRecipients(_projectRecipients);
                var email = new EmailMessage(Sender, recipients, emailContent);
                
                _backgroundTaskQueue.EnqueueTask(async (_, _) =>
                {
                    try
                    {
                        if (_isProduction)
                        {
                            var emailClient = new EmailClient(_connectionString);
                            // ReSharper disable once MethodSupportsCancellation
                            await emailClient.SendAsync(WaitUntil.Completed, email);
                        }
                        else
                        {
                            Console.WriteLine("Email sended");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "SendBookingNotificationAsync failed");
                        throw;
                    }
                });
            }
        }

        private string CreateBookingMessage(Projekt model, decimal prozent)
        {
            var url = _isProduction
                ? $"https://www.itree.ch/intern/kunden/{model.KundeId}/{model.Id}"
                : $"https://localhost:5001/intern/kunden/{model.KundeId}/{model.Id}";

            var template = $"<p>Hi,</p>";
            if (prozent > 80 && prozent < 90)
            {
                template += $"<p>Das Projektbudget des Projekts <a href=\"{url}\">Ticket ({model.Bezeichnung})</a> liegt nun bei {prozent}%.</p>";
            }
            else if(prozent > 90 && prozent < 100)
            {
                template += $"<p>Das Projektbudget des Projekts <a href=\"{url}\">Ticket ({model.Bezeichnung})</a> ist mit {prozent}% nahezu aufgebraucht.</p>";
            }
            else
            {
                template += $"<p>Das Projektbudget des Projekts <a href=\"{url}\">Ticket ({model.Bezeichnung})</a> ist mit {prozent}% aufgebraucht.</p>";
            }

            template += "</br>";
            return template;
        }

        private static string ExtractNameFromEmail(string email)
        {
            // Trenne den E-Mail-Namen anhand des Punkts und konvertiere jeden Teil zu einem Titel-Case-Format
            string[] parts = email.Split('@')[0].Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }

            // Verbinde die Teile zu einem vollständigen Namen
            string fullName = string.Join(" ", parts);

            return fullName;
        }
    }
}
