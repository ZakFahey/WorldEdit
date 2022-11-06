#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record DisplayItemData(int X, int Y, NetItem Item) : EntityData(X, Y)
    {
        public static readonly DisplayItemData Instance = new(0, 0, default);
        public override DisplayItemData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(), ReadNetItem(BinaryReader));
        protected override void WriteInner(BinaryWriter BinaryWriter) =>
            Write(BinaryWriter, (NetItem)Item);
    }
}