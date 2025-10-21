
using HNG_Task2.Data;
using Microsoft.EntityFrameworkCore;

namespace HNG_Task2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Register SQLite DB
            builder.Services.AddDbContext<AppDbContext>(options =>
              options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            builder.Services.AddHealthChecks();


            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
               //  app.UseHttpsRedirection();
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }


            app.UseAuthorization();


            app.MapControllers();
            app.MapGet("/", () => "🚀 API is running on PXXL App successfully!");

            // Log successful startup
            Console.WriteLine($"✅ Server running on port {port} in {app.Environment.EnvironmentName} mode");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated(); // Creates DB if not existing
            }


            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Application failed to start: " + ex);
                throw;
            }
        }
    }
}
