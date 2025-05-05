using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystem.Infrastructure.Data.Configurations;

public class BookBorrowingResquestDetailConfiguration : IEntityTypeConfiguration<BookBorrowingRequestDetail>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequestDetail> builder)
    {
        builder.HasKey(d => d.DetailId);
        
        builder.Property(d => d.DetailId)
           .ValueGeneratedOnAdd();
        
        builder.Property(d => d.Status)
            .IsRequired();
            
        // Indices
        builder.HasIndex(d => d.RequestId);
        builder.HasIndex(d => d.BookId);
        builder.HasIndex(d => new { d.RequestId, d.BookId }).IsUnique();
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.DueDate);
    }
}