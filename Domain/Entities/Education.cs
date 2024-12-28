using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Education
    {
        [Key]
        [ScaffoldColumn(false)]
        public int EducationId { get; set; }
        public int ApplicantId { get; set; }
        public required string School { get; set; }
        public required string Degree { get; set; }
        public decimal GPA { get; set; }
        public required string StartDate { get; set; }
        public required string EndDate { get; set; }
        public required Applicant Applicant { get; set; }
    }
}
