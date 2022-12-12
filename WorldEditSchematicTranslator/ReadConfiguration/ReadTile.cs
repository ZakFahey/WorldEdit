namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private static Dictionary<Version, ReadTileD> ReadTile = null!;
    private static void InitializeTilesReaders() => ReadTile = new()
    {
        #region V1_0

        [V1_0] = (br =>
        {
            Tile tile = new()
            {
                sTileHeader = br.ReadUInt16(),
                bTileHeader = br.ReadByte(),
                bTileHeader2 = br.ReadByte()
            };

            if (tile.active())
            {
                tile.type = br.ReadUInt16();
                if (TileFrameImportant[V1_0][tile.type])
                {
                    tile.frameX = br.ReadInt16();
                    tile.frameY = br.ReadInt16();
                }
            }
            tile.wall = br.ReadByte();
            tile.liquid = br.ReadByte();
            return tile;
        }),

        #endregion
        #region V2_0

        [V2_0] = (br =>
        {
            Tile tile = new()
            {
                sTileHeader = br.ReadUInt16(),
                bTileHeader = br.ReadByte(),
                bTileHeader2 = br.ReadByte()
            };

            if (tile.active() && (tile.color() == OLD_ILLUMINANT_PAINT_ID))
            {
                tile.color(0);
                tile.fullbrightBlock(true);
            }
            if (tile.wallColor() == OLD_ILLUMINANT_PAINT_ID)
            {
                tile.wallColor(0);
                tile.fullbrightWall(true);
            }

            if (tile.active())
            {
                tile.type = br.ReadUInt16();
                if (TileFrameImportant[V2_0][tile.type])
                {
                    tile.frameX = br.ReadInt16();
                    tile.frameY = br.ReadInt16();
                }
            }
            tile.wall = br.ReadUInt16();
            tile.liquid = br.ReadByte();
            return tile;
        }),

        #endregion
        #region V3_0

        [V3_0] = (br =>
        {
            Tile tile = new()
            {
                sTileHeader = br.ReadUInt16(),
                bTileHeader = br.ReadByte(),
                bTileHeader2 = br.ReadByte(),
                bTileHeader3 = br.ReadByte()
            };

            if (tile.active())
            {
                tile.type = br.ReadUInt16();
                if (TileFrameImportant[V3_0][tile.type])
                {
                    tile.frameX = br.ReadInt16();
                    tile.frameY = br.ReadInt16();
                }
            }
            tile.wall = br.ReadUInt16();
            tile.liquid = br.ReadByte();
            return tile;
        })

        #endregion
    };
}