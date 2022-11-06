#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record ChestData(int X, int Y, NetItem[] Items) : EntityData(X, Y)
    {
        public static readonly ChestData Instance = new(0, 0, default!);
        public override ChestData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(), ReadNetItems(BinaryReader));
        protected override void WriteInner(BinaryWriter BinaryWriter) =>
            Write(BinaryWriter, (NetItem[])Items);
    }
}