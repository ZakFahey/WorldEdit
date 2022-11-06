namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private static Dictionary<Version, ReadEntitiesD> ReadEntities = null!;
    private static void InitializeEntitiesReaders() => ReadEntities = new()
    {
        [V1_0] = (br => new(br)),
        [V2_0] = (br => new(br)),
        [V3_0] = (br => new(br))
    };
}