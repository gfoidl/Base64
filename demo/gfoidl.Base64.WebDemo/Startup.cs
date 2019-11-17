using System.IO.Pipelines;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gfoidl.Base64.WebDemo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }
        //---------------------------------------------------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("POST data");
                });

                endpoints.MapPost("/", async context =>
                {
                    // Very simple and hacky example

                    ReadResult readResult = await context.Request.BodyReader.ReadAsync();

                    await context.Response.WriteAsync("base64:\n");
                    Base64.Default.Encode(readResult.Buffer, context.Response.BodyWriter, out long _, out long written);

                    await context.Response.WriteAsync("\nbase64Url:\n");
                    Base64.Url.Encode(readResult.Buffer, context.Response.BodyWriter, out long _, out written);
                });
            });
        }
    }
}
