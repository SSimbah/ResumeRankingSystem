using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SkillRequirement
    {
        // Primary Key
        [Key]
        [ScaffoldColumn(false)]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SkillRequirementId { get; set; }

        // Foreign Key to JobPosting
        [Required]
        public int JobId { get; set; }

        // Navigation property for JobPosting
        [ForeignKey("JobId")]
        public virtual JobPosting? JobPosting { get; set; }

        // Name of the skill requirement
        [Required]
        [StringLength(255)]
        public string? Name { get; set; }

        // Scoring associated with the skill requirement
        [Range(1, 5)]
        public decimal Scoring { get; set; }
    }

}
