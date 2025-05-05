using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _context;
    
    public UserRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<User> GetByIdAsync(int userId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<IEnumerable<User>> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = null)
    {
        var query = _context.Users.AsQueryable().AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => 
                u.Username.ToLower().Contains(searchTerm) || 
                u.Email.ToLower().Contains(searchTerm) ||
                u.FullName.ToLower().Contains(searchTerm));
        }
        
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<User>> GetUsersByIdsAsync(List<int> userIds)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .ToListAsync();
    }
    
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
    
    public async Task<bool> EmailExistsAsync(string email, int exceptUserId)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.UserId != exceptUserId);
    }
    
    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }
    
    public async Task UpdateAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await Task.CompletedTask;
    }
    
    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
        
        await Task.CompletedTask;
    }
    
    public async Task<int> CountBySearchTermAsync(string searchTerm = null)
    {
        var query = _context.Users.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => 
                u.Username.ToLower().Contains(searchTerm) || 
                u.Email.ToLower().Contains(searchTerm) ||
                u.FullName.ToLower().Contains(searchTerm));
        }
        
        return await query.CountAsync();
    }
}