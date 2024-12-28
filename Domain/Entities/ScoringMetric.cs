using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ScoringMetric
    {
        [Key]
        [ScaffoldColumn(false)]
        public int MetricId { get; set; }
        public decimal SkillsScoring { get; set; }
        public decimal EducationScoring { get; set; }
        public decimal ExperienceScoring { get; set; }
        public ICollection<Application> Applications { get; set; }
    }
}
