namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private static Dictionary<Version, bool> HeaderZipped = null!;
    private static Dictionary<Version, ReadHeaderD> ReadHeader = null!;
    private static void InitializeHeaderReaders()
    {
        HeaderZipped = new()
        {
            [V1_0] = true,
            [V2_0] = false,
            [V3_0] = false
        };
        ReadHeader = new();
        ReadHeader[V1_0] = ReadHeader[V2_0] = ((_, br) => new(br.ReadInt32(), br.ReadInt32(),
                                                              br.ReadInt32(), br.ReadInt32()));
        ReadHeader[V3_0] = ((fromVersion, br) =>
        {
            Version fileVersion = new(br.ReadInt32(), br.ReadInt32());
            if (fileVersion != fromVersion)
            {
                Console.Error.WriteLine($"[WorldEdit] Translating from v{fromVersion}, " +
                                        $"but file version is v{fileVersion}.");
                Environment.Exit(EXIT_CODE_VERSION_TRANSLATION_MISMATCH);
            }

            return new VersionedHeader(fromVersion, br.ReadInt32(), br.ReadInt32(),
                                                    br.ReadInt32(), br.ReadInt32());
        });
    }
}