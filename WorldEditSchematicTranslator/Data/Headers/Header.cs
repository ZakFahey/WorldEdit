#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private record Header(int X, int Y, int Width, int Height)
    {
        public virtual void Write(BinaryWriter BinaryWriter)
        {
            BinaryWriter.Write((int)LastVersion.Major);
            BinaryWriter.Write((int)LastVersion.Minor);
            BinaryWriter.Write((int)X);
            BinaryWriter.Write((int)Y);
            BinaryWriter.Write((int)Width);
            BinaryWriter.Write((int)Height);
        }
    }
}