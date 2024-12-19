
namespace TestSolution.Server;

public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        services.AddControllers();

        services.AddApplicationServices()
            .AddApplicationAuthorization()
            .AddApplicationSwagger();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

}
