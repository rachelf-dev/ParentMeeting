using SchoolParentMeetingSystem.Service.Services;
using DataContext;
using Service;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Services;

namespace SchoolParentMeetingSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddServices(connectionString);
            builder.Services.AddDbContext<SchoolParentMeetingSystemContext>(options =>
                options.UseSqlServer(connectionString));
            
            // Add services to the container.
            //builder.Services.AddScoped<IService, ParentService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
