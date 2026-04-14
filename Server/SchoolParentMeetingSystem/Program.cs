using SchoolParentMeetingSystem.Service.Services;
using DataContext;
using Microsoft.EntityFrameworkCore;
using SchoolParentMeetingSystem.Extensions;

var builder = WebApplication.CreateBuilder(args);

//  חיבור למסד נתונים
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SchoolParentMeetingSystemContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddServices(connectionString);

//  רישום אבטחה 
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();