// <copyright file="Startup.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook
{
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using NiCE.Community.TeamsCallRecordWebhook.Configuration;
    using NiCE.Community.TeamsCallRecordWebhook.Formatter;
    using NiCE.Community.TeamsCallRecordWebhook.Interfaces;
    using NiCE.Community.TeamsCallRecordWebhook.Subscription;
    using Serilog;

    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> added by DI.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> added by DI.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(o => o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter()))
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NiCE.Community.Webhook", Version = "v1" });
            });

            services.AddHostedService<Worker>();

            services.Configure<ApiConfiguration>(this.Configuration.GetSection("WebHook"));

            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
            services.AddSingleton<CallRecordCache>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> added by DI.</param>
        public void Configure(IApplicationBuilder app/*, IWebHostEnvironment env*/)
        {
            // if (env.IsDevelopment())
            // {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiCE.Community.Webhook v1");
            });

            // }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSerilogRequestLogging(o =>
            {
                o.EnrichDiagnosticContext = (context, httpContext) =>
                {
                    var request = httpContext.Request;
                    var connection = httpContext.Connection;
                    context.Set("IPAddress", connection.RemoteIpAddress);
                    context.Set("Port", connection.RemotePort);
                };

                o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms for {IPAddress}:{Port}";
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
