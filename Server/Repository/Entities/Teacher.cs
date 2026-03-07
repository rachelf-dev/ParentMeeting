using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(5, MinimumLength = 1)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [ForeignKey("School")]
        public int SchoolId { get; set; }
        public School School { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<ParentMeeting> ParentMeetings { get; set; } = new List<ParentMeeting>();

    }
}
