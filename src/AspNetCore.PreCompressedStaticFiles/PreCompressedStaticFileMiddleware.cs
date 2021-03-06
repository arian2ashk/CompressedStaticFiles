﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.PreCompressedStaticFiles.CompressionTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.PreCompressedStaticFiles
{
    public class PreCompressedStaticFileMiddleware
    {
        private readonly IOptions<PreCompressedStaticFileOptions> _compressedStaticFileOptions;
        private readonly StaticFileMiddleware _base;
        private readonly ILogger _logger;

        public PreCompressedStaticFileMiddleware(
            RequestDelegate next, IWebHostEnvironment hostingEnv, IOptions<PreCompressedStaticFileOptions> compressedStaticFileOptions, ILoggerFactory loggerFactory)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (hostingEnv == null)
            {
                throw new ArgumentNullException(nameof(hostingEnv));
            }

            _logger = loggerFactory.CreateLogger<PreCompressedStaticFileMiddleware>();


            _compressedStaticFileOptions = compressedStaticFileOptions ?? throw new ArgumentNullException(nameof(compressedStaticFileOptions));
            if (!_compressedStaticFileOptions.Value.CompressionTypes.Any())
            {
                _compressedStaticFileOptions.Value.CompressionTypes.Add<Gzip>();
                _compressedStaticFileOptions.Value.CompressionTypes.Add<Brotli>();
            }

            InitializeStaticFileOptions(hostingEnv, compressedStaticFileOptions);

            _base = new StaticFileMiddleware(next, hostingEnv, compressedStaticFileOptions, loggerFactory);
        }

        private static void InitializeStaticFileOptions(IWebHostEnvironment hostingEnv, IOptions<PreCompressedStaticFileOptions> compressedStaticFileOptions)
        {
            compressedStaticFileOptions.Value.FileProvider ??= hostingEnv.WebRootFileProvider;
            var contentTypeProvider = compressedStaticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            if (contentTypeProvider is FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
            {
                // the StaticFileProvider would not serve the file if it does not know the content-type
                foreach (var compressionType in compressedStaticFileOptions.Value.CompressionTypes)
                {
                    if (!fileExtensionContentTypeProvider.Mappings.ContainsKey(compressionType.Extension))
                    {
                        fileExtensionContentTypeProvider.Mappings[compressionType.Extension] = compressionType.ContentType;
                    }
                }
            }
            compressedStaticFileOptions.Value.ContentTypeProvider = contentTypeProvider;

            var originalPrepareResponse = compressedStaticFileOptions.Value.OnPrepareResponse;
            compressedStaticFileOptions.Value.OnPrepareResponse = ctx =>
            {
                originalPrepareResponse(ctx);
                foreach (var compressionType in compressedStaticFileOptions.Value.CompressionTypes)
                {
                    if (ctx.File.Name.EndsWith(compressionType.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        // we need to restore the original content type, otherwise it would be based on the compression type
                        // (for example "application/brotli" instead of "text/html")
                        if (contentTypeProvider.TryGetContentType(ctx.File.PhysicalPath.Remove(
                            ctx.File.PhysicalPath.Length - compressionType.Extension.Length, compressionType.Extension.Length), out var contentType))
                            ctx.Context.Response.ContentType = contentType;
                        ctx.Context.Response.Headers.Add("Content-Encoding", new[] { compressionType.Encoding });
                    }
                }
            };
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                ProcessRequest(context);
            }
            return _base.Invoke(context);
        }

        private void ProcessRequest(HttpContext context)
        {
            var fileSystem = _compressedStaticFileOptions.Value.FileProvider;
            var originalFile = fileSystem.GetFileInfo(context.Request.Path);

            if (!originalFile.Exists)
            {
                return;
            }

            var supportedEncodings = GetSupportedEncodings(context);

            // try to find a compressed version of the file and ensure that it is smaller than the uncompressed version
            IFileInfo matchedFile = originalFile;
            foreach (var compressionType in supportedEncodings)
            {
                var fileExtension = _compressedStaticFileOptions.Value.CompressionTypes.FirstOrDefault(c=> c.Encoding == compressionType)?.Extension ?? string.Empty;
                var file = fileSystem.GetFileInfo(context.Request.Path + fileExtension);
                if (file.Exists && file.Length < matchedFile.Length)
                {
                    matchedFile = file;
                    break;
                }
            }

            if (matchedFile != originalFile)
            {
                // a compressed version exists and is smaller, change the path to serve the compressed file
                var matchedPath = context.Request.Path.Value + Path.GetExtension(matchedFile.Name);
                _logger.LogFileServed(context.Request.Path.Value, matchedPath, originalFile.Length, matchedFile.Length);
                context.Request.Path = new PathString(matchedPath);
            }
        }

        /// <summary>
        /// Find the encodings that are supported by the browser and by this middleware
        /// </summary>
        private IEnumerable<string> GetSupportedEncodings(HttpContext context)
        {
            var browserSupportedCompressionTypes = context.Request.Headers["Accept-Encoding"].ToString().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var validCompressionTypes = _compressedStaticFileOptions.Value.CompressionTypes.Encodings.Intersect(browserSupportedCompressionTypes, StringComparer.OrdinalIgnoreCase);
            return validCompressionTypes;
        }
    }
}
