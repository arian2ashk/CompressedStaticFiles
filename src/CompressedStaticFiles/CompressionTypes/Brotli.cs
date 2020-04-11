namespace AspNetCore.PreCompressedStaticFiles.CompressionTypes
{
    public class Brotli : ICompressionType
    {
        internal const string encoding = "br";
        internal const string extension = ".br";
        internal const string contentType = "application/brotli";
        public string Encoding { get; } = encoding;
        public string Extension { get; } = extension;
        public string ContentType { get; } = contentType;
    }
}
