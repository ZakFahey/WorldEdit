#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record ItemTypeData(int X, int Y, int ItemType) : EntityData(X, Y)
    {
        public static readonly ItemTypeData Instance = new(0, 0, 0);
        public override ItemTypeData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(), BinaryReader.ReadInt32());
        protected override void WriteInner(BinaryWriter BinaryWriter) =>
            BinaryWriter.Write((int)ItemType);
    }
}
