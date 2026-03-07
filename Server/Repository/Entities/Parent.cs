using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Parent
    {

        [Key]
        public int Id { get; set; }

        [Required, StringLength(12, MinimumLength = 7)]
        public string ParentIdentity { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 2)]
        public string ParentName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(50)]
        public string ParentEmail { get; set; } = string.Empty;
        [MaxLength(4)]
        public int SchoolId { get; set; }

        public ICollection<Student> Students { get; set; }

        public ICollection<ParentMeeting> ParentMeetings { get; set; }

        public ICollection<ParentAvailability> ParentAvailability { get; set; }

    }
}
