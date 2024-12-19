using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace TestSolution.Server;

public static class StartupExtensions
{

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ApplicationConfiguration>();
        services.AddScoped<TokenGeneratorService>();

        return services;
    }

    public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<ApplicationConfiguration>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.JwtIssuer,
                    ValidAudience = configuration.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddApplicationSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Http Processor Tester",
                Description = "Live API Documentation for Http Processor Tester"
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });

        });

        return services;
    }

}
