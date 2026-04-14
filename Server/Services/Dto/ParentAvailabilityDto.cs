using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class ParentAvailabilityDto
    {
        [Required]
        public int ParentId { get; set; }
        
        public DateTime MeetingDate { get; set; }
        public int SchoolId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;
    }
}
