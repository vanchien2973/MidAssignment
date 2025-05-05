using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Infrastructure.Data.Seeding;

public class DataSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IServiceProvider serviceProvider, ILogger<DataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed data
            await SeedUsersAsync(context);
            await SeedCategoriesAsync(context);
            await SeedBooksAsync(context);
            await SeedBorrowingRequestsAsync(context);

            await context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }

    private async Task SeedUsersAsync(LibraryDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        _logger.LogInformation("Seeding users data...");

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Password = HashPassword("Admin@123"),
                Email = "admin@library.com",
                FullName = "System Administrator",
                IsActive = true,
                UserType = UserType.SuperUser,
                CreatedDate = DateTime.Now
            },
            new User
            {
                Username = "librarian",
                Password = HashPassword("Librarian@123"),
                Email = "librarian@library.com",
                FullName = "Library Staff",
                IsActive = true,
                UserType = UserType.SuperUser,
                CreatedDate = DateTime.Now
            },
            new User
            {
                Username = "user1",
                Password = HashPassword("User@123"),
                Email = "user1@example.com",
                FullName = "Normal User",
                IsActive = true,
                UserType = UserType.NormalUser,
                CreatedDate = DateTime.Now
            }
        };

        await context.Users.AddRangeAsync(users);
    }

    private async Task SeedCategoriesAsync(LibraryDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        _logger.LogInformation("Seeding categories data...");

        var categories = new List<Category>
        {
            new Category { CategoryId = Guid.NewGuid(), CategoryName = "Fiction", Description = "Literature in the form of prose", CreatedDate = DateTime.Now },
            new Category { CategoryId = Guid.NewGuid(), CategoryName = "Non-Fiction", Description = "Based on facts and real events", CreatedDate = DateTime.Now },
            new Category { CategoryId = Guid.NewGuid(), CategoryName = "Science", Description = "Scientific books and journals", CreatedDate = DateTime.Now },
            new Category { CategoryId = Guid.NewGuid(), CategoryName = "Technology", Description = "Books about technology and computing", CreatedDate = DateTime.Now },
            new Category { CategoryId = Guid.NewGuid(), CategoryName = "History", Description = "Books about historical events", CreatedDate = DateTime.Now },
            new Category { CategoryId = Guid.NewGuid(), CategoryName = "Biography", Description = "Books about the life of a person", CreatedDate = DateTime.Now }
        };

        await context.Categories.AddRangeAsync(categories);
    }

    private async Task SeedBooksAsync(LibraryDbContext context)
    {
        if (await context.Books.AnyAsync())
            return;

        _logger.LogInformation("Seeding books data...");

        // Wait for categories to be saved
        await context.SaveChangesAsync();

        var categories = await context.Categories.ToListAsync();
        var fiction = categories.First(c => c.CategoryName == "Fiction");
        var nonFiction = categories.First(c => c.CategoryName == "Non-Fiction");
        var science = categories.First(c => c.CategoryName == "Science");
        var technology = categories.First(c => c.CategoryName == "Technology");
        var history = categories.First(c => c.CategoryName == "History");

        var books = new List<Book>
        {
            new Book
            {
                BookId = Guid.NewGuid(),
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                CategoryId = fiction.CategoryId,
                ISBN = "9780061120084",
                PublishedYear = 1960,
                Publisher = "HarperCollins",
                Description = "A novel about racial inequality and moral growth in the American South.",
                TotalCopies = 10,
                AvailableCopies = 10,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new Book
            {
                BookId = Guid.NewGuid(),
                Title = "1984",
                Author = "George Orwell",
                CategoryId = fiction.CategoryId,
                ISBN = "9780451524935",
                PublishedYear = 1949,
                Publisher = "Secker & Warburg",
                Description = "A dystopian social science fiction novel that portrays a totalitarian regime.",
                TotalCopies = 8,
                AvailableCopies = 8,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new Book
            {
                BookId = Guid.NewGuid(),
                Title = "A Brief History of Time",
                Author = "Stephen Hawking",
                CategoryId = science.CategoryId,
                ISBN = "9780553380163",
                PublishedYear = 1988,
                Publisher = "Bantam Books",
                Description = "A book about cosmology for non-specialists.",
                TotalCopies = 5,
                AvailableCopies = 5,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                CategoryId = technology.CategoryId,
                ISBN = "9780132350884",
                PublishedYear = 2008,
                Publisher = "Prentice Hall",
                Description = "A handbook of agile software craftsmanship.",
                TotalCopies = 7,
                AvailableCopies = 7,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Sapiens: A Brief History of Humankind",
                Author = "Yuval Noah Harari",
                CategoryId = history.CategoryId,
                ISBN = "9780062316097",
                PublishedYear = 2014,
                Publisher = "Harper",
                Description = "A survey of the history of humankind.",
                TotalCopies = 6,
                AvailableCopies = 6,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new Book
            {
                BookId = Guid.NewGuid(),
                Title = "Thinking, Fast and Slow",
                Author = "Daniel Kahneman",
                CategoryId = nonFiction.CategoryId,
                ISBN = "9780374533557",
                PublishedYear = 2011,
                Publisher = "Farrar, Straus and Giroux",
                Description = "A book about cognitive biases and heuristics.",
                TotalCopies = 5,
                AvailableCopies = 5,
                CreatedDate = DateTime.Now,
                IsActive = true
            }
        };

        await context.Books.AddRangeAsync(books);
    }

    private async Task SeedBorrowingRequestsAsync(LibraryDbContext context)
    {
        if (await context.BookBorrowingRequests.AnyAsync())
            return;

        _logger.LogInformation("Seeding borrowing requests data...");

        // Wait for users and books to be saved
        await context.SaveChangesAsync();

        var normalUser = await context.Users.FirstAsync(u => u.Username == "user1");
        var admin = await context.Users.FirstAsync(u => u.Username == "admin");
        var books = await context.Books.Take(3).ToListAsync();

        var request = new BookBorrowingRequest
        {
            RequestId = Guid.NewGuid(),
            RequestorId = normalUser.UserId,
            RequestDate = DateTime.Now.AddDays(-5),
            Status = BorrowingRequestStatus.Approved,
            ApproverId = admin.UserId,
            ApprovalDate = DateTime.Now.AddDays(-4),
            Notes = "Sample approved request",
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    BookId = books[0].BookId,
                    Status = BorrowingDetailStatus.Borrowing,
                    DueDate = DateTime.Now.AddDays(10)
                },
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    BookId = books[1].BookId,
                    Status = BorrowingDetailStatus.Borrowing,
                    DueDate = DateTime.Now.AddDays(10)
                }
            }
        };

        var pendingRequest = new BookBorrowingRequest
        {
            RequestId = Guid.NewGuid(),
            RequestorId = normalUser.UserId,
            RequestDate = DateTime.Now.AddDays(-1),
            Status = BorrowingRequestStatus.Waiting,
            Notes = "Sample pending request",
            RequestDetails = new List<BookBorrowingRequestDetail>
            {
                new BookBorrowingRequestDetail
                {
                    DetailId = Guid.NewGuid(),
                    BookId = books[2].BookId,
                    Status = BorrowingDetailStatus.Borrowing,
                    DueDate = null
                }
            }
        };

        await context.BookBorrowingRequests.AddRangeAsync(request, pendingRequest);

        // Update available copies for borrowed books
        books[0].AvailableCopies -= 1;
        books[1].AvailableCopies -= 1;
        context.Books.UpdateRange(books[0], books[1]);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        return hash;
    }
}