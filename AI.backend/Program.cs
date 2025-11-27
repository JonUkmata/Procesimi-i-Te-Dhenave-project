using Microsoft.EntityFrameworkCore;
using AI.backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient(); // Add this line for HttpClient

// Configure EF Core DbContext
var connectionString = builder.Configuration.GetConnectionString("procesimiAI");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Use CORS - THIS MUST BE BEFORE OTHER MIDDLEWARE
app.UseCors("AllowReact");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();