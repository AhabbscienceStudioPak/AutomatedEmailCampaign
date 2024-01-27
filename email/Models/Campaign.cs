namespace email.Models;

public class Campaign
{
    public int CampaignId { get; set; }

    public string CampaignName { get; set; }

    public DateTime ScheduledDate { get; set; }

    public int EmailId { get; set; }

    public bool IsActive { get; set; } //not in db


}
