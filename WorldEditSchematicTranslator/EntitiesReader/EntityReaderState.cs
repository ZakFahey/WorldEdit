#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private record struct EntityReaderState(bool Read, EntityType Type,
        int TotalCount, int ReadCount, EntityData Entity)
    {
        private bool IsNewType => (ReadCount == ((TotalCount == 0) ? 0 : 1));
        public EntityReaderState()
            : this(true, EntityType.None, 0, 0, null!) { }

        public void Write(BinaryWriter Writer)
        {
            if (!Read)
            {
                Writer.Write((int)0);
                return;
            }

            if (IsNewType)
                Writer.Write((int)TotalCount);
            if (TotalCount > 0)
                Entity.Write(Writer);
        }
    }
}