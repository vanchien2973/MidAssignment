using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category> GetByIdAsync(Guid categoryId);
    Task<IEnumerable<Category>> GetAllAsync(int pageNumber, int pageSize, string sortBy = null, string sortOrder = null, string searchTerm = null);
    Task<bool> NameExistsAsync(string categoryName);
    Task<Category> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Guid categoryId);
    Task<int> CountAsync(string searchTerm = null);
}

