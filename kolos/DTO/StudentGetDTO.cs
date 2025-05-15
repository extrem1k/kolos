namespace kolos.DTO;

public class StudentGetDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public List<StudentGroupGetDto> Groups { get; set; }
}