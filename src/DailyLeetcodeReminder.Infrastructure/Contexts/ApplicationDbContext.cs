using DailyLeetcodeReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DailyLeetcodeReminder.Infrastructure.Contexts;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<ChallengerWithNoAttempt>(e =>
        {
            e.HasNoKey();
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.DefaultTypeMapping<ChallengerWithNoAttempt>();
    }
}