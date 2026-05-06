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
        public int? ParentId { get; set; }
        public string ParentIdentity { get; set; }

        
        public DateTime MeetingDate { get; set; }
        public int SchoolId { get; set; }


        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
