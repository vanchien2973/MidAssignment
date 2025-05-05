namespace LibraryManagementSystem.Domain.Entities;

public class UserActivityLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string ActivityType { get; set; }
    public DateTime ActivityDate { get; set; }
    public string Details { get; set; }
    public string IpAddress { get; set; }
    
    // Navigation properties
    public User User { get; set; }
}