using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.DataAccess
{
    public class DatabaseDbContext : DbContext
    {
        public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options)
            : base(options)
        {
        }

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
