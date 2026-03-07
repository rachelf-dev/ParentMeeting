using DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Service.Dto;
using Service.Importing;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolController(IRegister<SchoolRegisterDto> registerService, ILogin<SchoolLoginDto> loginSevice, IService<SchoolDto> service, ExcelImportService excelImportService, SchoolParentMeetingSystemContext context, IToken<School> tokenService) : ControllerBase
    {
        private readonly IRegister<SchoolRegisterDto> _registerService = registerService;
        private readonly ILogin<SchoolLoginDto> _loginService = loginSevice;
        private readonly IService<SchoolDto> _service = service;
        private readonly ExcelImportService _excelImportService = excelImportService ;
        private readonly SchoolParentMeetingSystemContext _context = context;
        private readonly IToken<School> _tokenService = tokenService;


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SchoolRegisterDto school)
        {
            try
            {
                // בדיקה אם שם בית הספר כבר קיים
                var existsName = await _context.Schools
                    .AnyAsync(s => s.Name == school.Name);

                if (existsName)
                    return BadRequest("A school with this name already exists.");


                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(school.Password);
                var existsPass = await _context.Schools
                    .AnyAsync(s => s.Password == hashedPassword);

                var result = await _registerService.Register(school);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //לבדוק האם הסיסמא שונה
        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] SchoolRegisterDto school)
        //{
        //    try
        //    {
        //        // בדיקה אם שם בית הספר כבר קיים
        //        var existsName = await _context.Schools
        //            .AnyAsync(s => s.Name == school.Name);

        //        if (existsName)
        //            return BadRequest("A school with this name already exists.");

        //        // רישום בית הספר
        //        var result = await _registerService.Register(school);

        //        // יצירת טוקן
        //        var token = _tokenService.GenerateToken(school);

        //        return Ok(new { School = result, Token = token });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] SchoolLoginDto loginDto)
        {
            try
            {
                var result = await _loginService.Login(loginDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAllSchools")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schools = await _service.GetAll();
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("getSchool")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var school = await _service.GetById(id);

                if (school == null)
                    return NotFound();

                return Ok(school);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteSchool/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, SchoolDto school)
        {
            try
            {
                var updatedSchool = await _service.UpdateItem(id, school);

                if (updatedSchool == null)
                    return NotFound();

                return Ok(updatedSchool);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(int schoolId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = file.OpenReadStream();
            await _excelImportService.ImportFromExcel(stream, schoolId);

            return Ok("File imported successfully");
        }

        //        [HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] SchoolRegisterDto school)
        //{
        //    try
        //    {
        //        // בדיקה אם שם כבר קיים
        //        var existsName = await _context.Schools
        //            .AnyAsync(s => s.Name == school.Name);

        //        if (existsName)
        //            return BadRequest("A school with this name already exists.");

        //        // יצירת hash לסיסמה
        //        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(school.Password);

        //        // יצירת Entity חדש
        //        var newSchool = new School
        //        {
        //            Name = school.Name,
        //            Password = hashedPassword
        //        };

        //        _context.Schools.Add(newSchool);
        //        await _context.SaveChangesAsync();

        //        return Ok(newSchool);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

    }
}
