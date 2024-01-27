using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using email.Models;
using email.Context; // Make sure to include the correct namespace for your DbContext

namespace email.Controllers
{
    public class EmailCampaignController : Controller
    {
        private readonly MyDbContext _dbContext;

        public EmailCampaignController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /EmailCampaign/ComposeEmail
        [HttpGet]
        public IActionResult ComposeEmail()
        {
            var recipients = _dbContext.Recipients.Select(r => r.RecipientName).ToList(); //_dbContext.Recipients.ToList();

            ViewBag.Recipients = recipients;

            return View(new Email()); // Pass an instance of the Email model
        }

        [HttpGet]
        public IActionResult GetRecipientData(string recipient)
        {
            // Logic to fetch recipient email and recipient ID based on the recipient name
            var recipientData = _dbContext.Recipients
                    .FirstOrDefault(r => r.RecipientName == recipient);

            return Json(new { email = recipientData.ContactEmail, recipientId = recipientData.RecipientId });
        }


        [HttpPost]
        public IActionResult ComposeEmail(Email email)
        {
            if (ModelState.IsValid)
            {
                Email newEmail = new Email{
                    Frequency = email.Frequency,
                    Subject = email.Subject,
                    CC = email.CC,
                    RecipientId = email.RecipientId,
                    Content = email.Content
                };
                _dbContext.Emails.Add(newEmail);
                _dbContext.SaveChanges();

                return Ok(newEmail);
            }

            var recipients = _dbContext.Recipients.Select(r => r.RecipientName).ToList();
            ViewBag.Recipients = recipients;

            return View(email);
        }


        [HttpGet]
        public IActionResult SendEmail()
        {
            var emails = _dbContext.Emails.ToList();

            var emailViewModels = new List<EmailViewModel>();

            foreach (var email in emails)
            {
                var recipient = _dbContext.Recipients.FirstOrDefault(r => r.RecipientId == email.RecipientId);
                var emailViewModel = new EmailViewModel
                {
                    Email = email,
                    RecipientName = recipient?.RecipientName // Use null conditional operator in case recipient is not found
                };
                emailViewModels.Add(emailViewModel);
            }

            return View(emailViewModels);
        }

        [HttpPost]
        public IActionResult SendEmail(List<int> selectedEmailIds, DateTime sendDate)
        {
            foreach (var emailId in selectedEmailIds)
            { 
                Console.WriteLine("loop working");
                // Retrieve the email based on the ID and update the scheduled date and campaign name
                var email = _dbContext.Emails.FirstOrDefault(e => e.EmailId == emailId);
                var recipient = _dbContext.Recipients.FirstOrDefault(r => r.RecipientId == email.RecipientId);
                if (email != null ) //&& _dbContext.Campaigns.FirstOrDefault(c => c.EmailId == email.EmailId) == null
                {
                    Campaign newCampaign = new Campaign{
                        ScheduledDate = sendDate.ToUniversalTime(),
                        CampaignName = email.Subject,
                        EmailId = email.EmailId,
                        IsActive = true
                    };
                    _dbContext.Campaigns.Add(newCampaign);
                    _dbContext.SaveChanges();
                    
                    // ScheduleAndSendEmail(email, sendDate.ToUniversalTime(), email.Frequency, recipient.ContactEmail);
                    // return Ok(newCampaign);
                }
                // Schedule and send email
                // ScheduleAndSendEmail(email, sendDate.ToUniversalTime(), email.Frequency, recipient.ContactEmail);
            }

            // You can perform other actions here, like sending emails

            return RedirectToAction("SendEmail"); // Redirect back to the list after processing
        }

        private void ScheduleAndSendEmail(Email email, DateTime ScheduledDate, string frequency, string recipientEmailAddress)
        {
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


            // Get the current date and time in UTC
            DateTime currentUtcDateTime = DateTime.UtcNow;

            // Calculate the delay before sending
            TimeSpan delay = ScheduledDate - currentUtcDateTime;

            if (delay.TotalMilliseconds > 0)
            {
                // Use a timer to delay sending the email
                System.Timers.Timer timer = new System.Timers.Timer(delay.TotalMilliseconds);
                timer.Elapsed += (sender, args) =>
                {
                    // Send the email using the provided SMTP logic
                    SendEmailSMTP(email, recipientEmailAddress);
                };
                timer.AutoReset = false; // Only fire once
                timer.Start();
            } 
            else
            {
                // The scheduled date is not in the future, so send immediately
                SendEmailSMTP(email, recipientEmailAddress);
            }
        }


        private void SendEmailSMTP(Email email, string RecipientEmailAddress)
        {
            // Implement your email sending logic here
            // You can use libraries like SmtpClient or third-party services for sending emails
            // Example using SmtpClient:
            using (var smtpClient = new SmtpClient())
            {
                // Configure SMTP settings
                smtpClient.Host = "smtp.mailgun.org";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("postmaster@sandbox87f9cc9adb244fecaf3c118dbb2d56de.mailgun.org", "558757328ec926ccd0407f600e601a26-ee16bf1a-5ddf768b");

                // Create and send the email
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress("postmaster@sandbox87f9cc9adb244fecaf3c118dbb2d56de.mailgun.org	");
                    mailMessage.To.Add(RecipientEmailAddress);
                    mailMessage.Subject = email.Subject;
                    mailMessage.Body = email.Content;
                    mailMessage.IsBodyHtml = true;

                    smtpClient.Send(mailMessage);
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteSelectedEmails(List<int> selectedEmailIds)
        {
            foreach (var emailId in selectedEmailIds)
            {
                var emailToRemove = _dbContext.Emails.FirstOrDefault(e => e.EmailId == emailId);
                if (emailToRemove != null)
                {
                    _dbContext.Emails.Remove(emailToRemove);
                    _dbContext.SaveChanges();
                }
            }

            return RedirectToAction("SendEmail");
        }

        [HttpGet]
        public IActionResult ActivateEmail(int page = 1)
        {
            int pageSize = 8; // Number of records per page
            
            var totalRecords = _dbContext.Campaigns.Count();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var campaigns = _dbContext.Campaigns
                .OrderByDescending(c => c.ScheduledDate) // Order campaigns by scheduled date or any other property you want
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PageNumber = page;
            ViewBag.TotalPages = totalPages;

            return View(campaigns);
        }

        [HttpPost]
        public IActionResult DeactivateCampaigns(List<int> selectedCampaignIds, int pageNumber)
        {
            var campaignsToUpdate = _dbContext.Campaigns
                .Where(c => selectedCampaignIds.Contains(c.CampaignId))
                .ToList();

            foreach (var campaign in campaignsToUpdate)
            {
                campaign.IsActive = false;
            }

            _dbContext.SaveChanges();

            return RedirectToAction("ActivateEmail", new { page = pageNumber });
        }

        [HttpPost]
        public IActionResult ActivateCampaigns(List<int> selectedCampaignIds, int pageNumber)
        {
            var campaignsToUpdate = _dbContext.Campaigns
                .Where(c => selectedCampaignIds.Contains(c.CampaignId))
                .ToList();

            foreach (var campaign in campaignsToUpdate)
            {
                campaign.IsActive = true;
            }

            _dbContext.SaveChanges();

            return RedirectToAction("ActivateEmail", new { page = pageNumber });
        }

        [HttpPost]
        public IActionResult AddRecipient(Recipient recipient)
        {
            // Server-side validation logic
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database
                var existingRecipient = _dbContext.Recipients.FirstOrDefault(r => r.ContactEmail == recipient.ContactEmail);
                var existingRecipientName = _dbContext.Recipients.FirstOrDefault(r => r.RecipientName == recipient.RecipientName);
                if (existingRecipient != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View("AddRecipient", recipient);
                }
                if (existingRecipientName != null)
                {
                    ModelState.AddModelError("Name", "Name already exists - use a unique name.");
                    return View("AddRecipient", recipient);
                }

                // Other custom validation checks as needed...

                // If all server-side validations pass, save the user to the database
                _dbContext.Recipients.Add(recipient);
                _dbContext.SaveChanges();

                return Ok(recipient);
            }

            // If the ModelState is not valid, return the view with validation errors
            return BadRequest(ModelState);
        }

        public IActionResult AddRecipient()
        {
            var recipient = new Recipient(); // You can initialize the model here
            return View(recipient);
        }

    }
}

