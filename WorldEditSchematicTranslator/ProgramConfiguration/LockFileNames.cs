﻿namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private static Dictionary<Version, string?> LockFileNames = null!, LockFilePaths = null!;
    private static Dictionary<Version, string?> LockFileContent = null!;

    private static void InitializeLockFileNames()
    {
        LockFilePaths = LockFileNames = new()
        {
            [V1_0] = "1.4.0.lock",
            [V2_0] = "1.4.4.lock",
            [V3_0] = null
        };
        LockFileContent = new()
        {
            [V1_0] = null,
            [V2_0] = "1.4.4-with-version",
            [V3_0] = null
        };
    }
    private static void InitializeLockFilePaths(out string CurrentDirectory)
    {
        string dir = CurrentDirectory = Environment.CurrentDirectory;
        LockFilePaths = LockFileNames.ToDictionary(k => k.Key,
                                                   v => ((v.Value is null) ? null
                                                                           : Path.Combine(dir, v.Value)));
    }
}