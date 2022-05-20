using AutoClick.Models;
using AutoClick.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;

namespace AutoClick
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                var env = hostContext.HostingEnvironment;
                config.SetBasePath(env.ContentRootPath)
                      .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);
            })
          .ConfigureServices((hostContext, services) =>
          {
              services.AddHttpClient();
              services.AddSingleton<APIService>();
              services.AddSingleton<LineNotifyService>();

              services.AddHostedService<HItCardService>();
          });


    }
}
