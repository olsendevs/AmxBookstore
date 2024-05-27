using AmxBookstore.DependencyInjection;
using AmxBookstore.Infrastructure.Identity;
using AmxBookstore.Infrastructure.Logging;
using AmxBookstoreAPI.Middleware;
using AspNetCoreRateLimit;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

LoggingConfiguration.ConfigureLogging(builder.Configuration);

builder.Host.UseSerilog();

builder.Services.AddSerilogLogging();

builder.Services.AddProjectServices(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()

                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");

app.UseIpRateLimiting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AmxBookstore API V1");
});

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseErrorHandlingMiddleware();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    RoleInitializer.InitializeAsync(roleManager, userManager).Wait();
}

app.Run();

public partial class Program { }
