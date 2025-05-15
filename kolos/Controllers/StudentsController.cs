using kolos.DTO;
using kolos.Exceptions;
using kolos.Models;
using kolos.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolos.Controllers;


[ApiController]
[Route("[controller]")]
public class StudentsController(IdbService dbService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAllStudents([FromQuery] string? searchName)
    {
       return Ok(await dbService.GetStudentDetailsAsync(searchName));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudentAsync([FromBody] StudentCreateDto studentCreateDto)
    {
        try
        {
            var student = await dbService.CreateStudentAsync(studentCreateDto);
            return Created($"students/{student.Id}", student);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    
    }
    
    
}