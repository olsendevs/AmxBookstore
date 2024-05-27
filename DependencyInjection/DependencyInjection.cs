using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using AmxBookstore.Infrastructure.Persistence;
using AmxBookstore.Infrastructure.Identity;
using AmxBookstore.Domain.Interfaces;
using AmxBookstore.Infrastructure.Repositories;
using System.Reflection;
using AmxBookstore.Application.Interfaces;
using AmxBookstore.Application.Services;
using Serilog;
using AmxBookstore.Application.UseCases.Books.Queries.GetAllBooks;
using FluentValidation.AspNetCore;
using FluentValidation;
using AmxBookstore.Application.Profiles;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using AmxBookstore.Application.Models;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using AspNetCoreRateLimit;
using System;
using Polly.Retry;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data.Common;
using Shared;

namespace AmxBookstore.Shared.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDb")
                       .AddInterceptors(new ResilientDbCommandInterceptor(GetDbRetryPolicy())));

            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("IdentityDb"));

            services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IAuthService, AuthService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(GetAllBooksQueryHandler).Assembly));

            services.AddAutoMapper(typeof(AutoMapperProfile));

            services.AddFluentValidationAutoValidation()
                    .AddValidatorsFromAssemblyContaining<AmxBookstore.Application.Validators.UserDTOValidator>()
                    .AddValidatorsFromAssemblyContaining<AmxBookstore.Application.Validators.BookDTOValidator>()
                    .AddValidatorsFromAssemblyContaining<AmxBookstore.Application.Validators.OrderDTOValidator>()
                    .AddValidatorsFromAssemblyContaining<AmxBookstore.Application.Validators.StockDTOValidator>();

            services.AddSingleton(Log.Logger);

            services.AddMemoryCache();

            services.AddHealthChecks();

            services.AddHttpClient("externalService")
                .AddPolicyHandler(GetHttpRetryPolicy())
                .AddPolicyHandler(GetHttpCircuitBreakerPolicy());

            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddInMemoryRateLimiting();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AmxBookstore API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, new string[] { } }
                });

                c.DocInclusionPredicate((version, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes(true)
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v}" == version);
                });

                c.TagActionsBy(api => new List<string> { api.GroupName });
                c.DocInclusionPredicate((name, api) => true);
                c.OperationFilter<RemoveVersionFromParameter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPath>();
            });

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("SellerPolicy", policy => policy.RequireRole("Seller"));
                options.AddPolicy("ClientPolicy", policy => policy.RequireRole("Client"));
            });

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            return services;
        }

        private static AsyncRetryPolicy GetDbRetryPolicy()
        {
            return Policy
                .Handle<DbException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));
        }
    }
}
