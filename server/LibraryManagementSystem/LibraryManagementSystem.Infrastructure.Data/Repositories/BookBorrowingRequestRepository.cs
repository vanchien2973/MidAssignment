using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories;

public class BookBorrowingRequestRepository : IBookBorrowingRequestRepository
{
    private readonly LibraryDbContext _context;
    
    public BookBorrowingRequestRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<BookBorrowingRequest> GetByIdAsync(int requestId)
    {
        return await GetByIdAsync(new Guid($"00000000-0000-0000-0000-{requestId:D12}"));
    }
    
    public async Task<BookBorrowingRequest> GetByIdAsync(Guid requestId)
    {
        return await _context.BookBorrowingRequests
            .Include(r => r.Requestor)
            .Include(r => r.Approver)
            .Include(r => r.RequestDetails)
                .ThenInclude(d => d.Book)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);
    }
    
    public async Task<IEnumerable<BookBorrowingRequest>> GetByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
        return await _context.BookBorrowingRequests
            .Include(r => r.Requestor)
            .Include(r => r.Approver)
            .Include(r => r.RequestDetails)
                .ThenInclude(d => d.Book)
            .Where(r => r.RequestorId == userId)
            .OrderByDescending(r => r.RequestDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<BookBorrowingRequest>> GetPendingRequestsAsync(int pageNumber, int pageSize)
    {
        return await _context.BookBorrowingRequests
            .Include(r => r.Requestor)
            .Include(r => r.RequestDetails)
                .ThenInclude(d => d.Book)
            .Where(r => r.Status == BorrowingRequestStatus.Waiting)
            .OrderBy(r => r.RequestDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<BookBorrowingRequest>> GetByStatusAsync(BorrowingRequestStatus status, int pageNumber, int pageSize)
    {
        return await _context.BookBorrowingRequests
            .Include(r => r.Requestor)
            .Include(r => r.Approver)
            .Include(r => r.RequestDetails)
                .ThenInclude(d => d.Book)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.RequestDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<BookBorrowingRequest>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _context.BookBorrowingRequests
            .Include(r => r.Requestor)
            .Include(r => r.Approver)
            .Include(r => r.RequestDetails)
                .ThenInclude(d => d.Book)
            .OrderByDescending(r => r.RequestDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<BookBorrowingRequest> CreateAsync(BookBorrowingRequest request)
    {
        await _context.BookBorrowingRequests.AddAsync(request);
        return request;
    }
    
    public Task UpdateAsync(BookBorrowingRequest request)
    {
        _context.Entry(request).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    
    public async Task<int> GetRequestCountByUserInMonthAsync(int userId, DateTime monthStart, DateTime monthEnd)
    {
        return await _context.BookBorrowingRequests
            .CountAsync(r => r.RequestorId == userId 
                      && r.RequestDate >= monthStart 
                      && r.RequestDate <= monthEnd);
    }
    
    public async Task<int> CountAsync()
    {
        return await _context.BookBorrowingRequests.CountAsync();
    }
    
    public async Task<bool> HasUserActiveBookLoansAsync(int userId)
    {
        return await _context.BookBorrowingRequests
            .Include(r => r.RequestDetails)
            .AnyAsync(r => r.RequestorId == userId 
                     && r.Status == BorrowingRequestStatus.Approved
                     && r.RequestDetails.Any(d => d.ReturnDate == null || d.ReturnDate > DateTime.UtcNow));
    }
}