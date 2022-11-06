#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record SignData(int X, int Y, string Text) : EntityData(X, Y)
    {
        public static readonly SignData Instance = new(0, 0, string.Empty);
        public override SignData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(), BinaryReader.ReadString());
        protected override void WriteInner(BinaryWriter BinaryWriter) =>
            BinaryWriter.Write((string)Text);
    }
}