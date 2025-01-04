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

        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }

        [ScaffoldColumn(false)]
        public string? FirstName { get; set; }

        [ScaffoldColumn(false)]
        public string? MiddleName { get; set; }

        [ScaffoldColumn(false)]
        public string? LastName { get; set; }

        [ScaffoldColumn(false)]
        public string? Objective { get; set; }

        [ScaffoldColumn(false)]
        public string? Email { get; set; }

        [ScaffoldColumn(false)]
        public string? PhoneNumber { get; set; }

        [ScaffoldColumn(false)]
        public string? Address { get; set; }

        [ScaffoldColumn(false)]
        public string? Gender { get; set; }

        [ScaffoldColumn(false)]
        public int Age { get; set; }

        [ScaffoldColumn(false)]
        public string? Experience { get; set; }

        [ScaffoldColumn(false)]
        public string? Education { get; set; }

        [ScaffoldColumn(false)]
        public string? Skills { get; set; }
        public ICollection<Application>? Applications { get; set; }
    }
}
