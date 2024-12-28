using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProofDocument
    {
        [Key]
        [ScaffoldColumn(false)]
        public int DocumentId { get; set; }
        public int ApplicantId { get; set; }
        public required string DocumentType { get; set; }
        public required string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public Applicant Applicant { get; set; }
    }
}
