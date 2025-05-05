using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementSystem.Infrastructure.Data.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.HasKey(l => l.LogId);
        
        builder.Property(l => l.ActivityType)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(l => l.ActivityDate)
            .IsRequired();
            
        builder.Property(l => l.Details)
            .HasMaxLength(500);
            
        builder.Property(l => l.IpAddress)
            .HasMaxLength(50);
            
        // Indices
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.ActivityDate);
        builder.HasIndex(l => l.ActivityType);
    }
}