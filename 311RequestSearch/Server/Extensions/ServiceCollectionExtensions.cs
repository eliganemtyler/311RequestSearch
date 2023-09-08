using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Tyler.Core.Caching;
using _311RequestSearch.Server.Filters;

namespace _311RequestSearch.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ModelValidationFilter));
            })
            .AddNewtonsoftJson(options =>             
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            return services;
        }

        public static IServiceCollection RegisterTylerCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            string redisService = configuration.GetValue<string>("Redis:Config");
        
            if (!String.IsNullOrWhiteSpace(redisService))
            {    
                // Enable redis for anti-forgery key caching    
                var redis = ConnectionMultiplexer.Connect(redisService);    
                services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");    
                
                // Initialize Redis cache    
                Cache.Initialize(CacheProvider.Redis);  
            }
            
            return services;
        }

        public static IServiceCollection RegisterCustomServices(this IServiceCollection services)
        {
            services.AddScoped<ApiExceptionFilter>();
            return services;
        }
    }
}
