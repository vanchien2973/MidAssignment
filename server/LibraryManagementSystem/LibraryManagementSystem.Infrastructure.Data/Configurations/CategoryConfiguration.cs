using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystem.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.CategoryId);
        
        builder.Property(c => c.CategoryId)
            .ValueGeneratedOnAdd();
        
        builder.Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(c => c.Description)
            .HasMaxLength(500);
            
        builder.Property(c => c.CreatedDate)
            .IsRequired();
            
        // Relationships
        builder.HasMany(c => c.Books)
            .WithOne(b => b.Category)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indices
        builder.HasIndex(c => c.CategoryName).IsUnique();
    }
}