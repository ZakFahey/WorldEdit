namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private record struct Tile(ushort sTileHeader, byte bTileHeader, byte bTileHeader2,
        byte bTileHeader3, ushort type, short frameX, short frameY, ushort wall, byte liquid)
    { public bool active() => ((sTileHeader & 32) == 32); }
}