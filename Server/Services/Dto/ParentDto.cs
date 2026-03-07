using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class ParentDto
    {
        [Required]
        [StringLength(12,MinimumLength = 7)]
        public string ParentIdentity { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string ParentName { get; set; }

        [EmailAddress]
        public string ParentEmail { get; set; }
    }
}
