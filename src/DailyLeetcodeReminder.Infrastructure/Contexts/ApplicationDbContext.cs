using Microsoft.EntityFrameworkCore;

namespace DailyLeetcodeReminder.Infrastructure.Contexts;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}
}