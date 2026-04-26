using Terraria;
using Terraria.DataStructures;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit
{
    public interface PlaceID
    {
        string Name { get; }
        bool Is(ITile tile);
        bool CanSet(ITile tile,
            Selection selection, Expressions.Expression expression,
            MagicWand magicWand, int x, int y, TSPlayer player);
        protected static bool CanSetBase(ITile tile,
            Selection selection, Expressions.Expression expression,
            MagicWand magicWand, int x, int y, TSPlayer player
        )
        {
            return selection(x, y, player)
                && expression.Evaluate(tile)
                && magicWand.InSelection(x, y);
        }
        void SetTile(int x, int y);
        void SwapTile(int x, int y);
    }
    public interface TilePlaceID : PlaceID { }
    public enum LiquidKind
    {
        Lava,
        Honey,
        Water,
        Shimmer,
    }
    public readonly record struct AirPlaceID : TilePlaceID
    {
        public string Name => "air";
        public bool Is(ITile tile) => !tile.active();
        public bool CanSet(ITile tile, Selection selection, Expression expression, MagicWand magicWand, int x, int y, TSPlayer player) =>
            tile.active() && PlaceID.CanSetBase(tile, selection, expression, magicWand, x, y, player);
        public void SetTile(int x, int y)
        {
            var tile = Main.tile[x, y];
            tile.active(false);
            tile.frameX = -1;
            tile.frameY = -1;
            tile.liquidType(0);
            tile.liquid = 0;
            tile.type = 0;
        }
        public void SwapTile(int x, int y)
        {
            SetTile(x, y);
        }
    }
    public readonly record struct LiquidPlaceID(LiquidKind kind) : TilePlaceID
    {
        public string Name => kind switch
        {
            LiquidKind.Lava => "lava",
            LiquidKind.Honey => "honey",
            LiquidKind.Water => "water",
            LiquidKind.Shimmer => "shimmer",
            _ => throw new System.InvalidOperationException(),
        };
        public bool Is(ITile tile) => kind switch
        {
            LiquidKind.Lava => tile.liquidType() == 1,
            LiquidKind.Honey => tile.liquidType() == 2,
            LiquidKind.Water => tile.liquidType() == 0,
            LiquidKind.Shimmer => tile.liquidType() == 3,
            _ => throw new System.InvalidOperationException(),
        } && tile.liquid > 0;

        public bool CanSet(ITile tile, Selection selection, Expression expression, MagicWand magicWand, int x, int y, TSPlayer player) =>
            kind switch
            {
                LiquidKind.Lava => (tile.liquid != 255) || (tile.liquidType() != 1),
                LiquidKind.Honey => (tile.liquid != 255) || (tile.liquidType() != 2),
                LiquidKind.Water => (tile.liquid != 255) || (tile.liquidType() != 0),
                LiquidKind.Shimmer => (tile.liquid != 255) || (tile.liquidType() != 3),
                _ => throw new System.InvalidOperationException(),
            } && PlaceID.CanSetBase(tile, selection, expression, magicWand, x, y, player);

        public void SetTile(int x, int y)
        {
            var type = kind switch
            {
                LiquidKind.Lava => 1,
                LiquidKind.Honey => 2,
                LiquidKind.Water => 0,
                LiquidKind.Shimmer => 3,
                _ => throw new System.InvalidOperationException(),
            };
            var tile = Main.tile[x, y];
            tile.active(false);
            tile.liquidType(type);
            tile.liquid = 255;
            tile.type = 0;
        }

        public void SwapTile(int x, int y)
        {
            SetTile(x, y);
        }
    }
    public readonly record struct BlockPlaceID(int tileID, int placeStyle, string name) : TilePlaceID
    {
        public string Name => name;

        public bool CanSet(ITile tile, Selection selection, Expression expression, MagicWand magicWand, int x, int y, TSPlayer player)
        {
            return
                (!tile.active() || (tile.type != tileID)) &&
                    PlaceID.CanSetBase(tile, selection, expression, magicWand, x, y, player);
        }
        public bool Is(ITile tile)
        {
            return tile.active() && tile.type == tileID;
        }

        public void SetTile(int x, int y)
        {
			Main.tile[x, y].Clear(TileDataType.Tile);
			WorldGen.PlaceTile(x, y, tileID, style: placeStyle);

			#region Workarounds for WorldGen.PlaceTile being silly
			// for some reason PlaceTile discards the placestyle for books
			if (tileID == 50)
			{
				Main.tile[x, y].frameX = (short)(18 * placeStyle);
			}
            #endregion
        }

        public void SwapTile(int x, int y)
        {
            if (Main.tile[x, y].active())
            {
                Main.tile[x, y].type = (ushort)tileID;
            }
            else
            {
                SetTile(x, y);
            }
        }
    }
    public readonly record struct WallPlaceID(int wallID, string name) : PlaceID
    {
        public string Name => name;

        public bool CanSet(ITile tile, Selection selection, Expression expression, MagicWand magicWand, int x, int y, TSPlayer player)
        {
            return tile.wall != wallID && PlaceID.CanSetBase(tile, selection, expression, magicWand, x, y, player);
        }
        public bool Is(ITile tile)
        {
            return tile.wall == wallID;
        }
        public void SetTile(int x, int y)
        {
            Main.tile[x, y].wall = (ushort)wallID;
        }
        public void SwapTile(int x, int y)
        {
            SetTile(x, y);
        }
    }
}