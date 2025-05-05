using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LibraryManagementSystem.Infrastructure.Data.Context;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<BookBorrowingRequest> BookBorrowingRequests { get; set; }
    public DbSet<BookBorrowingRequestDetail> BookBorrowingRequestDetails { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}