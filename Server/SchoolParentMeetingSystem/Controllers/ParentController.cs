using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repository.Entities;
using Service.Dto;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentController(IService<ParentDto> service) : ControllerBase
    {
        private readonly IService<ParentDto> _service = service;

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentDto parentDto)
        {
            try
            {
                var result = await _service.AddItem(parentDto);
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
                var parents = await _service.GetAll();
                return Ok(parents);
            }
            catch (Exception ex)
            {
                //return BadRequest(ex.Message);
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var parent = await _service.GetById(id);
                if (parent == null)
                {
                    return NotFound();
                }
                return Ok(parent);
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
        public async Task<IActionResult> Post([FromForm] ParentDto parent, int id)
        {
            try
            {
                var newParent = await _service.UpdateItem(id, parent);
                if (newParent == null)
                {
                    return NotFound();
                }
                return Ok(newParent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
