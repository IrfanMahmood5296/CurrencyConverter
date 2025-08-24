using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Services;
using Currency.Application.Services.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrencyProviders(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<OpenExchangeRatesProviderService>();
            services.AddTransient<FrankFurterProviderService>();
            services.AddScoped<IProviderFactoryService, ProviderFactoryService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IRedisCacheService, RedisCacheService>();

            return services;
        }

        public static IServiceCollection AddIdentityServerConfig(this IServiceCollection services, IConfiguration _configuration)
        {
            services.AddIdentityServer()
                .AddInMemoryClients(Config.Clients)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddTestUsers(Config.Users)
                .AddDeveloperSigningCredential();


            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = _configuration["IdentityServerConfig:Authority"]!;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "currency_api");
                });
            });
            return services;
        }

        public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration _configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _configuration["RedisServerConfig:Authority"]!; 
            });

            return services;
        }
    }
}
