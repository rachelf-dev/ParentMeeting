using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentMeetingController(IService<ParentMeetingDto> service) : ControllerBase
    {
        private readonly IService<ParentMeetingDto> _service = service;

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentMeetingDto parentMeetingDto)
        {
            try
            {
                var result = await _service.AddItem(parentMeetingDto);
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
                var parentsMeeting = await _service.GetAll();
                return Ok(parentsMeeting);
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
                var parentMeeting = await _service.GetById(id);
                if (parentMeeting == null)
                {
                    return NotFound();
                }
                return Ok(parentMeeting);
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
        public async Task<IActionResult> Post([FromForm] ParentMeetingDto parentMeeting, int id)
        {
            try
            {
                var newParentMeeting = await _service.UpdateItem(id, parentMeeting);
                if (newParentMeeting == null)
                {
                    return NotFound();
                }
                return Ok(newParentMeeting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
