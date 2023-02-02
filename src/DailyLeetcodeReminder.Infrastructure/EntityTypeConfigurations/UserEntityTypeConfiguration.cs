using DailyLeetcodeReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyLeetcodeReminder.Infrastructure.EntityTypeConfigurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<Challenger>
{
    public void Configure(EntityTypeBuilder<Challenger> builder)
    {
        builder.ToTable("Challengers");

        builder.HasKey(u => u.TelegramId);

        builder.Property(u => u.TelegramId)
            .ValueGeneratedNever();

        builder.Property(u => u.LeetcodeUserName)
            .HasMaxLength(100)
            .IsRequired(true);

        builder
            .HasIndex(u => u.LeetcodeUserName)
            .IsUnique();
        
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired(false);
    }
}