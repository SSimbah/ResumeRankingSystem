using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public required string Status { get; set; }
        public int Rank { get; set; }
        public required JobPosting JobPosting { get; set; }
        public required Applicant Applicant { get; set; }
    }
}
