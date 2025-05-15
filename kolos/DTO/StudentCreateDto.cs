using System.ComponentModel.DataAnnotations;

namespace kolos.DTO;

public class StudentCreateDto
{
    [MaxLength(50)]
    public required string FirstName { get; set; }
    
    [MaxLength(50)]
    public required string LastName { get; set; }
    [Range(0,100)]
    public required int Age { get; set; }
    
    public  List<int>? GroupAssignments { get; set; }
}