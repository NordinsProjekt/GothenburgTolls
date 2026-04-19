using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EFCore;

public class DesignTimeFactory : IDesignTimeDbContextFactory<TollDbContext>
{
    public TollDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TollDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GothenburgTolls;Trusted_Connection=True;")
            .Options;

        return new TollDbContext(options);
    }
}
