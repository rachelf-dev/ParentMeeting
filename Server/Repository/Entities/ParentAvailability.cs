using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class ParentAvailability
    {
        [Key]
        public int Id { get; set; }

        
        [Required]
        public string ParentIdentity { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public DateTime MeetingDate { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
    }
}
