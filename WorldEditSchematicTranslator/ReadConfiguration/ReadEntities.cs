namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private static Dictionary<Version, ReadEntitiesD> ReadEntities = null!;
    private static void InitializeEntitiesReaders() => ReadEntities = new()
    {
        [V1_0] = (br => new(br, useOldMannequinFormat: true)),
        [V2_0] = (br => new(br, useOldMannequinFormat: true)),
        [V3_0] = (br => new(br, useOldMannequinFormat: true)),
        [V4_0] = (br => new(br, useOldMannequinFormat: false))
    };
}