using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Experience
    {
        [Key]
        [ScaffoldColumn(false)]
        public int ExperienceId { get; set; }
        public int ApplicantId { get; set; }
        public required string Company { get; set; }
        public required string JobTitle { get; set; }
        public required string Description { get; set; }
        public required string StartDate { get; set; }
        public required string EndDate { get; set; }
        public required Applicant Applicant { get; set; }
    }
}
