using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class ParentMeeting
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey(nameof(Parent))]
        [Required]
        public int ParentId { get; set; }
        public Parent Parent { get; set; }

        [Required]
        public string ClassName { get; set; } = string.Empty;
        [Required]
        public int SchoolId { get; set; }

        [Required]
        public DateTime MeetingDate { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsPast { get; set; } = false;
    }
}
