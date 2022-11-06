#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private abstract record EntityData(int X, int Y)
    {
        public abstract EntityData Read(BinaryReader BinaryReader);
        public void Write(BinaryWriter BinaryWriter)
        {
            BinaryWriter.Write((int)X);
            BinaryWriter.Write((int)Y);
            WriteInner(BinaryWriter);
        }
        protected virtual void WriteInner(BinaryWriter BinaryWriter) { }

        #region NetItem(s)

        public static NetItem ReadNetItem(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(), BinaryReader.ReadByte());
        public static void Write(BinaryWriter BinaryWriter, NetItem Item)
        {
            BinaryWriter.Write((int)Item.netId);
            BinaryWriter.Write((int)Item.stack);
            BinaryWriter.Write((byte)Item.prefixId);
        }
        public static NetItem[] ReadNetItems(BinaryReader BinaryReader)
        {
            NetItem[] items = new NetItem[BinaryReader.ReadInt32()];
            for (int i = 0, maxI = (items.Length - 1); i <= maxI; i++)
                items[i] = ReadNetItem(BinaryReader);
            return items;
        }
        public static void Write(BinaryWriter BinaryWriter, NetItem[] Items)
        {
            BinaryWriter.Write((int)Items.Length);
            foreach (NetItem item in Items)
                Write(BinaryWriter, item);
        }

        #endregion
    }
}