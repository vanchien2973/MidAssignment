using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystem.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.BookId);

        builder.Property(b => b.BookId)
            .ValueGeneratedOnAdd();
        
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(b => b.ISBN)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(b => b.Publisher)
            .HasMaxLength(100);
            
        builder.Property(b => b.Description)
            .HasMaxLength(1000);
            
        builder.Property(b => b.TotalCopies)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(b => b.AvailableCopies)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(b => b.CreatedDate)
            .IsRequired();
            
        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        // Relationships
        builder.HasMany(b => b.BorrowingDetails)
            .WithOne(bd => bd.Book)
            .HasForeignKey(bd => bd.BookId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indices
        builder.HasIndex(b => b.ISBN).IsUnique();
        builder.HasIndex(b => b.Title);
        builder.HasIndex(b => b.Author);
    }
}