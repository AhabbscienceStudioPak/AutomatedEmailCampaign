namespace email.Models;

public class Email
{
    public int EmailId { get; set; }
    public string Frequency { get; set; }
    public string Subject { get; set; } 
    public string CC { get; set; } 
    public int RecipientId { get; set; }
    public string Content { get; set; }

}