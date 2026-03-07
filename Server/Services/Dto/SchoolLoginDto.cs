using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace Service.Dto
{
    public class SchoolLoginDto
    {
        [Required(ErrorMessage = "שם בית הספר הוא שדה חובה")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "סיסמה היא שדה חובה")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-zA-Z]).{6,20}$",
         ErrorMessage = "הסיסמה חייבת להכיל אותיות ומספרים ובאורך 6-20 תווים")]
        public string Password { get; set; } = string.Empty;

        public string? Token { get; set; }
    }
}