using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Spt.Shared.AspNetCore;
using Spt.Shared.AspNetCore.Middlewares;

namespace Spt.Portal.Web
{
    public class Startup
    {
        public const string MICROSERVICE_NAME = "portal-web";
        public const string MICROSERVICE_URL_PREFIX = "/portal";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Applications Insights initialization
            services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer(MICROSERVICE_NAME));
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<ApplicationInsightsLoggingMiddleware>();

            // this defines a CORS policy called "default"
            services.AddCors(options =>
            {
                var origins = new List<string>() { Configuration["NativeClientOAuthConfiguration:Authority"] };
                if (origins[0].Contains("://cloud-dev."))
                    origins.Add("https://localhost:5001");
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(origins.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            services.AddControllers(options =>
            {
                options.Filters.Add(new ResponseExceptionFilter());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = ValidationFactory.InvalidModelStateResponseFactory;
            });
            services.AddHttpContextAccessor();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            //Enable Reserved Proxy - Service fabric
            app.UsePathBase(MICROSERVICE_URL_PREFIX);
            app.Use((context, next) =>
            {
                //Apply the fabric URL if accessing by the Service Fabric Reverse Proxy
                if (context.Request.Headers.TryGetValue("X-Forwarded-Host", out var host))
                {
                    context.Request.PathBase = MICROSERVICE_URL_PREFIX;
                    context.Request.Host = new HostString(host);
                    context.Request.Scheme = "https";
                }

                return next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<AccessTokenFromQueryStringMiddleware>();
            app.UseMiddleware<ApplicationInsightsLoggingMiddleware>();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

#if DEBUG
                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
#endif
            });
        }
    }
}
