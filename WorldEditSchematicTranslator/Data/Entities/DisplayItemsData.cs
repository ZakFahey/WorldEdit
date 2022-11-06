#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record DisplayItemsData(int X, int Y, NetItem[] Items, NetItem[] Dyes)
        : EntityData(X, Y)
    {
        public static readonly DisplayItemsData Instance = new(0, 0, default!, default!);
        public override DisplayItemsData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(),
                ReadNetItems(BinaryReader), ReadNetItems(BinaryReader));
        protected override void WriteInner(BinaryWriter BinaryWriter)
        {
            Write(BinaryWriter, (NetItem[])Items);
            Write(BinaryWriter, (NetItem[])Dyes);
        }
    }
}