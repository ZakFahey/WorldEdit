namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private readonly record struct ProgramFlags(bool DoLog, bool Exit,
        bool UseFailExitCode, string? DirectFile, string? TempDirectory,
        string? FromVersion, SearchOption SearchOption, bool DeleteOldFiles)
    {
        public ProgramFlags(string[] Args) : this(!Args.Contains("-nolog"),
                                                   Args.Contains("-exit"),
                                                   Args.Contains("-usefailexitcode"),
                                                   GetValue(Args, "-file"),
                                                   GetValue(Args, "-tempdirectory"),
                                                   GetValue(Args, "-fromversion"),
                                                  (Args.Contains("-topdirectoryonly")
                                                    ? SearchOption.TopDirectoryOnly
                                                    : SearchOption.AllDirectories),
                                                  !Args.Contains("-keepallfiles")) { }
        private static string? GetValue(string[] Args, string Flag)
        {
            int flagIndex = Array.IndexOf(Args, Flag);
            return (((flagIndex < 0) || (flagIndex >= (Args.Length - 1)))
                        ? null
                        : Args[flagIndex + 1]);
        }
    }
}