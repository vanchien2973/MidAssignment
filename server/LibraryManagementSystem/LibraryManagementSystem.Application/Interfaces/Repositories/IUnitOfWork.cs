namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    IBookRepository Books { get; }
    IUserRepository Users { get; }
    IBookBorrowingRequestRepository BookBorrowingRequests { get; }
    IBookBorrowingRequestDetailRepository BookBorrowingRequestDetails { get; }
    IUserActivityLogRepository UserActivityLogs { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}