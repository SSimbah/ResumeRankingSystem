using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Domain.Entities
{
    public class Applicant
    {
        [Key]
        [ScaffoldColumn(false)]
        public int ApplicantId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string MiddleName { get; set; }
        public required string LastName { get; set; }
        public int Age { get; set; }
        public required string Gender { get; set; }
        public required string Address { get; set; }
        public int PhoneNumber { get; set; }
        public required string Skills { get; set; }
        public ICollection<Education> Educations { get; set; }
        public ICollection<Experience> Experiences { get; set; }
        public ICollection<Application> Applications { get; set; }
    }
}
