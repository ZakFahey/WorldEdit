#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record LogicSensorData(int X, int Y, int Type) : EntityData(X, Y)
    {
        public static readonly LogicSensorData Instance = new(0, 0, 0);
        public override LogicSensorData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(), BinaryReader.ReadInt32());
        protected override void WriteInner(BinaryWriter BinaryWriter) =>
            BinaryWriter.Write((int)Type);
    }
}