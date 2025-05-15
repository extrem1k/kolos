using kolos.DTO;
using kolos.Models;

namespace kolos.Services;

public interface IdbService
{
    public Task<IEnumerable<StudentGetDTO>> GetStudentDetailsAsync(string? searchName);
    public Task<StudentGetDTO> CreateStudentAsync(StudentCreateDto studentCreateDto);
}