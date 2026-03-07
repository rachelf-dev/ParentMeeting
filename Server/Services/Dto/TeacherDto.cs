using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class TeacherDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(5, MinimumLength = 1)]
        public string ClassName { get; set; }

        [Required]
        public int SchoolId { get; set; }


    }
}
