using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Domain.Entities
{
    public class JobPosting
    {
        [Key]
        [ScaffoldColumn(false)]
        public int JobId { get; set; }
        public int UserId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        //public decimal SkillsScoring { get; set; } // to be deleted
        //public decimal EducationScoring { get; set; } // to be deleted
        //public decimal ExperienceScoring { get; set; } // to be deleted
        public User? User { get; set; }
        public ICollection<Application>? Applications { get; set; }
        // Collection of EducationRequirements
        public virtual ICollection<EducationRequirement>? EducationRequirements { get; set; }

        // Collection of ExperienceRequirements
        public virtual ICollection<ExperienceRequirement>? ExperienceRequirements { get; set; }

        // Collection of SkillRequirements
        public virtual ICollection<SkillRequirement>? SkillRequirements { get; set; }
    }
}
