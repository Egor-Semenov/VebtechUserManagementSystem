using Application.Mappers;
using Application.Middleware;
using Microsoft.OpenApi.Models;
using VebtechTask.Api.Extentions;

namespace VebtechTask.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper(typeof(UserMapper));
            builder.Services.ConfigureSqlConnection(builder.Configuration);
            builder.Services.ConfigureJwt(builder.Configuration);
            builder.Services.ConfigureRepositories();
            builder.Services.ConfigureServices();
            builder.Services.ConfigureValidators();
            builder.Services.ConfigureSwagger();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x =>
            {
                x.EnableAnnotations();
                x.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management", Version = "v1" });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management V1");
                });
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}