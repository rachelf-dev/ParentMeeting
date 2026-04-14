using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParentMeetingController(IService<ParentMeetingDto> service) : ControllerBase
    {
        private readonly IService<ParentMeetingDto> _service = service;

        private int GetCurrentSchoolId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // ADD
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentMeetingDto parentMeetingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                parentMeetingDto.SchoolId = GetCurrentSchoolId();

                var result = await _service.AddItem(parentMeetingDto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET ALL
        [Authorize(Roles = "Admin,School")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schoolId = GetCurrentSchoolId();
                var meetings = await _service.GetBySchoolId(schoolId);

                return Ok(meetings);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //  GET BY ID
        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var meeting = await _service.GetById(id);

                if (meeting == null)
                    return NotFound();

                if (meeting.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                return Ok(meeting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var meeting = await _service.GetById(id);

                if (meeting == null)
                    return NotFound();

                if (meeting.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                await _service.DeleteItem(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // UPDATE
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ParentMeetingDto parentMeeting, int id)
        {
            try
            {
                var existing = await _service.GetById(id);

                if (existing == null)
                    return NotFound();

                if (existing.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                parentMeeting.SchoolId = existing.SchoolId;

                var result = await _service.UpdateItem(id, parentMeeting);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}