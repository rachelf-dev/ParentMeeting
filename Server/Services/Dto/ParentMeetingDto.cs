using Repository;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class ParentMeetingDto
    {


        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ParentId { get; set; }

        [Required, StringLength(5, MinimumLength = 1)]
        public string ClassName { get; set; }
        [MaxLength(4)]
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
