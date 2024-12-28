using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        [ScaffoldColumn(false)]
        public int UserId { get; set; } // Primary Key

        [DisplayName("Username")]
        [Required(ErrorMessage = "Please Enter Username")]
        public required string Username { get; set; }

        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public required string Email { get; set; }
        public ICollection<JobPosting>? JobPostings { get; set; }
    }
}
