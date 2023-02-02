using DailyLeetcodeReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyLeetcodeReminder.Infrastructure.EntityTypeConfigurations;

public class DailyAttemptTypeConfiguration : IEntityTypeConfiguration<DailyAttempt>
{
    public void Configure(EntityTypeBuilder<DailyAttempt> builder)
    {
        builder.ToTable("DailyAttempts");

        builder.HasKey(da => new { da.Date, da.UserId });
        
        builder
            .HasOne(da => da.User)
            .WithMany(u => u.DailyAttempts)
            .HasForeignKey(da => da.UserId);
    }
}