using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController(IService<TeacherDto> service) : ControllerBase
    {
        private readonly IService<TeacherDto> _service = service;

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] TeacherDto teacherDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _service.AddItem(teacherDto);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var teachers = await _service.GetAll();
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var teacher = await _service.GetById(id);
                if (teacher == null)
                {
                    return NotFound();
                }
                return Ok(teacher);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Post([FromForm] TeacherDto teacher, int id)
        {
            try
            {
                var newTeacher = await _service.UpdateItem(id, teacher);
                if (newTeacher == null)
                {
                    return NotFound();
                }
                return Ok(newTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
