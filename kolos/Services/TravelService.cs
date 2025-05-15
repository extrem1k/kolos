using kolos.DTO;
using kolos.Exceptions;
using kolos.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace kolos.Services;

public class TravelService (IConfiguration config) : IdbService
{
    private readonly string? _connectionString = config.GetConnectionString("DefaultConnection");
    
    //helper
    private async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        if(connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();
        return connection;
    }

      public async Task<StudentGetDTO> CreateStudentAsync(StudentCreateDto studentData)
    {
        await using var connection = await GetConnectionAsync();

        var groups = new List<StudentGroupGetDto>();
        
        if (studentData.GroupAssignments is not null && studentData.GroupAssignments.Count != 0)
        {
            foreach (var group in studentData.GroupAssignments)
            {
                var groupCheckSql = """
                                    select Id, Name 
                                    from "Group" 
                                    where Id = @Id;
                                    """;

                await using var groupCheckCommand = new SqlCommand(groupCheckSql, connection);
                groupCheckCommand.Parameters.AddWithValue("@Id", group);
                await using var groupCheckReader = await groupCheckCommand.ExecuteReaderAsync();

                if (!await groupCheckReader.ReadAsync())
                {
                    throw new NotFoundException($"Group with id {group} does not exist");
                }

                groups.Add(new StudentGroupGetDto
                {
                    Id = groupCheckReader.GetInt32(0),
                    Name = groupCheckReader.GetString(1),
                });
            }
        }

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {

            var createStudentSql = """
                                   insert into student
                                   output inserted.Id
                                   values (@FirstName, @LastName, @Age);
                                   """;

            await using var createStudentCommand =
                new SqlCommand(createStudentSql, connection, (SqlTransaction)transaction);
            createStudentCommand.Parameters.AddWithValue("@FirstName", studentData.FirstName);
            createStudentCommand.Parameters.AddWithValue("@LastName", studentData.LastName);
            createStudentCommand.Parameters.AddWithValue("@Age", studentData.Age);

            var createdStudentId = Convert.ToInt32(await createStudentCommand.ExecuteScalarAsync());

            foreach (var group in groups)
            {
                var groupAssignmentSql = """
                                         insert into groupAssignment 
                                         values (@StudentId, @GroupId);
                                         """;
                await using var groupAssignmentCommand =
                    new SqlCommand(groupAssignmentSql, connection, (SqlTransaction)transaction);
                groupAssignmentCommand.Parameters.AddWithValue("@StudentId", createdStudentId);
                groupAssignmentCommand.Parameters.AddWithValue("@GroupId", group.Id);

                await groupAssignmentCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            return new StudentGetDTO
            {
                Id = createdStudentId,
                FirstName = studentData.FirstName,
                LastName = studentData.LastName,
                Age = studentData.Age,
                Groups = groups
            };

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<StudentGetDTO>> GetStudentDetailsAsync(string? searchName)
    {
        
        var result = new Dictionary<int, StudentGetDTO>();
        await using var connection = await GetConnectionAsync();


      const  string 
         sql = """
               SELECT S.Id, S.FirstName, S.LastName, S.Age, G.Id, G.Name FROM Student S left join 
               GroupAssignment GA on GA.Student_Id= S.Id left join [Group] G on G.Id = GA.Group_Id
               where @searchName like '%' + @searchName + '%' or @searchName is null;
                 
""";
       

        
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchName", searchName is null ? DBNull.Value : searchName);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (!result.ContainsKey(reader.GetInt32(0)))
            {
                var student = new StudentGetDTO()
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Age = reader.GetInt32(3),
                    Groups = new List<StudentGroupGetDto>(),
                };
                result.Add(reader.GetInt32(0), student);
            }

            if ( !reader.IsDBNull(4))
            {
                var StudentGroupDto = new StudentGroupGetDto()
                {
                    Id = reader.GetInt32(4),
                    Name = reader.GetString(5),
                };
                result[reader.GetInt32(0)].Groups.Add(StudentGroupDto);
            }
            
            
            
        }

        return result.Values;
    }
    
    
}