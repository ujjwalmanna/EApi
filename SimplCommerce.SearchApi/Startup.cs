using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Polly;
using SimplCommerce.SearchApi.HttpManager;
using SimplCommerce.SearchApi.Configurations;

namespace SimplCommerce.SearchApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IAsyncPolicy<HttpResponseMessage> httWaitAndpRetryPolicy =
               Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                   .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
            var elasticConfig = Configuration.GetSection("ElasticSettings").Get<ElasticSettings>();

            services.AddHttpClient<IElasticClient, ElasticClient>(client =>
            {
                client.WithUserInfo(elasticConfig.UserId, elasticConfig.Password);
                client.Timeout = new TimeSpan(0,0,2);
            }).AddPolicyHandler(httWaitAndpRetryPolicy);


            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();
                c.SwaggerDoc("v1", new Info { Title = "SimplCommerce.SearchApi", Version = "v1" });
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseMiddleware<CustomErrorMiddleware>();
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {

                // To deploy on IIS
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimplCommerce.SearchApi");


            });
            app.UseMvc();
        }
    }
}
