using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;
using System.Security.Claims;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParentAvailabilityController : ControllerBase
    {
        private readonly IService<ParentAvailabilityDto> _service;
        private readonly IService<ParentDto> _parentService; // שימוש בממשק הגנרי שלך

        // Constructor שמרכז את ההזרקות
        public ParentAvailabilityController(
            IService<ParentAvailabilityDto> service,
            IService<ParentDto> parentService)
        {
            _service = service;
            _parentService = parentService;
        }

        private int GetCurrentSchoolId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [Authorize(Roles = "Admin,School")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentAvailabilityDto parentAvailabilityDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // 1. קבלת מזהה בית הספר מהטוקן
                int currentSchoolId = GetCurrentSchoolId();
                if (currentSchoolId == 0) return Unauthorized("לא נמצא מזהה בית ספר בטוקן.");

                parentAvailabilityDto.SchoolId = currentSchoolId;

                // 2. ויתור על חיפוש ההורה/יצירתו
                // אנחנו פשוט שומרים את ה-ParentId כפי שהגיע מה-Frontend (שהוא הת"ז)
                // שים לב: זה יעבוד רק אם השדה ParentId במסד הנתונים הוא מסוג שיכול להכיל ת"ז 
                // ואם אין עליו Constraint של מפתח זר שמחייב קיום בטבלת Parents.

                // 3. שמירת האילוץ ישירות
                var result = await _service.AddItem(parentAvailabilityDto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // אם מופיעה כאן שגיאת SQL על Foreign Key, סימן שצריך לבטל את הקישור במסד הנתונים
                return BadRequest("שגיאה בשמירה: וודא שהמערכת מאפשרת הזנת תעודת זהות ללא שיוך להורה קיים. " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schoolId = GetCurrentSchoolId(); 
                if (schoolId <= 0)
                {
                    return Unauthorized("מזהה בית ספר לא נמצא");
                }

                var list = await _service.GetBySchoolId(schoolId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _service.GetById(id);
                if (item == null) return NotFound();
                if (item.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin")) return Forbid();

                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // כאן אפשר להוסיף את שאר המתודות (GetById, Update) באותה צורה
    }
}