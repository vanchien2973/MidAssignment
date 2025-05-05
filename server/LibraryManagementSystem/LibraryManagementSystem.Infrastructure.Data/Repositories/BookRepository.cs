using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
namespace LibraryManagementSystem.Infrastructure.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;
    
    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<Book> GetByIdAsync(Guid bookId)
    {
        return await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.BookId == bookId);
    }
    
    public async Task<IEnumerable<Book>> GetAllAsync(int pageNumber, int pageSize, string sortBy = null, string sortOrder = null)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "title":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.Title)
                        : query.OrderBy(b => b.Title);
                    break;
                case "author":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.Author)
                        : query.OrderBy(b => b.Author);
                    break;
                case "category":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.Category.CategoryName)
                        : query.OrderBy(b => b.Category.CategoryName);
                    break;
                case "year":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.PublishedYear)
                        : query.OrderBy(b => b.PublishedYear);
                    break;
                case "available":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.AvailableCopies)
                        : query.OrderBy(b => b.AvailableCopies);
                    break;
                default:
                    query = query.OrderBy(b => b.BookId);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(b => b.BookId);
        }
        
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Book>> GetByCategoryIdAsync(Guid categoryId, int pageNumber, int pageSize)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Where(b => b.CategoryId == categoryId)
            .OrderBy(b => b.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Book>> GetAvailableBooksAsync(int pageNumber, int pageSize)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Where(b => b.AvailableCopies > 0 && b.IsActive)
            .OrderBy(b => b.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<bool> IsbnExistsAsync(string isbn)
    {
        return await _context.Books
            .AnyAsync(b => b.ISBN == isbn);
    }
    
    public async Task<Book> CreateAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        return book;
    }
    
    public Task UpdateAsync(Book book)
    {
        _context.Entry(book).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    
    public async Task DeleteAsync(Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book != null)
        {
            _context.Books.Remove(book);
        }
    }
    
    public async Task<int> CountAsync()
    {
        return await _context.Books.CountAsync();
    }
    
    public async Task<int> CountByCategoryAsync(Guid categoryId)
    {
        return await _context.Books
            .CountAsync(b => b.CategoryId == categoryId);
    }

    public async Task<int> CountAvailableBooksAsync()
    {
        return await _context.Books
            .CountAsync(b => b.AvailableCopies > 0 && b.IsActive);
    }

    public async Task<bool> HasBooksInCategoryAsync(Guid categoryId)
    {
        return await _context.Books
            .AnyAsync(b => b.CategoryId == categoryId);
    }
}