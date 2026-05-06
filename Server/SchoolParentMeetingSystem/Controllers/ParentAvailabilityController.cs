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
        public async Task<IActionResult> AddItem([FromBody] ParentAvailabilityDto dto)
        {
            try
            {
                int currentSchoolId = GetCurrentSchoolId();
                dto.SchoolId = currentSchoolId;

                // חיפוש זריז: אם ההורה קיים, נשמור את ה-ID שלו ליתר ביטחון
                var allParents = await _parentService.GetBySchoolId(currentSchoolId);
                var parent = allParents.FirstOrDefault(p => p.ParentIdentity == dto.ParentIdentity);

                if (parent != null)
                {
                    dto.ParentId = parent.Id;
                }
                else
                {
                    dto.ParentId = null; // אין הורה? לא נורא, יש לנו את ה-ParentIdentity
                }

                var result = await _service.AddItem(dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
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