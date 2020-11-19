using Microsoft.EntityFrameworkCore;

namespace advent_of_qode_server
{
    public class AdventContext : DbContext
    {
        public DbSet<ScoreBoard> ScoreBoard { get; set; }

        public AdventContext(DbContextOptions<AdventContext> options) : base(options)
        {

        }
    }
}