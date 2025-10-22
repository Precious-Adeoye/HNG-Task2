using HNG_Task2.IStringServices;
using HNG_Task2.StringServices;
using Microsoft.AspNetCore.Mvc;

namespace HNG_Task2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register storage service
            builder.Services.AddSingleton<IStringStorage, StringStorageService>();

            // Configure port for Railway
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            // Add services
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Configure pipeline
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.MapGet("/", () => "🚀 API is running on Railway successfully!");

            // Log startup
            Console.WriteLine($"✅ Server running on port {port} in {app.Environment.EnvironmentName} mode");

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Application failed to start: {ex}");
                throw;
            }
        }
    }
}