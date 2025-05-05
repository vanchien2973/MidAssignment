using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystem.Infrastructure.Data.Configurations;

public class BookBorrowingResquestConfiguration : IEntityTypeConfiguration<BookBorrowingRequest>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequest> builder)
    {
        builder.HasKey(r => r.RequestId);
        
        builder.Property(r => r.RequestId)
            .ValueGeneratedOnAdd();
        
        builder.Property(r => r.RequestDate)
            .IsRequired();
            
        builder.Property(r => r.Status)
            .IsRequired();
            
        builder.Property(r => r.Notes)
            .HasMaxLength(500);
            
        // Relationships
        builder.HasMany(r => r.RequestDetails)
            .WithOne(d => d.Request)
            .HasForeignKey(d => d.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indices
        builder.HasIndex(r => r.RequestorId);
        builder.HasIndex(r => r.Status);
    }
}