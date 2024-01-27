using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using email.Models;
using email.Context;
using System.Net.Mail;
using System.Net;

public class BackgroundEmailSender : BackgroundService
{
    private readonly ILogger<BackgroundEmailSender> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BackgroundEmailSender(
        ILogger<BackgroundEmailSender> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundEmailSender is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay for 1 minute

            _logger.LogInformation("BackgroundEmailSender is processing emails.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                var runningCampaigns = dbContext.Campaigns
                    .Where(c => c.ScheduledDate >= DateTime.UtcNow || c.ScheduledDate >= DateTime.UtcNow.AddMinutes(-1) && c.IsActive == true)
                    .ToList();

                await Task.WhenAll(runningCampaigns.Select(campaign => SendEmailAsync(dbContext, campaign)));

                await dbContext.SaveChangesAsync();
            }
        }

        _logger.LogInformation("BackgroundEmailSender is stopping.");
    }

    private async Task SendEmailAsync(MyDbContext dbContext, Campaign campaign)
    {
        _logger.LogInformation("BackgroundEmailSender: SendEmailAsync");
        var email = dbContext.Emails.FirstOrDefault(e => e.EmailId == campaign.EmailId);
        var recipient = dbContext.Recipients.FirstOrDefault(r => r.RecipientId == email.RecipientId);
        // Use the ScheduleAndSendEmail and SendEmailSMTP methods here
        ScheduleAndSendEmail(dbContext, email, campaign.ScheduledDate, email.Frequency, recipient.ContactEmail, campaign);
        await dbContext.SaveChangesAsync();
    }

        private void ScheduleAndSendEmail(MyDbContext dbContext, Email email, DateTime ScheduledDate, string frequency, string recipientEmailAddress, Campaign campaign)
        {
            _logger.LogInformation("BackgroundEmailSender: ScheduleAndSendEmail");

            TimeSpan timeUntilScheduled = ScheduledDate - DateTime.UtcNow;
            if (TimeSpan.Compare(timeUntilScheduled, TimeSpan.Zero) < 0)
            {
                // The previous scheduled date is not in the future, so send immediately
                SendEmailSMTP(email, recipientEmailAddress);

                DateTime _scheduledDate;
                // Determine the scheduled date based on the frequency
                switch (frequency)
                {
                    case "One-time":
                        _scheduledDate = ScheduledDate; // Use the provided scheduled date
                        break;
                    case "Daily":
                        _scheduledDate = DateTime.UtcNow.AddDays(1).Date.Add(ScheduledDate.TimeOfDay);
                        break;
                    case "Weekly":
                        _scheduledDate = DateTime.UtcNow.AddDays(7).Date.Add(ScheduledDate.TimeOfDay);
                        break;
                    case "Monthly":
                        _scheduledDate = DateTime.UtcNow.AddMonths(1).Date.Add(ScheduledDate.TimeOfDay);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(frequency), "Invalid frequency type.");
                }
                _logger.LogInformation("Updated schedule date: " + _scheduledDate);

                    campaign.ScheduledDate = _scheduledDate;
                    dbContext.SaveChanges(); // Update scheduled date
                }
        }


        private void SendEmailSMTP(Email email, string RecipientEmailAddress)
        {
            _logger.LogInformation("BackgroundEmailSender: SendEmailSMTP");
            // Implement your email sending logic here
            // You can use libraries like SmtpClient or third-party services for sending emails
            // Example using SmtpClient:
            using (var smtpClient = new SmtpClient())
            {
                // Configure SMTP settings
                smtpClient.Host = "smtp.mailgun.org";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("<mailgun-domain-address>", "<api-key>");

                // Create and send the email
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress("<mailgun-domain-address>");
                    mailMessage.To.Add(RecipientEmailAddress);
                    mailMessage.Subject = email.Subject;
                    mailMessage.Body = email.Content;
                    mailMessage.IsBodyHtml = true;

                    smtpClient.Send(mailMessage);
                }
            }
        }
}
