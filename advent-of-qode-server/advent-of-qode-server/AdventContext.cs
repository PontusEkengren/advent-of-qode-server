using Microsoft.EntityFrameworkCore;

namespace advent_of_qode_server
{
    public class AdventContext : DbContext
    {
        public DbSet<ScoreBoard> ScoreBoard { get; set; }
        public DbSet<Question> Questions { get; set; }

        public AdventContext(DbContextOptions<AdventContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>().HasAlternateKey(q => new {q.Year, q.Day});
            modelBuilder.Entity<Question>().HasKey(q => q.Id);
        }
    }
}