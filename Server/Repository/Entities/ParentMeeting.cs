using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [ForeignKey("TeacherId")]
        public int TeacherId { get; set; }
        
        public virtual Teacher Teacher { get; set; }

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