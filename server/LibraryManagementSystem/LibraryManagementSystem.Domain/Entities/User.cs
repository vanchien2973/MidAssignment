using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public UserType UserType { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    // Navigation properties
    public ICollection<BookBorrowingRequest> BorrowingRequests { get; set; }
    public ICollection<BookBorrowingRequest> ApprovedRequests { get; set; }
    public ICollection<UserActivityLog> ActivityLogs { get; set; }
}