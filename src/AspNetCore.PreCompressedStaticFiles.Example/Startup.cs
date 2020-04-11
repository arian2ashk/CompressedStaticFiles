using AspNetCore.PreCompressedStaticFiles.CompressionTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.PreCompressedStaticFiles.Example
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseDeveloperExceptionPage();

            var options = new PreCompressedStaticFileOptions();
            options.CompressionTypes.Add<Brotli>();
            options.CompressionTypes.Add<Gzip>();
            app.UsePreCompressedStaticFiles(options);
        }
    }
}
