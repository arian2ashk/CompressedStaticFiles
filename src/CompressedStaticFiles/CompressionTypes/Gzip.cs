namespace CompressedStaticFiles.CompressionTypes
{
    public class Gzip : ICompressionType
    {
        internal const string encoding = "gzip";
        internal const string extension = ".gz";
        internal const string contentType = "application/x-gzip";
        public string Encoding { get; } = encoding;
        public string Extension { get; } = extension;
        public string ContentType { get; } = contentType;
    }
}
