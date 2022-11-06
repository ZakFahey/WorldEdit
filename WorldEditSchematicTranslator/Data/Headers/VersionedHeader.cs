namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private record VersionedHeader(Version Version, int X, int Y, int Width, int Height)
        : Header(X, Y, Width, Height);
}