using IPInfoAPI.Data;
using Microsoft.EntityFrameworkCore;

public class IPInfoDbContext : DbContext
{
    public IPInfoDbContext(DbContextOptions<IPInfoDbContext> options) : base(options) { }

    public DbSet<IPDetailsEntity> IPDetails { get; set; }
}
