using Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(12,MinimumLength = 8)]
        public string StudentIdentity { get; set; } = string.Empty;

        [StringLength(50,MinimumLength =2)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(5, MinimumLength = 1)]
        public string ClassName { get; set; } =string.Empty;  

        [ForeignKey(nameof(Parent))]
        [Required]
        public int ParentId { get; set; }
        public Parent Parent { get; set; }

        [Required]
        [ForeignKey("School")]
        public int SchoolId { get; set; }
        public School  School { get; set; }

        public int? TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }

    }
}
