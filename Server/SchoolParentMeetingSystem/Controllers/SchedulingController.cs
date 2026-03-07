using Microsoft.AspNetCore.Mvc;
using Service.Scheduling;
using System.Threading.Tasks;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;

        public SchedulingController(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
        }

        [HttpPost("generate/{schoolId}")]
        public async Task<IActionResult> Generate(int schoolId)
        {
            try
            {
                var result = await _schedulingService.ProcessSchedulingAsync(schoolId);
                return Ok(new
                {
                    Message = "השיבוץ הסתיים בהצלחה",
                    TotalMeetings = result.Count,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה ביצירת שיבוץ: {ex.Message}");
            }
        }
    }
}