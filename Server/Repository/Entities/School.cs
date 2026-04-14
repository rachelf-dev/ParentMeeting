using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class School
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "School";

        [Required]
        public DateTime MeetingDate { get; set; }

        [Required]
        public TimeSpan MeetingStartTime { get; set; }

        [Required]
        public TimeSpan MeetingEndTime { get; set; }

        [Required]
        public int SlotDurationMinutes { get; set; }

        public ICollection<Student> Students { get; set; }
        public ICollection<Parent> Parents { get; set; } 
        public ICollection<ParentMeeting> parentMeetings { get; set; }

    }
}
