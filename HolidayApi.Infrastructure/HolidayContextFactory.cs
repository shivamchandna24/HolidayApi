using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HolidayApi.Infrastructure
{
    public class HolidayContextFactory : IDesignTimeDbContextFactory<HolidayContext>
    {
        public HolidayContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HolidayContext>();
            optionsBuilder.UseSqlServer("Your_Connection_String_Here"); // replace with actual connection string

            return new HolidayContext(optionsBuilder.Options);
        }
    }
}
