using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace AspNetCore.PreCompressedStaticFiles
{
    public static class PreCompressedStaticFileExtensions
    {
        public static IApplicationBuilder UseCompressedStaticFiles(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<PreCompressedStaticFileMiddleware>();
        }


        public static IApplicationBuilder UseCompressedStaticFiles(this IApplicationBuilder app, PreCompressedStaticFileOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<PreCompressedStaticFileMiddleware>(Options.Create(options));
        }
    }
}
