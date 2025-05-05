using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystem.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.CreatedDate)
            .IsRequired();
            
        builder.Property(u => u.UserType)
            .IsRequired();
            
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        // Relationships
        builder.HasMany(u => u.BorrowingRequests)
            .WithOne(r => r.Requestor)
            .HasForeignKey(r => r.RequestorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.ApprovedRequests)
            .WithOne(r => r.Approver)
            .HasForeignKey(r => r.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.ActivityLogs)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indices
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}