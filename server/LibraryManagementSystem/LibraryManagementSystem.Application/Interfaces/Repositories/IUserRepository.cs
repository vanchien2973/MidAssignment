using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> GetByIdAsync(int userId);
    Task<User> GetByUsernameAsync(string username);
    Task<User> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = null);
    Task<IEnumerable<User>> GetUsersByIdsAsync(List<int> userIds);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> EmailExistsAsync(string email, int exceptUserId);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int userId);
    Task<int> CountBySearchTermAsync(string searchTerm = null);
}