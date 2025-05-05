using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories;

public class BookBorrowingRequestDetailRepository : IBookBorrowingRequestDetailRepository
{
    private readonly LibraryDbContext _context;
    
    public BookBorrowingRequestDetailRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<BookBorrowingRequestDetail> GetByIdAsync(Guid detailId)
    {
        return await _context.BookBorrowingRequestDetails
            .Include(d => d.Book)
            .Include(d => d.Request)
            .FirstOrDefaultAsync(d => d.DetailId == detailId);
    }
    
    public async Task<BookBorrowingRequestDetail> CreateAsync(BookBorrowingRequestDetail detail)
    {
        await _context.BookBorrowingRequestDetails.AddAsync(detail);
        return detail;
    }
    
    public Task UpdateAsync(BookBorrowingRequestDetail detail)
    {
        _context.Entry(detail).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    
    public async Task<IEnumerable<BookBorrowingRequestDetail>> GetOverdueItemsAsync(int pageNumber, int pageSize)
    {
        var today = DateTime.Today;
        
        return await _context.BookBorrowingRequestDetails
            .Include(d => d.Book)
            .Include(d => d.Request)
                .ThenInclude(r => r.Requestor)
            .Where(d => d.Status == BorrowingDetailStatus.Borrowing && d.DueDate < today)
            .OrderBy(d => d.DueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<bool> HasActiveBorrowingsForBookAsync(Guid bookId)
    {
        return await _context.BookBorrowingRequestDetails
            .AnyAsync(d => d.BookId == bookId && d.Status == BorrowingDetailStatus.Borrowing);
    }
}