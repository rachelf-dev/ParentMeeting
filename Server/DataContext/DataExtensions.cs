using DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interfaces;
//using Service.Services;

namespace SchoolParentMeetingSystem.DataContext
{
    public static class DataExtensions
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SchoolParentMeetingSystemContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IContext, SchoolParentMeetingSystemContext>();

            //services.AddScoped<ExcelImportService>();

            return services;
        }
    }
}
