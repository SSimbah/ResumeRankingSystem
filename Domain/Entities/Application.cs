using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Application
    {
        [Key]
        [ScaffoldColumn(false)]
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public int ApplicantId { get; set; }
        public required string Status { get; set; } = "Pending";
        public decimal SkillsScoring { get; set; }
        public decimal EducationScoring { get; set; }
        public decimal ExperienceScoring { get; set; }
        public decimal Score { get; set; }
        //public required JobPosting JobPosting { get; set; }
        //public required Applicant Applicant { get; set; }
        // Navigation properties
        [ForeignKey("JobId")] // Explicitly link to JobId
        public virtual JobPosting? JobPosting { get; set; }

        [ForeignKey("ApplicantId")] // Explicitly link to ApplicantId
        public virtual Applicant? Applicant { get; set; }
    }
}
