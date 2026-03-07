using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class StudentDto
    {
        [Required]
        [StringLength(12, MinimumLength = 7)]
        public string StudentIdentity { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(4, MinimumLength = 1)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public int ParentId { get; set; }
        [Required]
        public int SchoolId { get; set; }
    }
}
