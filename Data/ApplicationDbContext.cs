using BookAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data
{
	public class ApplicationDbContext : IdentityDbContext<User>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}
		public DbSet<Book> Books { get; set; }
	}
}
