using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface IBookRepository
{
    Task<Book> GetByIdAsync(Guid bookId);
    Task<IEnumerable<Book>> GetAllAsync(int pageNumber, int pageSize, string sortBy = null, string sortOrder = null);
    Task<IEnumerable<Book>> GetByCategoryIdAsync(Guid categoryId, int pageNumber, int pageSize);
    Task<IEnumerable<Book>> GetAvailableBooksAsync(int pageNumber, int pageSize);
    Task<bool> IsbnExistsAsync(string isbn);
    Task<Book> CreateAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(Guid bookId);
    Task<int> CountAsync();
    Task<int> CountByCategoryAsync(Guid categoryId);
    Task<int> CountAvailableBooksAsync();
    Task<bool> HasBooksInCategoryAsync(Guid categoryId);
}