using kolos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers();
builder.Services.AddScoped<IdbService, TravelService>();
builder.Services.AddEndpointsApiExplorer();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}




app.UseAuthorization();
app.MapControllers();


app.Run();

