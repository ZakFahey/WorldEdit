﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Hooks;
using Terraria;
using TShockAPI;
using WorldEdit.Commands;

namespace WorldEdit
{
    [APIVersion(1, 12)]
    public class WorldEdit : TerrariaPlugin
    {
        public static List<byte[]> BiomeConversions = new List<byte[]>();
        public static List<string> BiomeNames = new List<string>();
        public static PlayerInfo[] Players = new PlayerInfo[256];
        public static List<Func<int, int, int, bool>> Selections = new List<Func<int, int, int, bool>>();
        public static List<string> SelectionNames = new List<string>();
        public static Dictionary<string, byte> TileNames = new Dictionary<string, byte>();
        public static Dictionary<string, byte> WallNames = new Dictionary<string, byte>();

        public override string Author
        {
            get { return "MarioE"; }
        }
        private BlockingCollection<WECommand> CommandQueue = new BlockingCollection<WECommand>();
        private Thread CommandQueueThread;
        public override string Description
        {
            get { return "Adds commands for mass editing of blocks."; }
        }
        public override string Name
        {
            get { return "WorldEdit"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public WorldEdit(Main game)
            : base(game)
        {
            for (int i = 0; i < 256; i++)
            {
                Players[i] = new PlayerInfo();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Initialize -= OnInitialize;
                NetHooks.GetData -= OnGetData;
                ServerHooks.Leave -= OnLeave;
            }
        }
        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            NetHooks.GetData += OnGetData;
            ServerHooks.Leave += OnLeave;
        }

        void OnGetData(GetDataEventArgs e)
        {
            if (!e.Handled && e.MsgID == PacketTypes.Tile)
            {
                PlayerInfo info = Players[e.Msg.whoAmI];
                if (info.pt != 0)
                {
                    int X = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 1);
                    int Y = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 5);
                    if (X >= 0 && Y >= 0 && X < Main.maxTilesX && Y < Main.maxTilesY)
                    {
                        if (info.pt == 1)
                        {
                            info.x = X;
                            info.y = Y;
                            TShock.Players[e.Msg.whoAmI].SendMessage(String.Format("Set point 1.", X, Y), Color.Yellow);
                        }
                        else
                        {
                            info.x2 = X;
                            info.y2 = Y;
                            TShock.Players[e.Msg.whoAmI].SendMessage(String.Format("Set point 2.", X, Y), Color.Yellow);
                        }
                        info.pt = 0;
                        e.Handled = true;
                        TShock.Players[e.Msg.whoAmI].SendTileSquare(X, Y, 3);
                    }
                }
            }
        }
        void OnInitialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Biome, "/biome"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Copy, "/copy"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Drain, "/drain"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Flip, "/flip"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Flood, "/flood"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Paste, "/paste"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", PointCmd, "/point"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Redo, "/redo"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Replace, "/replace"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", ReplaceWall, "/replacewall"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Rotate, "/rotate"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Schematic, "/schematic", "/schem"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Select, "/select"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Set, "/set"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", SetWall, "/setwall"));
            TShockAPI.Commands.ChatCommands.Add(new Command("worldedit", Undo, "/undo"));

            #region Biomes
            // 255 => remove
            byte[] Corruption = { 0, 25, 112, 23, 24, 255, 32 };
            byte[] Hallow = { 0, 117, 116, 109, 110, 113, 255 };
            byte[] Jungle = { 59, 1, 53, 60, 61, 74, 69 };
            byte[] Mushroom = { 59, 1, 53, 70, 71, 255, 255 };
            byte[] Normal = { 0, 1, 53, 2, 3, 73, 255 };
            BiomeConversions.Add(Corruption);
            BiomeConversions.Add(Hallow);
            BiomeConversions.Add(Jungle);
            BiomeConversions.Add(Mushroom);
            BiomeConversions.Add(Normal);
            BiomeNames.Add("corruption");
            BiomeNames.Add("hallow");
            BiomeNames.Add("jungle");
            BiomeNames.Add("mushroom");
            BiomeNames.Add("normal");
            #endregion
            #region Selections
            Selections.Add((i, j, plr) => ((i + j) & 1) == 0);
            Selections.Add((i, j, plr) => ((i + j) & 1) == 1);
            Selections.Add((i, j, plr) =>
            {
                PlayerInfo info = Players[plr];

                int X = Math.Min(info.x, info.x2);
                int Y = Math.Min(info.y, info.y2);
                int X2 = Math.Max(info.x, info.x2);
                int Y2 = Math.Max(info.y, info.y2);

                Vector2 center = new Vector2((float)(X2 - X) / 2, (float)(Y2 - Y) / 2);
                float major = Math.Max(center.X, center.Y);
                float minor = Math.Min(center.X, center.Y);
                if (center.Y > center.X)
                {
                    float temp = major;
                    major = minor;
                    minor = temp;
                }
                return (i - center.X - X) * (i - center.X - X) / (major * major) + (j - center.Y - Y) * (j - center.Y - Y) / (minor * minor) <= 1;
            });
            Selections.Add((i, j, plr) => true);
            Selections.Add((i, j, plr) =>
            {
                return i == Players[plr].x || i == Players[plr].x2 || j == Players[plr].y || j == Players[plr].y2;
            });
            SelectionNames.Add("altcheckers");
            SelectionNames.Add("checkers");
            SelectionNames.Add("ellipse");
            SelectionNames.Add("normal");
            SelectionNames.Add("outline");
            #endregion
            #region Tile Names
            TileNames.Add("dirt", 0);
            TileNames.Add("stone", 1);
            TileNames.Add("grass", 2);
            TileNames.Add("iron", 6);
            TileNames.Add("copper", 7);
            TileNames.Add("gold", 8);
            TileNames.Add("silver", 9);
            TileNames.Add("platform", 19);
            TileNames.Add("demonite", 22);
            TileNames.Add("corrupt grass", 23);
            TileNames.Add("ebonstone", 25);
            TileNames.Add("wood", 30);
            TileNames.Add("meteorite", 37);
            TileNames.Add("gray brick", 38);
            TileNames.Add("red brick", 39);
            TileNames.Add("clay", 40);
            TileNames.Add("blue brick", 41);
            TileNames.Add("green brick", 43);
            TileNames.Add("pink brick", 44);
            TileNames.Add("gold brick", 45);
            TileNames.Add("silver brick", 46);
            TileNames.Add("copper brick", 47);
            TileNames.Add("spike", 48);
            TileNames.Add("cobweb", 51);
            TileNames.Add("sand", 53);
            TileNames.Add("glass", 54);
            TileNames.Add("obsidian", 56);
            TileNames.Add("ash", 57);
            TileNames.Add("hellstone", 58);
            TileNames.Add("mud", 59);
            TileNames.Add("jungle grass", 60);
            TileNames.Add("sapphire", 63);
            TileNames.Add("ruby", 64);
            TileNames.Add("emerald", 65);
            TileNames.Add("topaz", 66);
            TileNames.Add("amethyst", 67);
            TileNames.Add("diamond", 68);
            TileNames.Add("mushroom grass", 70);
            TileNames.Add("obsidian brick", 75);
            TileNames.Add("hellstone brick", 76);
            TileNames.Add("cobalt", 107);
            TileNames.Add("mythril", 108);
            TileNames.Add("hallowed grass", 109);
            TileNames.Add("adamantite", 111);
            TileNames.Add("ebonsand", 112);
            TileNames.Add("pearlsand", 116);
            TileNames.Add("pearlstone", 117);
            TileNames.Add("pearlstone brick", 118);
            TileNames.Add("iridescent brick", 119);
            TileNames.Add("mudstone block", 120);
            TileNames.Add("cobalt brick", 121);
            TileNames.Add("mythril brick", 122);
            TileNames.Add("silt", 123);
            TileNames.Add("wooden beam", 124);
            TileNames.Add("ice", 127);
            TileNames.Add("crystal", 129);
            TileNames.Add("active stone", 130);
            TileNames.Add("inactive stone", 131);
            TileNames.Add("demonite brick", 140);
            TileNames.Add("explosives", 141);
            TileNames.Add("candy cane", 145);
            TileNames.Add("green candy cane", 146);
            TileNames.Add("snow", 147);
            TileNames.Add("snow brick", 148);
            // These are not actually correct, but are for ease of usage.
            TileNames.Add("air", 149);
            TileNames.Add("lava", 150);
            TileNames.Add("water", 151);
            TileNames.Add("wire", 152);
            #endregion
            #region Wall Names
            WallNames.Add("air", 0);
            WallNames.Add("stone", 1);
            WallNames.Add("ebonstone", 3);
            WallNames.Add("wood", 4);
            WallNames.Add("gray brick", 5);
            WallNames.Add("red brick", 6);
            WallNames.Add("gold brick", 10);
            WallNames.Add("silver brick", 11);
            WallNames.Add("copper brick", 12);
            WallNames.Add("hellstone brick", 13);
            WallNames.Add("mud", 15);
            WallNames.Add("dirt", 16);
            WallNames.Add("blue brick", 17);
            WallNames.Add("green brick", 18);
            WallNames.Add("pink brick", 19);
            WallNames.Add("obsidian brick", 20);
            WallNames.Add("glass", 21);
            WallNames.Add("pearlstone brick", 22);
            WallNames.Add("iridescent brick", 23);
            WallNames.Add("mudstone brick", 24);
            WallNames.Add("cobalt brick", 25);
            WallNames.Add("mythril brick", 26);
            WallNames.Add("planked", 27);
            WallNames.Add("pearlstone", 28);
            WallNames.Add("candy cane", 29);
            WallNames.Add("green candy cane", 30);
            WallNames.Add("snow brick", 31);
            #endregion
            CommandQueueThread = new Thread(QueueCallback);
            CommandQueueThread.Start();
            Directory.CreateDirectory("worldedit");
        }
        void OnLeave(int plr)
        {
            Tools.ClearClipboard(plr);
            Tools.ClearHistory(plr);
            Players[plr] = new PlayerInfo();
        }

        void QueueCallback(object t)
        {
            while (!Netplay.disconnect)
            {
                WECommand command = CommandQueue.Take();
                command.Clamp();
                command.Execute();
            }
        }

        void Biome(CommandArgs e)
        {
            if (e.Parameters.Count != 2)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //biome <biome1> <biome2>", Color.Red);
                return;
            }
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
            {
                e.Player.SendMessage("Invalid selection.", Color.Red);
                return;
            }

            byte biome1 = 255;
            byte biome2 = 255;
            for (byte i = 0; i < BiomeNames.Count; i++)
            {
                if (e.Parameters[0].ToLower() == BiomeNames[i])
                {
                    biome1 = i;
                }
                if (e.Parameters[1].ToLower() == BiomeNames[i])
                {
                    biome2 = i;
                }
            }
            if (biome1 == 255 || biome2 == 255)
            {
                e.Player.SendMessage("Invalid biome.", Color.Red);
                return;
            }

            int x = Math.Min(info.x, info.x2);
            int y = Math.Min(info.y, info.y2);
            int x2 = Math.Max(info.x, info.x2);
            int y2 = Math.Max(info.y, info.y2);
            CommandQueue.Add(new BiomeCommand(x, y, x2, y2, e.Player.Index, biome1, biome2));
        }
        void Copy(CommandArgs e)
        {
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
            {
                e.Player.SendMessage("Invalid selection.", Color.Red);
                return;
            }

            int x = Math.Min(info.x, info.x2);
            int y = Math.Min(info.y, info.y2);
            int x2 = Math.Max(info.x, info.x2);
            int y2 = Math.Max(info.y, info.y2);
            CommandQueue.Add(new CopyCommand(x, y, x2, y2, e.Player.Index));
        }
        void Drain(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //drain <radius>", Color.Red);
                return;
            }

            int radius;
            if (!int.TryParse(e.Parameters[0], out radius) || radius <= 0)
            {
                e.Player.SendMessage("Invalid radius.", Color.Red);
                return;
            }
            CommandQueue.Add(new DrainCommand(e.Player.TileX, e.Player.TileY, e.Player.Index, radius));
        }
        void Flood(CommandArgs e)
        {
            if (e.Parameters.Count != 2)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //flood <lava|water> <radius>", Color.Red);
                return;
            }
            if (e.Parameters[0].ToLower() != "water" && e.Parameters[0].ToLower() != "lava")
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //flood <lava|water> <radius>", Color.Red);
                return;
            }

            int radius;
            if (!int.TryParse(e.Parameters[1], out radius) || radius <= 0)
            {
                e.Player.SendMessage("Invalid radius.", Color.Red);
                return;
            }
            CommandQueue.Add(new FloodCommand(e.Player.TileX, e.Player.TileY, e.Player.Index, radius, e.Parameters[0].ToLower() == "lava"));
        }
        void Flip(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //flip <direction>", Color.Red);
                return;
            }
            if (!Tools.HasClipboard(e.Player.Index))
            {
                e.Player.SendMessage("Invalid clipboard.", Color.Red);
                return;
            }

            byte flip = 0;
            foreach (char c in e.Parameters[0].ToLower())
            {
                if (c == 'x')
                {
                    flip ^= 1;
                }
                else if (c == 'y')
                {
                    flip ^= 2;
                }
                else
                {
                    e.Player.SendMessage("Invalid direction.", Color.Red);
                    return;
                }
            }
            CommandQueue.Add(new FlipCommand(e.Player.Index, flip));
        }
        void Paste(CommandArgs e)
        {
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1)
            {
                e.Player.SendMessage("Invalid first point.", Color.Red);
                return;
            }
            if (!Tools.HasClipboard(e.Player.Index))
            {
                e.Player.SendMessage("Invalid clipboard.", Color.Red);
                return;
            }

            CommandQueue.Add(new PasteCommand(info.x, info.y, e.Player.Index));
        }
        void PointCmd(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //point <1|2>", Color.Red);
                return;
            }

            switch (e.Parameters[0])
            {
                case "1":
                    Players[e.Player.Index].pt = 1;
                    e.Player.SendMessage("Hit a block to set point 1.", Color.Yellow);
                    break;
                case "2":
                    Players[e.Player.Index].pt = 2;
                    e.Player.SendMessage("Hit a block to set point 2.", Color.Yellow);
                    break;
                default:
                    e.Player.SendMessage("Invalid syntax! Proper syntax: //point <1|2>", Color.Red);
                    break;
            }
        }
        void Redo(CommandArgs e)
        {
            if (Players[e.Player.Index].redoLevel < 0)
            {
                e.Player.SendMessage("No redo history available.", Color.Red);
                return;
            }
            CommandQueue.Add(new RedoCommand(e.Player.Index));
        }
        void Replace(CommandArgs e)
        {
            if (e.Parameters.Count != 2)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //replace <tile1> <tile2>");
                return;
            }
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
            {
                e.Player.SendMessage("Invalid selection.", Color.Red);
                return;
            }

            byte tile1 = 0;
            if (!(TileNames.TryGetValue(e.Parameters[0].ToLower(), out tile1) || (byte.TryParse(e.Parameters[0], out tile1) && tile1 < 149)))
            {
                e.Player.SendMessage("Invalid tile.", Color.Red);
                return;
            }
            byte tile2 = 0;
            if (!(TileNames.TryGetValue(e.Parameters[1].ToLower(), out tile2) || (byte.TryParse(e.Parameters[1], out tile2) && tile2 < 149)))
            {
                e.Player.SendMessage("Invalid tile.", Color.Red);
                return;
            }

            int x = Math.Min(info.x, info.x2);
            int y = Math.Min(info.y, info.y2);
            int x2 = Math.Max(info.x, info.x2);
            int y2 = Math.Max(info.y, info.y2);
            CommandQueue.Add(new ReplaceCommand(x, y, x2, y2, e.Player.Index, tile1, tile2));
        }
        void ReplaceWall(CommandArgs e)
        {
            if (e.Parameters.Count != 2)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //replacewall <wall1> <wall2>");
                return;
            }
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
            {
                e.Player.SendMessage("Invalid selection.", Color.Red);
                return;
            }

            byte wall1 = 0;
            if (!(WallNames.TryGetValue(e.Parameters[0].ToLower(), out wall1) || (byte.TryParse(e.Parameters[0], out wall1) && wall1 < 32)))
            {
                e.Player.SendMessage("Invalid wall.", Color.Red);
                return;
            }
            byte wall2 = 0;
            if (!(WallNames.TryGetValue(e.Parameters[1].ToLower(), out wall2) || (byte.TryParse(e.Parameters[1], out wall2) && wall2 < 32)))
            {
                e.Player.SendMessage("Invalid wall.", Color.Red);
                return;
            }

            int x = Math.Min(info.x, info.x2);
            int y = Math.Min(info.y, info.y2);
            int x2 = Math.Max(info.x, info.x2);
            int y2 = Math.Max(info.y, info.y2);
            CommandQueue.Add(new ReplaceWallCommand(x, y, x2, y2, e.Player.Index, wall1, wall2));
        }
        void Rotate(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //rotate <angle>", Color.Red);
                return;
            }
            if (!Tools.HasClipboard(e.Player.Index))
            {
                e.Player.SendMessage("Invalid clipboard.", Color.Red);
                return;
            }
            int degrees;
            if (!int.TryParse(e.Parameters[0], out degrees) || degrees % 90 != 0)
            {
                e.Player.SendMessage("Invalid angle.", Color.Red);
                return;
            }
            CommandQueue.Add(new RotateCommand(e.Player.Index, degrees));
        }
        void Select(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //select <selection type>", Color.Red);
                return;
            }

            string name = e.Parameters[0].ToLower();
            int ID = -1;
            for (int i = 0; i < SelectionNames.Count; i++)
            {
                if (SelectionNames[i] == name)
                {
                    ID = i;
                    break;
                }
            }
            if (ID < 0)
            {
                e.Player.SendMessage("Invalid selection type.", Color.Red);
                return;
            }
            Players[e.Player.Index].select = ID;
            e.Player.SendMessage(String.Format("Set selection type to {0}.", name), Color.Green);
        }
        void Set(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //set <tile>");
                return;
            }
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
            {
                e.Player.SendMessage("Invalid selection.", Color.Red);
                return;
            }

            byte ID;
            if (e.Parameters[0].ToLower() == "nowire")
            {
                ID = 153;
            }
            else if (!(TileNames.TryGetValue(e.Parameters[0].ToLower(), out ID) || (byte.TryParse(e.Parameters[0], out ID) && ID < 149)))
            {
                e.Player.SendMessage("Invalid tile.", Color.Red);
                return;
            }

            int x = Math.Min(info.x, info.x2);
            int y = Math.Min(info.y, info.y2);
            int x2 = Math.Max(info.x, info.x2);
            int y2 = Math.Max(info.y, info.y2);
            CommandQueue.Add(new SetCommand(x, y, x2, y2, e.Player.Index, ID));
        }
        void SetWall(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //setwall <wall>");
                return;
            }
            PlayerInfo info = Players[e.Player.Index];
            if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
            {
                e.Player.SendMessage("Invalid selection.", Color.Red);
                return;
            }

            byte ID;
            if (!(WallNames.TryGetValue(e.Parameters[0].ToLower(), out ID) || (byte.TryParse(e.Parameters[0], out ID) && ID < 32)))
            {
                e.Player.SendMessage("Invalid wall.", Color.Red);
                return;
            }

            int x = Math.Min(info.x, info.x2);
            int y = Math.Min(info.y, info.y2);
            int x2 = Math.Max(info.x, info.x2);
            int y2 = Math.Max(info.y, info.y2);
            CommandQueue.Add(new SetWallCommand(x, y, x2, y2, e.Player.Index, ID));
        }
        void Schematic(CommandArgs e)
        {
            if (e.Parameters.Count == 0)
            {
                e.Player.SendMessage("Invalid syntax! Proper syntax: //schematic <cmd> [arg]", Color.Red);
                return;
            }
            string subCmd = e.Parameters[0].ToLower();
            switch (subCmd)
            {
                case "delete":
                    {
                        if (e.Parameters.Count != 2)
                        {
                            e.Player.SendMessage("Invalid syntax! Proper syntax: //schematic delete <name>", Color.Red);
                            break;
                        }
                        string schematicPath = Path.Combine("worldedit", String.Format("schematic-{0}.dat", e.Parameters[1]));
                        if (!File.Exists(schematicPath))
                        {
                            e.Player.SendMessage("Invalid schematic.", Color.Red);
                            break;
                        }
                        File.Delete(schematicPath);
                        e.Player.SendMessage("Deleted schematic.", Color.Green);
                    }
                    break;
                case "list":
                    {
                        if (e.Parameters.Count != 1 && e.Parameters.Count != 2)
                        {
                            e.Player.SendMessage("Invalid syntax! Proper syntax: //schematic list [page]", Color.Red);
                            break;
                        }

                        List<string> schematics = new List<string>(Directory.EnumerateFiles("worldedit", "schematic-*.dat"));
                        if (schematics.Count == 0)
                        {
                            e.Player.SendMessage("No schematics exist.", Color.Red);
                            break;
                        }

                        int maxPages = (int)Math.Ceiling(schematics.Count / 15d);
                        int page = 1;
                        if (e.Parameters.Count == 2)
                        {
                            if (!int.TryParse(e.Parameters[1], out page) || page <= 0 || page > maxPages)
                            {
                                e.Player.SendMessage("Invalid page.", Color.Red);
                                break;
                            }
                        }
                        page--;

                        e.Player.SendMessage(String.Format("Schematics: (Page {0}/{1})", page + 1, maxPages), Color.Green);
                        StringBuilder line = new StringBuilder();
                        for (int i = page * 15; i < page * 15 + 15 && i < schematics.Count; i++)
                        {
                            string schematic = schematics[i];
                            line.Append(schematic.Substring(20, schematic.Length - 24));
                            if ((i + 1) % 5 == 0)
                            {
                                e.Player.SendMessage(line.ToString(), Color.Yellow);
                                line.Clear();
                            }
                            else if (i != schematics.Count - 1)
                            {
                                line.Append(", ");
                            }
                        }
                        if (line.Length != 0)
                        {
                            e.Player.SendMessage(line.ToString(), Color.Yellow);
                        }
                    }
                    break;
                case "load":
                    {
                        if (e.Parameters.Count != 2)
                        {
                            e.Player.SendMessage("Invalid syntax! Proper syntax: //schematic load <name>", Color.Red);
                            break;
                        }
                        string schematicPath = Path.Combine("worldedit", String.Format("schematic-{0}.dat", e.Parameters[1]));
                        if (!File.Exists(schematicPath))
                        {
                            e.Player.SendMessage("Invalid schematic.", Color.Red);
                            return;
                        }
                        string clipboardPath = Path.Combine("worldedit", String.Format("clipboard-{0}.dat", e.Player.Index));
                        File.Copy(schematicPath, clipboardPath, true);
                        e.Player.SendMessage("Loaded schematic to clipboard.", Color.Green);
                    }
                    break;
                case "save":
                    {
                        if (e.Parameters.Count != 2)
                        {
                            e.Player.SendMessage("Invalid syntax! Proper syntax: //schematic save <name>", Color.Red);
                            break;
                        }
                        string clipboardPath = Path.Combine("worldedit", String.Format("clipboard-{0}.dat", e.Player.Index));
                        if (!File.Exists(clipboardPath))
                        {
                            e.Player.SendMessage("Invalid clipboard.", Color.Red);
                            break;
                        }
                        string schematicPath = Path.Combine("worldedit", String.Format("schematic-{0}.dat", e.Parameters[1]));
                        File.Copy(clipboardPath, schematicPath, true);
                        e.Player.SendMessage("Saved clipboard to schematic.", Color.Green);
                    }
                    break;
                default:
                    e.Player.SendMessage("Unknown subcommand.", Color.Red);
                    break;
            }
        }
        void Undo(CommandArgs e)
        {
            if (Players[e.Player.Index].undoLevel < 0)
            {
                e.Player.SendMessage("No undo history available.", Color.Red);
                return;
            }
            CommandQueue.Add(new UndoCommand(e.Player.Index));
        }
    }
}