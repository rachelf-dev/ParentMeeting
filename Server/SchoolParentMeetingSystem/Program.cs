using SchoolParentMeetingSystem.Service.Services;
using DataContext;
using Microsoft.EntityFrameworkCore;
using SchoolParentMeetingSystem.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// 2. חיבור למסד נתונים
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SchoolParentMeetingSystemContext>(options =>
    options.UseSqlServer(connectionString));

// 3. רישום סרוויסים ואבטחה (Identity)
builder.Services.AddServices(connectionString);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // הוספת Swagger

var app = builder.Build();

// --- סדר ה-Middleware קריטי כאן ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();  
app.UseAuthorization();  

app.MapControllers();

app.Run();