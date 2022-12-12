namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private record struct Tile(ushort sTileHeader, byte bTileHeader, byte bTileHeader2,
        byte bTileHeader3, ushort type, short frameX, short frameY, ushort wall, byte liquid)
    {
        public bool active() => ((sTileHeader & 32) == 32);

        public byte color() => (byte)(sTileHeader & 31);
        public byte wallColor() => (byte)(bTileHeader & 31);
        public bool fullbrightBlock() => ((bTileHeader3 & 128) == 128);
        public bool invisibleBlock() => ((bTileHeader3 & 32) == 32);
        public bool fullbrightWall() => ((sTileHeader & 32768) == 32768);
        public bool invisibleWall() => ((bTileHeader3 & 64) == 64);

        public void color(byte color) =>
            sTileHeader = (ushort)((sTileHeader & 65504) | color);
        public void wallColor(byte wallColor) =>
            bTileHeader = (byte)((bTileHeader & 224) | wallColor);
        public void fullbrightBlock(bool fullbrightBlock)
        {
            if (fullbrightBlock)
                bTileHeader3 |= 128;
            else
                bTileHeader3 = (byte)(bTileHeader3 & -129);
        }
        public void invisibleBlock(bool invisibleBlock)
        {
            if (invisibleBlock)
                bTileHeader3 |= 32;
            else
                bTileHeader3 = (byte)(bTileHeader3 & -33);
        }
        public void fullbrightWall(bool fullbrightWall)
        {
            if (fullbrightWall)
                sTileHeader |= 32768;
            else
                sTileHeader = (ushort)(sTileHeader & -32769);
        }
        public void invisibleWall(bool invisibleWall)
        {
            if (invisibleWall)
                bTileHeader3 |= 64;
            else
                bTileHeader3 = (byte)(bTileHeader3 & -65);
        }
    }
}