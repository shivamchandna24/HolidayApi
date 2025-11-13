using HolidayApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace HolidayApi.Infrastructure
{
    public class HolidayContext : DbContext
    {
        public HolidayContext(DbContextOptions<HolidayContext> options) : base(options) { }
        public DbSet<Holiday> Holidays { get; set; }

    }
}

