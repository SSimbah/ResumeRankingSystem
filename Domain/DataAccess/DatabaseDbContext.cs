using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Domain.DataAccess
{
    public class DatabaseDbContext : DbContext
    {
        //public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options)
        //    : base(options)
        //{
        //}

        private readonly IConfiguration _configuration;

        public DatabaseDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DatabaseDbContext"));
        }


        //public DatabaseDbContext() : base() // Default constructor
        //{
        //}


        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<ScoringMetric> ScoringMetrics { get; set; }
        public DbSet<ProofDocument> ProofDocuments { get; set; }

    }
}
