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
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 1. הגדרת מזהה בית הספר מהטוקן
                parentAvailabilityDto.SchoolId = GetCurrentSchoolId();

                // 2. תרגום תעודת זהות ל-ID פנימי
                // אנחנו שולפים את כל ההורים ומחפשים את זה עם תעודת הזהות התואמת
                var allParents = await _parentService.GetAll();
                var parent = allParents.FirstOrDefault(p => p.ParentIdentity == parentAvailabilityDto.ParentId.ToString());

                if (parent == null)
                {
                    return BadRequest("שגיאה: תעודת זהות זו לא קיימת במערכת. יש להעלות את ההורה באקסל תחילה.");
                }

                // 3. עדכון ה-DTO ב-ID האמיתי (המפתח הזר)
                parentAvailabilityDto.ParentId = parent.Id;

                // 4. שמירה למסד הנתונים
                var result = await _service.AddItem(parentAvailabilityDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // במקרה של שגיאת SQL פנימית (כמו כפילות), נקבל הודעה ברורה
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schoolId = GetCurrentSchoolId();
                var list = await _service.GetBySchoolId(schoolId);
                return Ok(list);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
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