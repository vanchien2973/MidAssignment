using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly LibraryDbContext _context;
    
    public CategoryRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<Category> GetByIdAsync(Guid categoryId)
    {
        return await _context.Categories.FindAsync(categoryId);
    }
    
    public async Task<IEnumerable<Category>> GetAllAsync(int pageNumber, int pageSize, string sortBy = null, string sortOrder = null, string searchTerm = null)
    {
        var query = _context.Categories.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(c => 
                c.CategoryName.ToLower().Contains(searchTerm) || 
                (c.Description != null && c.Description.ToLower().Contains(searchTerm))
            );
        }
        
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.CategoryName)
                        : query.OrderBy(c => c.CategoryName);
                    break;
                case "created":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.CreatedDate)
                        : query.OrderBy(c => c.CreatedDate);
                    break;
                default:
                    query = query.OrderBy(c => c.CategoryId);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(c => c.CategoryId);
        }
        
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<bool> NameExistsAsync(string categoryName)
    {
        return await _context.Categories
            .AnyAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());
    }
    
    public async Task<Category> CreateAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        return category;
    }
    
    public Task UpdateAsync(Category category)
    {
        _context.Entry(category).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    
    public async Task DeleteAsync(Guid categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category != null)
        {
            _context.Categories.Remove(category);
        }
    }
    
    public async Task<int> CountAsync(string searchTerm = null)
    {
        var query = _context.Categories.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(c => 
                c.CategoryName.ToLower().Contains(searchTerm) || 
                (c.Description != null && c.Description.ToLower().Contains(searchTerm))
            );
        }
        
        return await query.CountAsync();
    }
}