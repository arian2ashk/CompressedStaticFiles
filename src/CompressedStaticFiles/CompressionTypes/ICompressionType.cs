namespace CompressedStaticFiles.CompressionTypes
{
    public interface ICompressionType
    {
        string Encoding { get; }
        string Extension { get; }
        string ContentType { get; }
    }
}
