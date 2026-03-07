using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentAvailabilityController(IService<ParentAvailabilityDto> service) : ControllerBase
    {

        private readonly IService<ParentAvailabilityDto> _service = service;

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentAvailabilityDto parentAvailabilityDto)
        {
            try
            {
                var result = await _service.AddItem(parentAvailabilityDto);
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
                var parentAvailability = await _service.GetAll();
                return Ok(parentAvailability);
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
                var parentAvailability = await _service.GetById(id);
                if (parentAvailability == null)
                {
                    return NotFound();
                }
                return Ok(parentAvailability);
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
        public async Task<IActionResult> Post([FromForm] ParentAvailabilityDto parentAvailability, int id)
        {
            try
            {
                var newParentAvailability = await _service.UpdateItem(id, parentAvailability);
                if (newParentAvailability == null)
                {
                    return NotFound();
                }
                return Ok(newParentAvailability);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
