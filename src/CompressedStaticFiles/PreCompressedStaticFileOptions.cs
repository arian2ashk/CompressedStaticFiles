using Microsoft.AspNetCore.Builder;

namespace AspNetCore.PreCompressedStaticFiles
{
    public class PreCompressedStaticFileOptions : StaticFileOptions
    {
        public CompressionTypeCollection CompressionTypes { get; } = new CompressionTypeCollection();
    }
}
