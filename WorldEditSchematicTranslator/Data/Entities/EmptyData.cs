namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record EmptyData(int X, int Y) : EntityData(X, Y)
    {
        public static readonly EmptyData Instance = new(0, 0);
        public override EmptyData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32());
    }
}