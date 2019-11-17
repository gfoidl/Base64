using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace gfoidl.Base64.WebDemo
{
    public static class Program
    {
        public static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();
        //---------------------------------------------------------------------
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
