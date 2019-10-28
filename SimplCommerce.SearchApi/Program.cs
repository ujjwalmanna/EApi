using System;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using System.IO;
using Serilog.Sinks.SystemConsole.Themes;

namespace SimplCommerce.SearchApi
{
    public class Program
    {
        public static int Main(string[] args)
            {
                try
                {
                    BuildWebHost(args).Build().Run();
                    Console.WriteLine("Host  started successfully");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Host terminated unexpectedly");
                    Console.Write(ex.ToString());
                    return 1;
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }

            public static IWebHostBuilder BuildWebHost(string[] args)
            {
                return WebHost.CreateDefaultBuilder(args)
                      .ConfigureAppConfiguration((hostingContext, config) =>
                      {
                          var env = hostingContext.HostingEnvironment;
                          config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                          config.AddEnvironmentVariables();
                      })
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseSerilog((provider, ContextBoundObject, loggerConfig) =>
                    {
                        var name = Assembly.GetExecutingAssembly().GetName();
                        loggerConfig
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .Destructure.ToMaximumDepth(100)
                            .Enrich.WithAspnetcoreHttpcontext(provider)
                            .Enrich.FromLogContext()
                            .Enrich.WithMachineName()
                            .Enrich.WithProperty("ApplicationName", $"SimplCommerce.SearchApi")
                            .Enrich.WithProperty("Assembly", $"{name.Name}")
                            .Enrich.WithProperty("Version", $"{name.Version}")
                            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information,
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", theme: SystemConsoleTheme.Literate)
                            .WriteTo.RollingFile("C:/temp/SearchApi/Logs/log-{Date}.txt", retainedFileCountLimit: 7, restrictedToMinimumLevel: LogEventLevel.Information,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
                    });


            }
        }
    }
