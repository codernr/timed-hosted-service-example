using Microsoft.EntityFrameworkCore;
using TimedHostedServiceExample.Model.Entities;

namespace TimedHostedServiceExample.Model
{
    public class JobDbContext : DbContext
    {
        public DbSet<JobData> JobDatas { get; set; }

        public JobDbContext(DbContextOptions<JobDbContext> options): base(options) { }
    }
}