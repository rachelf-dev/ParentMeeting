using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParentAvailabilityController(IService<ParentAvailabilityDto> service) : ControllerBase
    {
        private readonly IService<ParentAvailabilityDto> _service = service;

        private int GetCurrentSchoolId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // ADD
        [Authorize(Roles = "Admin,School")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentAvailabilityDto parentAvailabilityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                parentAvailabilityDto.SchoolId = GetCurrentSchoolId();

                var result = await _service.AddItem(parentAvailabilityDto);

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
                var list = await _service.GetBySchoolId(schoolId);

                return Ok(list);
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
                var item = await _service.GetById(id);

                if (item == null)
                    return NotFound();

                if (item.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE
        [Authorize(Roles = "Admin,School")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _service.GetById(id);

                if (item == null)
                    return NotFound();

                if (item.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
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
        public async Task<IActionResult> Update([FromBody] ParentAvailabilityDto parentAvailability, int id)
        {
            try
            {
                var existing = await _service.GetById(id);

                if (existing == null)
                    return NotFound();

                if (existing.SchoolId != GetCurrentSchoolId() && !User.IsInRole("Admin"))
                    return Forbid();

                parentAvailability.SchoolId = existing.SchoolId;

                var result = await _service.UpdateItem(id, parentAvailability);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}