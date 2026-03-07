#pragma warning disable IDE0004 // unneded cast, but i prefer explicit BinaryWriter usage
namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed record DisplayDollData(int X, int Y, NetItem[] Items, NetItem[] Dyes, NetItem[] Misc, byte Pose)
        : EntityData(X, Y)
    {
        public static readonly DisplayDollData Instance = new(0, 0, default!, default!, default!, 0);
        public override DisplayDollData Read(BinaryReader BinaryReader) =>
            new(BinaryReader.ReadInt32(), BinaryReader.ReadInt32(),
                ReadNetItems(BinaryReader), ReadNetItems(BinaryReader),
                ReadNetItems(BinaryReader), BinaryReader.ReadByte());
        protected override void WriteInner(BinaryWriter BinaryWriter)
        {
            Write(BinaryWriter, (NetItem[])Items);
            Write(BinaryWriter, (NetItem[])Dyes);
            Write(BinaryWriter, (NetItem[])Misc);
            BinaryWriter.Write((byte)Pose);
        }

        public static DisplayDollData FromOldFormat(DisplayItemsData Old)
        {
            var items = new NetItem[Old.Items.Length + 1];
            Array.Copy(Old.Items, items, Old.Items.Length);
            items[Old.Items.Length] = new(0, 0, 0);

            var dyes = new NetItem[Old.Dyes.Length + 1];
            Array.Copy(Old.Dyes, dyes, Old.Dyes.Length);
            dyes[Old.Dyes.Length] = new(0, 0, 0);

            return new(Old.X, Old.Y, items, dyes, new NetItem[] { new(0, 0, 0) }, 0);
        }
    }
}
