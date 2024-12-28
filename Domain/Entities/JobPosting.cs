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
        public required string SkillsRequirement { get; set; }
        public required string EducationRequirement { get; set; }
        public required string ExperienceRequirement { get; set; }
        public DateTime CreatedAt { get; set; }
        public required User User { get; set; }
        public ICollection<Application> Applications { get; set; }
    }
}
