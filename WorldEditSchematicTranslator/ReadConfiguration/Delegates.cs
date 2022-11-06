namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private delegate Header ReadHeaderD(Version FromVersion, BinaryReader BinaryReader);
    private delegate Tile ReadTileD(BinaryReader BinaryReader);
    private delegate EntitiesReader ReadEntitiesD(BinaryReader BinaryReader);
}