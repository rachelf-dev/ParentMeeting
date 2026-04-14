using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController(IService<TeacherDto> service) : ControllerBase
    {
        private readonly IService<TeacherDto> _service = service;

        private int GetCurrentSchoolId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        //  ADD
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] TeacherDto teacherDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                
                teacherDto.SchoolId = GetCurrentSchoolId();

                var result = await _service.AddItem(teacherDto);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //  GET ALL 
        [Authorize(Roles = "Admin,School")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schoolId = GetCurrentSchoolId();
                var teachers = await _service.GetBySchoolId(schoolId);

                return Ok(teachers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET BY ID 
        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var teacher = await _service.GetById(id);

                if (teacher == null)
                    return NotFound();

                if (teacher.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                return Ok(teacher);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //  DELETE
        [Authorize(Roles = "School")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var teacher = await _service.GetById(id);

                if (teacher == null)
                    return NotFound();

                if (teacher.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
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
        [Authorize(Roles = "Admin,School")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] TeacherDto teacher, int id)
        {
            try
            {
                var existing = await _service.GetById(id);

                if (existing == null)
                    return NotFound();

                if (existing.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                teacher.SchoolId = existing.SchoolId;

                var newTeacher = await _service.UpdateItem(id, teacher);

                return Ok(newTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}