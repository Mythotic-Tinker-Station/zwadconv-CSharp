using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static zwadconv_CSharp.MapConverterTypes;
using static zwadconv_CSharp.MapConverterConstants;
using static zwadconv_CSharp.Constants;

namespace zwadconv_CSharp
{
    class MapConverter
    {
        static uint[] FlyBehavior = new uint[1024];
        static uint FlySize, FlyScript;
        static uint[] ScriptStarts = new uint[255];

        static byte[] SectorTags = new byte[65536];
        static byte[] RefCount = new byte[65536 / 8];
        static byte[] UsedTags = new byte[256 / 8];

        static byte[] LineIDMaps = new byte[65536];
        static ushort[] ReverseLineIDMaps = new ushort[256];

        static bool SomeProblems = false;

        static bool UsedLineIDs;

        public static void ToHexen(byte[] file, Map map)
        {
            // Check type first, Doom maps need more work than Hexen maps
            // Doom to Hexen code: (Adapt to convert directly to UDMF)
            UsedLineIDs = false;
            FlySize = 2;
            FlyScript = 0;

            List<MapThing_Hexen> newThings = new();
            List<MapLineDef_Hexen> newLineDefs = new();
            List<MapSubSector> newSubSectors = new();
            List<MapSector> newSectors = new();

            List<MapLineDef_Doom> cachedLineDefs = new();
            List<MapSideDef> cachedSideDefs = new();
            List<MapSeg> cachedSegs = new();
            List<MapNode> cachedNodes = new();

            // THINGS
            Lump THINGS = map.Lumps[0];

            for (int i = 0; i < THINGS.Size;)
            {
                MapThing_Hexen newThing = new();

                newThing.X = BitConverter.ToInt16(new[] { file[THINGS.Offset + i++], file[THINGS.Offset + i++] });
                newThing.Y = BitConverter.ToInt16(new[] { file[THINGS.Offset + i++], file[THINGS.Offset + i++] });
                newThing.Angle = BitConverter.ToUInt16(new[] { file[THINGS.Offset + i++], file[THINGS.Offset + i++] });
                newThing.Type = BitConverter.ToUInt16(new[] { file[THINGS.Offset + i++], file[THINGS.Offset + i++] });
                short options = BitConverter.ToInt16(new[] { file[THINGS.Offset + i++], file[THINGS.Offset + i++] });

                newThing.Flags = (ushort)((options & 0xF) | 0x7E0);

                if ((options & BTF_NOTSINGLE) > 0)
                {
                    ushort newFlags = (ushort)(newThing.Flags & ~MTF_SINGLE);

                    newThing.Flags = newFlags;
                }

                if ((options & BTF_NOTDEATHMATCH) > 0)
                {
                    ushort newFlags = (ushort)(newThing.Flags & ~MTF_DEATHMATCH);

                    newThing.Flags = newFlags;
                }

                if ((options & BTF_NOTCOOPERATIVE) > 0)
                {
                    ushort newFlags = (ushort)(newThing.Flags & ~MTF_COOPERATIVE);

                    newThing.Flags = newFlags;
                }

                newThings.Add(newThing);
            }

            // LINEDEFS
            Lump LINEDEFS = map.Lumps[1];

            for (int i = 0, pos = 0; i < LINEDEFS.Size; pos++)
            {
                MapLineDef_Doom doomLineDef = new()
                {
                    Vertex1 = BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }),
                    Vertex2 = BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }),
                    Flags = BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }),
                    Special = BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }),
                    Tag = BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }),
                    SideNum = new[] { BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }), BitConverter.ToUInt16(new[] { file[LINEDEFS.Offset + i++], file[LINEDEFS.Offset + i++] }) }
                };

                MapLineDef_Hexen hexenLineDef = new();
                if (TranslateLineDef(ref hexenLineDef, doomLineDef))
                {
                    Console.WriteLine($"Linedef {pos} referenced sector tag {doomLineDef.Tag}, but there were no free scripts.");
                    SomeProblems = true;
                }

                cachedLineDefs.Add(doomLineDef);
                newLineDefs.Add(hexenLineDef);
            }

            // SIDEDEFS
            Lump SIDEDEFS = map.Lumps[2];

            for (int i = 0; i < SIDEDEFS.Size;)
            {
                cachedSideDefs.Add(new MapSideDef
                {
                    OffsetX = BitConverter.ToInt16(new[] { file[SIDEDEFS.Offset + i++], file[SIDEDEFS.Offset + i++] }),
                    OffsetY = BitConverter.ToInt16(new[] { file[SIDEDEFS.Offset + i++], file[SIDEDEFS.Offset + i++] }),
                    TopTexture = new[] { (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++] },
                    BottomTexture = new[] { (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++] },
                    MidTexture = new[] { (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++], (char)file[SIDEDEFS.Offset + i++] },
                    Sector = BitConverter.ToInt16(new[] { file[SIDEDEFS.Offset + i++], file[SIDEDEFS.Offset + i++] })
                });
            }

            // VERTEXES
            Lump VERTEXES = map.Lumps[3];

            // SEGS
            Lump SEGS = map.Lumps[4];

            for (int i = 0, pos = 0; i < SEGS.Size; pos++)
            {
                cachedSegs.Add(new MapSeg
                {
                    Vertex1 = BitConverter.ToInt16(new[] { file[SEGS.Offset + i++], file[SEGS.Offset + i++] }),
                    Vertex2 = BitConverter.ToInt16(new[] { file[SEGS.Offset + i++], file[SEGS.Offset + i++] }),
                    Angle = BitConverter.ToInt16(new[] { file[SEGS.Offset + i++], file[SEGS.Offset + i++] }),
                    LineDef = BitConverter.ToInt16(new[] { file[SEGS.Offset + i++], file[SEGS.Offset + i++] }),
                    Side = BitConverter.ToInt16(new[] { file[SEGS.Offset + i++], file[SEGS.Offset + i++] }),
                    Offset = BitConverter.ToInt16(new[] { file[SEGS.Offset + i++], file[SEGS.Offset + i++] })
                });
            }

            // SSECTORS
            Lump SSECTORS = map.Lumps[5];

            for (int i = 0; i < SSECTORS.Size;)
            {
                MapSubSector subSector = new()
                {
                    NumSegs = BitConverter.ToInt16(new[] { file[SSECTORS.Offset + i++], file[SSECTORS.Offset + i++] }),
                    FirstSeg = BitConverter.ToUInt16(new[] { file[SSECTORS.Offset + i++], file[SSECTORS.Offset + i++] }),
                };

                MapSeg seg = cachedSegs[subSector.FirstSeg];
                MapLineDef_Hexen line = newLineDefs[seg.LineDef];
                MapSideDef side = cachedSideDefs[line.SideNum[seg.Side]];

                subSector.NumSegs = side.Sector;

                newSubSectors.Add(subSector);
            }

            // NODES
            Lump NODES = map.Lumps[6];
            int nodeCount = NODES.Size / 28; // 28 = Size of Node in bytes

            for (int i = 0; i < NODES.Size;)
            {
                cachedNodes.Add(new MapNode
                {
                    X = BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                    Y = BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                    DX = BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                    DY = BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),

                    BoundingBox = new short[2, 4] {
                        {
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] })
                        },
                        {
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }),
                            BitConverter.ToInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] })
                        }
                    },

                    Children = new[] { BitConverter.ToUInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }), BitConverter.ToUInt16(new[] { file[NODES.Offset + i++], file[NODES.Offset + i++] }) }
                });
            }

            // SECTORS
            Lump SECTORS = map.Lumps[7];

            for (int i = 0; i < SECTORS.Size;)
            {
                MapSector sector = new()
                {
                    FloorHeight = BitConverter.ToInt16(new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] }),
                    CeilingHeight = BitConverter.ToInt16(new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] }),
                    FloorPic = new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] },
                    CeilingPic = new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] },
                    LightLevel = BitConverter.ToInt16(new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] }),
                    Special = BitConverter.ToInt16(new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] }),
                    Tag = BitConverter.ToUInt16(new[] { file[SECTORS.Offset + i++], file[SECTORS.Offset + i++] })
                };

                if (SectorTags[sector.Tag] > 0)
                {
                    sector.Tag = SectorTags[sector.Tag];
                }
                else if ((RefCount[sector.Tag / 8] & (1 << (sector.Tag & 7))) == 0 && sector.Tag > 0 && sector.Tag != 666 && sector.Tag != 667)
                {
                    Console.WriteLine($"Sector tag {sector.Tag} is unreferenced");
                    RefCount[sector.Tag / 8] = (byte)(RefCount[sector.Tag / 8] | (1 << (sector.Tag & 7)));
                }

                if (sector.Special > 0)
                {
                    sector.Special = (short)((sector.Special == 9) ? SECRET_MASK : ((sector.Special & 0xfe0) << 3) | ((sector.Special & 0x01f) + (((sector.Special & 0x1f) < 21) ? 64 : -20)));
                }

                newSectors.Add(sector);
            }

            // REJECT
            Lump REJECT = map.Lumps[8];

            // BLOCKMAP
            Lump BLOCKMAP = map.Lumps[9];

            // -- Extra Stuff --
            // TranslateTeleportThings():
            for (int i = 0; i < newThings.Count; i++)
            {
                MapThing_Hexen thing = newThings[i];

                if (thing.Type == 14) // 14 = T_DESTINATION
                {
                    // PointInSector(x, y):
                    short x = thing.X;
                    short y = thing.Y;

                    MapNode node;
                    int side;
                    int nodenum;

                    // single subsector is a special case
                    if (nodeCount == 0)
                    {
                        thing.ID = newSectors[newSubSectors[0].NumSegs].Tag;
                    }
                    else
                    {
                        nodenum = nodeCount - 1;

                        while ((nodenum & 0x8000) == 0) // 0x8000 = NF_SUBSECTOR
                        {
                            node = cachedNodes[nodenum];
                            side = PointOnSide(x, y, node);
                            nodenum = node.Children[side];
                        }

                        thing.ID = newSectors[newSubSectors[nodenum & ~0x8000].NumSegs].Tag;
                    }

                    newThings[i] = thing;
                }
            }

            if (UsedLineIDs)
            {
                // Make sure that everything that could be referenced by a
                // line id is. We also need an open script to set the lines
                // up if we can't just give them a special that sets their id.

                Console.WriteLine("Setting LineIDs");
                for (int i = 0; i < cachedLineDefs.Count; i++)
                {
                    MapLineDef_Doom line = cachedLineDefs[i];
                    MapLineDef_Hexen to = newLineDefs[i];

                    if (LineIDMaps[line.Tag] > 0)
                    {
                        if (to.Special != (byte)LineSpecial.Teleport_Line &&
                            to.Special != (byte)LineSpecial.TranslucentLine &&
                            to.Special != (byte)LineSpecial.Scroll_Texture_Model)
                        {
                            if (line.Special == 0)
                            {
                                // Set this line to Line_SetIdentification
                                to.Special = (byte)LineSpecial.Line_SetIdentification;
                                to.Args[0] = LineIDMaps[line.Tag];
                            }
                            else
                            {
                                // This line does something special! Gak!
                                Console.WriteLine($"FIX THIS: Linedef {i} (might) need id {LineIDMaps[line.Tag]} but already has a special.");
                                SomeProblems = true;
                            }
                        }
                    }
                }
            }

            // -- Compile map --
            byte[] header = new byte[12] { (byte)'P', (byte)'W', (byte)'A', (byte)'D', 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            List<byte> data = new();
            List<byte> directory = new();

            int currentOffset = 12;
            int totalLumps = 0;

            List<byte> temp = new();
            // Map marker
            totalLumps++;
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(0));
            directory.AddRange("MAP01".ToByteArray());
            directory.AddRange(new byte[] { 0x00, 0x00, 0x00 });

            // THINGS
            for (int i = 0; i < newThings.Count; i++)
            {
                temp.AddRange(BitConverter.GetBytes(newThings[i].ID));
                temp.AddRange(BitConverter.GetBytes(newThings[i].X));
                temp.AddRange(BitConverter.GetBytes(newThings[i].Y));
                temp.AddRange(BitConverter.GetBytes(newThings[i].Z));
                temp.AddRange(BitConverter.GetBytes(newThings[i].Angle));
                temp.AddRange(BitConverter.GetBytes(newThings[i].Type));
                temp.AddRange(BitConverter.GetBytes(newThings[i].Flags));
                temp.Add(newThings[i].Special);
                temp.AddRange(newThings[i].Args);
            }
            data.AddRange(temp);

            totalLumps++;
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(temp.Count));
            directory.AddRange("THINGS".ToByteArray());
            directory.AddRange(new byte[] { 0x00, 0x00 });

            currentOffset += temp.Count;
            temp.Clear();

            // LINEDEFS
            for (int i = 0; i < newLineDefs.Count; i++)
            {
                temp.AddRange(BitConverter.GetBytes(newLineDefs[i].Vertex1));
                temp.AddRange(BitConverter.GetBytes(newLineDefs[i].Vertex2));
                temp.AddRange(BitConverter.GetBytes(newLineDefs[i].Flags));
                temp.Add(newLineDefs[i].Special);
                temp.AddRange(newLineDefs[i].Args);
                temp.AddRange(BitConverter.GetBytes(newLineDefs[i].SideNum[0]));
                temp.AddRange(BitConverter.GetBytes(newLineDefs[i].SideNum[1]));
            }
            data.AddRange(temp);

            totalLumps++;
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(temp.Count));
            directory.AddRange("LINEDEFS".ToByteArray());

            currentOffset += temp.Count;
            temp.Clear();

            // SIDEDEFS
            totalLumps++;
            data.AddRange(file[SIDEDEFS.Offset..(SIDEDEFS.Offset + SIDEDEFS.Size)]);
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(SIDEDEFS.Size));
            directory.AddRange("SIDEDEFS".ToByteArray());
            currentOffset += SIDEDEFS.Size;

            // VERTEXES
            totalLumps++;
            data.AddRange(file[VERTEXES.Offset..(VERTEXES.Offset + VERTEXES.Size)]);
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(VERTEXES.Size));
            directory.AddRange("VERTEXES".ToByteArray());
            currentOffset += VERTEXES.Size;

            // SEGS
            totalLumps++;
            data.AddRange(file[SEGS.Offset..(SEGS.Offset + SEGS.Size)]);
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(SEGS.Size));
            directory.AddRange("SEGS".ToByteArray());
            directory.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            currentOffset += SEGS.Size;

            // SSECTORS
            totalLumps++;
            for (int i = 0; i < newSubSectors.Count; i++)
            {
                temp.AddRange(BitConverter.GetBytes(newSubSectors[i].NumSegs));
                temp.AddRange(BitConverter.GetBytes(newSubSectors[i].FirstSeg));
            }
            data.AddRange(temp);

            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(temp.Count));
            directory.AddRange("SSECTORS".ToByteArray());

            currentOffset += temp.Count;
            temp.Clear();

            // NODES
            totalLumps++;
            data.AddRange(file[NODES.Offset..(NODES.Offset + NODES.Size)]);
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(NODES.Size));
            directory.AddRange("NODES".ToByteArray());
            directory.AddRange(new byte[] { 0x00, 0x00, 0x00 });
            currentOffset += NODES.Size;

            // SECTORS
            totalLumps++;
            for (int i = 0; i < newSectors.Count; i++)
            {
                temp.AddRange(BitConverter.GetBytes(newSectors[i].FloorHeight));
                temp.AddRange(BitConverter.GetBytes(newSectors[i].CeilingHeight));
                temp.AddRange(newSectors[i].FloorPic);
                temp.AddRange(newSectors[i].CeilingPic);
                temp.AddRange(BitConverter.GetBytes(newSectors[i].LightLevel));
                temp.AddRange(BitConverter.GetBytes(newSectors[i].Special));
                temp.AddRange(BitConverter.GetBytes(newSectors[i].Tag));
            }
            data.AddRange(temp);

            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(temp.Count));
            directory.AddRange("SECTORS".ToByteArray());
            directory.Add(0x00);

            currentOffset += temp.Count;
            temp.Clear();

            // REJECT
            totalLumps++;
            data.AddRange(file[REJECT.Offset..(REJECT.Offset + REJECT.Size)]);
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(REJECT.Size));
            directory.AddRange("REJECT".ToByteArray());
            directory.AddRange(new byte[] { 0x00, 0x00 });
            currentOffset += REJECT.Size;

            // BLOCKMAP
            totalLumps++;
            data.AddRange(file[BLOCKMAP.Offset..(BLOCKMAP.Offset + BLOCKMAP.Size)]);
            directory.AddRange(BitConverter.GetBytes(currentOffset));
            directory.AddRange(BitConverter.GetBytes(BLOCKMAP.Size));
            directory.AddRange("BLOCKMAP".ToByteArray());
            currentOffset += BLOCKMAP.Size;

            totalLumps++;
            if (FlyScript > 0)
            {
                // We generated a behavior lump
                FlyBehavior[0] = 5456705; // This is the header because fuck you
                FlyBehavior[1] = FlySize * 4;
                FlyBehavior[FlySize++] = FlyScript;

                for (uint i = 0; i < FlyScript; i++)
                {
                    FlyBehavior[FlySize++] = i + 1;
                    FlyBehavior[FlySize++] = ScriptStarts[i];
                    FlyBehavior[FlySize++] = 0;
                }

                FlyBehavior[FlySize++] = 0;     // 0 strings


                for (int i = 0; i < FlySize; i++)
                {
                    temp.AddRange(BitConverter.GetBytes(FlyBehavior[i]));
                }
                data.AddRange(temp);

                directory.AddRange(BitConverter.GetBytes(currentOffset));
                directory.AddRange(BitConverter.GetBytes(temp.Count));
                directory.AddRange("BEHAVIOR".ToByteArray());

                currentOffset += temp.Count;
                temp.Clear();

                Console.WriteLine($"{FlyScript} script{(FlyScript != 1 ? "s" : "")} created");
            }
            else
            {
                // The empty behavior lump will suffice
                byte[] NullBehavior = new byte[] { (byte)'A', (byte)'C', (byte)'S', 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                data.AddRange(NullBehavior);
                directory.AddRange(BitConverter.GetBytes(currentOffset));
                directory.AddRange(BitConverter.GetBytes(NullBehavior.Length));
                directory.AddRange("BEHAVIOR".ToByteArray());
                currentOffset += NullBehavior.Length;
            }

            // Fill in header and write data
            byte[] b1 = BitConverter.GetBytes(totalLumps);
            header[4] = b1[0];
            header[5] = b1[1];
            header[6] = b1[2];
            header[7] = b1[3];

            byte[] b2 = BitConverter.GetBytes(data.Count + 12);
            header[8] = b2[0];
            header[9] = b2[1];
            header[10] = b2[2];
            header[11] = b2[3];

            List<byte> fullFile = new();
            fullFile.AddRange(header);
            fullFile.AddRange(data);
            fullFile.AddRange(directory);

            File.WriteAllBytes(@$".\pk3\MAPS\{map.Name}.WAD", fullFile.ToArray());
        }

        static int PointOnSide(short x, short y, MapNode node)
        {
            short dx, dy;
            double left, right;

            if (node.DX == 0)
            {
                if (x <= node.X)
                    return node.DY > 0 ? 1 : 0;

                return node.DY < 0 ? 1 : 0;
            }

            if (node.DY == 0)
            {
                if (y <= node.Y)
                    return node.DX < 0 ? 1 : 0;

                return node.DX > 0 ? 1 : 0;
            }

            dx = (short)(x - node.X);
            dy = (short)(y - node.Y);

            // Try to quickly decide by looking at sign bits.
            if (((node.DY ^ node.DX ^ dx ^ dy) & 0x8000) > 0)
            {
                if (((node.DY ^ dx) & 0x8000) > 0)
                {
                    // (left is negative)
                    return 1;
                }

                return 0;
            }

            left = (node.DY / 65536.0) * dx;
            right = dy * (node.DX / 65536.0);

            return (right < left) ? 0 : 1;
        }

        // Returns true if a 666 or 667 tag was referenced, and there
        // were no free scripts.
        static bool TranslateLineDef(ref MapLineDef_Hexen ld, MapLineDef_Doom mld)
        {
            bool res = false;
            ushort special = mld.Special;
            ushort flags = mld.Flags;
            int i;
            byte nspecial;
            ushort[] args = new ushort[5];
            ushort tag = 0;
            bool usedtag = false;
            int numparms = 0;
            bool passthrough;

            ld.Vertex1 = mld.Vertex1;
            ld.Vertex2 = mld.Vertex2;
            ld.SideNum[0] = mld.SideNum[0];
            ld.SideNum[1] = mld.SideNum[1];

            passthrough = (flags & ML_PASSUSEORG) > 0;

            flags &= 0x01ff; // Ignore flags unknown to DOOM

            if (special <= NUM_SPECIALS)
            {
                // This is a regular special; translate thru LUT
                flags = (ushort)(flags | (SpecialTranslations[special].Flags << 8));

                if (passthrough && (flags & ML_ACTIVATIONMASK) == ML_ACTIVATEUSE)
                {
                    flags = (ushort)(flags & ~ML_ACTIVATIONMASK);
                    flags |= (ushort)ML_ACTIVATEUSETHROUGH;
                }

                nspecial = (byte)SpecialTranslations[special].NewSpecial;
                numparms = SpecialTranslations[special].NumParams;

                for (i = 0; i < 5; i++)
                {
                    if (SpecialTranslations[special].Args[i] == TAG)
                    {
                        usedtag = true;
                        if (mld.Tag != 666 && mld.Tag != 667)
                            tag = MapTag(mld.Tag);
                        else
                            tag = mld.Tag;
                        args[i] = tag;
                    }
                    else if (SpecialTranslations[special].Args[i] == LINETAG)
                    {
                        UsedLineIDs = true;
                        tag = MapLineID(mld.Tag);
                        args[i] = tag;
                    }
                    else
                    {
                        args[i] = SpecialTranslations[special].Args[i];
                    }
                }
            }
            else if (special <= GenCrusherBase)
            {
                if (special >= ZDoomStaticInits && special < ZDoomStaticInits + NUM_STATIC_INITS)
                {
                    // A ZDoom Static_Init special
                    nspecial = (byte)LineSpecial.Static_Init;
                    usedtag = true;
                    if (mld.Tag != 666 && mld.Tag != 667)
                        tag = MapTag(mld.Tag);
                    else
                        tag = mld.Tag;
                    args[0] = tag;
                    args[1] = (ushort)(special - ZDoomStaticInits);
                }
                else
                {
                    Console.WriteLine($"Unknown special: {special}");

                    // This is an unknown special. Just zero it.
                    nspecial = 0;
                    args = new ushort[5] { 0, 0, 0, 0, 0 };
                    flags = (ushort)(flags & ~ML_ACTIVATIONMASK);
                }
            }
            else
            {
                // Anything else is a BOOM generalized linedef type
                if (mld.Tag != 666 && mld.Tag != 667)
                    tag = MapTag(mld.Tag);
                else
                    tag = mld.Tag;

                switch ((TriggerType)(special & 0x0007))
                {
                    case TriggerType.WalkMany:
                        flags |= ML_REPEATABLE;
                        goto case TriggerType.WalkOnce; // [Tri] This fallthrough syntax is stupid, why does this have to exist?
                    case TriggerType.WalkOnce:
                        flags |= ML_ACTIVATECROSS;
                        break;

                    case TriggerType.SwitchMany:
                    case TriggerType.PushMany:
                        flags |= ML_REPEATABLE;
                        goto case TriggerType.SwitchOnce;
                    case TriggerType.SwitchOnce:
                    case TriggerType.PushOnce:
                        if (passthrough)
                            flags |= ML_ACTIVATEUSETHROUGH;
                        else
                            flags |= ML_ACTIVATEUSE;
                        break;

                    case TriggerType.GunMany:
                        flags |= ML_REPEATABLE;
                        goto case TriggerType.GunOnce;
                    case TriggerType.GunOnce:
                        flags |= ML_ACTIVATEPROJECTILEHIT;
                        break;
                }

                // We treat push triggers like switch triggers with zero tags.
                if ((special & 0x0007) is (int)TriggerType.PushMany or (int)TriggerType.PushOnce)
                    args[0] = 0;
                else
                {
                    args[0] = tag;
                    usedtag = true;
                }

                if (special <= GenStairsBase)
                {
                    // Generalized crusher (tag, dnspeed, upspeed, silent, damage)
                    nspecial = (byte)LineSpecial.Generic_Crusher;
                    if ((special & 0x0020) > 0)
                        flags |= ML_MONSTERSCANACTIVATE;
                    switch (special & 0x0018)
                    {
                        case 0x0000: args[1] = (ushort)C_SLOW; break;
                        case 0x0008: args[1] = (ushort)C_NORMAL; break;
                        case 0x0010: args[1] = (ushort)C_FAST; break;
                        case 0x0018: args[1] = (ushort)C_TURBO; break;
                    }
                    args[2] = args[1];
                    args[3] = (ushort)((special & 0x0040) >> 6);
                    args[4] = 10;
                    numparms = 5;
                }
                else if (special <= GenLiftBase)
                {
                    // Generalized stairs (tag, speed, step, dir/igntxt, reset)
                    nspecial = (byte)LineSpecial.Generic_Stairs;
                    if ((special & 0x0020) > 0)
                        flags |= ML_MONSTERSCANACTIVATE;
                    switch (special & 0x0018)
                    {
                        case 0x0000: args[1] = (ushort)S_SLOW; break;
                        case 0x0008: args[1] = (ushort)S_NORMAL; break;
                        case 0x0010: args[1] = (ushort)S_FAST; break;
                        case 0x0018: args[1] = (ushort)S_TURBO; break;
                    }
                    switch (special & 0x00c0)
                    {
                        case 0x0000: args[2] = 4; break;
                        case 0x0040: args[2] = 8; break;
                        case 0x0080: args[2] = 16; break;
                        case 0x00c0: args[2] = 24; break;
                    }
                    args[3] = (ushort)((special & 0x0300) >> 8);
                    args[4] = 0;
                    numparms = 5;
                }
                else if (special <= GenLockedBase)
                {
                    // Generalized lift (tag, speed, delay, target, height)
                    nspecial = (byte)LineSpecial.Generic_Lift;
                    if ((special & 0x0020) > 0)
                        flags |= ML_MONSTERSCANACTIVATE;
                    switch (special & 0x0018)
                    {
                        case 0x0000: args[1] = (ushort)(P_SLOW * 2); break;
                        case 0x0008: args[1] = (ushort)(P_NORMAL * 2); break;
                        case 0x0010: args[1] = (ushort)(P_FAST * 2); break;
                        case 0x0018: args[1] = (ushort)(P_TURBO * 2); break;
                    }
                    switch (special & 0x00c0)
                    {
                        case 0x0000: args[2] = 8; break;
                        case 0x0040: args[2] = 24; break;
                        case 0x0080: args[2] = 40; break;
                        case 0x00c0: args[2] = 80; break;
                    }
                    args[3] = (ushort)(((special & 0x0300) >> 8) + 1);
                    args[4] = 0;
                    numparms = 5;
                }
                else if (special <= GenDoorBase)
                {
                    // Generalized locked door (tag, speed, kind, delay, lock)
                    nspecial = (byte)LineSpecial.Generic_Door;
                    if ((special & 0x0080) > 0)
                        flags |= ML_MONSTERSCANACTIVATE;
                    switch (special & 0x0018)
                    {
                        case 0x0000: args[1] = (ushort)D_SLOW; break;
                        case 0x0008: args[1] = (ushort)D_NORMAL; break;
                        case 0x0010: args[1] = (ushort)D_FAST; break;
                        case 0x0018: args[1] = (ushort)D_TURBO; break;
                    }
                    args[2] = (ushort)((special & 0x0020) >> 5);
                    args[3] = 0;
                    args[4] = (ushort)((special & 0x01c0) >> 6);
                    if (args[4] == 0)
                        args[4] = (ushort)KeyType.AnyKey;
                    else if (args[4] == 7)
                        args[4] = (ushort)KeyType.AllKeys;
                    args[4] = (ushort)(args[4] | ((special & 0x0200) >> 2));
                    numparms = 5;
                }
                else if (special <= GenCeilingBase)
                {
                    // Generalized door (tag, speed, kind, delay, lock)
                    nspecial = (byte)LineSpecial.Generic_Door;
                    switch (special & 0x0018)
                    {
                        case 0x0000: args[1] = (ushort)D_SLOW; break;
                        case 0x0008: args[1] = (ushort)D_NORMAL; break;
                        case 0x0010: args[1] = (ushort)D_FAST; break;
                        case 0x0018: args[1] = (ushort)D_TURBO; break;
                    }
                    args[2] = (ushort)((special & 0x0060) >> 5);
                    switch (special & 0x0300)
                    {
                        case 0x0000: args[3] = 8; break;
                        case 0x0100: args[3] = 32; break;
                        case 0x0200: args[3] = 72; break;
                        case 0x0300: args[3] = 240; break;
                    }
                    args[4] = 0;
                    numparms = 5;
                }
                else
                {
                    // Generalized ceiling (tag, speed, height, target, change/model/direct/crush)
                    // Generalized floor (tag, speed, height, target, change/model/direct/crush)
                    if (special <= GenFloorBase)
                        nspecial = (byte)LineSpecial.Generic_Ceiling;
                    else
                        nspecial = (byte)LineSpecial.Generic_Floor;

                    switch (special & 0x0018)
                    {
                        case 0x0000: args[1] = (ushort)F_SLOW; break;
                        case 0x0008: args[1] = (ushort)F_NORMAL; break;
                        case 0x0010: args[1] = (ushort)F_FAST; break;
                        case 0x0018: args[1] = (ushort)F_TURBO; break;
                    }
                    args[3] = (ushort)(((special & 0x0380) >> 7) + 1);
                    if (args[3] >= 7)
                    {
                        args[2] = (ushort)(24 + (args[3] - 7) * 8);
                        args[3] = 0;
                    }
                    else
                    {
                        args[2] = 0;
                    }
                    args[4] = (ushort)(((special & 0x0c00) >> 10) | ((special & 0x0060) >> 3) | ((special & 0x1000) >> 8));
                    numparms = 5;
                }
            }

            ld.Flags = flags;

            if (usedtag && (tag == 666 || tag == 667) && numparms > 0)
            {
                // Sector tags 666 and 667 are special cases:
                // We need to preserve them, so we construct a script for them.
                // If the special takes no parameters, though, we don't need to
                // bother, since the tag isn't actually used by it in that case.
                if (FlyScript >= 255)
                {
                    res = true;
                }
                else
                {
                    ScriptStarts[FlyScript++] = FlySize * 4;
                    FlyBehavior[FlySize++] = (uint)(PCD_LSPEC1DIRECT + numparms - 1);
                    FlyBehavior[FlySize++] = nspecial;
                    for (i = 0; i < numparms; i++)
                        FlyBehavior[FlySize++] = args[i];
                    FlyBehavior[FlySize++] = PCD_TERMINATE;

                    ld.Special = (byte)LineSpecial.ACS_ExecuteAlways;
                    ld.Args[0] = (byte)FlyScript;
                    ld.Args[1] = ld.Args[2] = ld.Args[3] = ld.Args[4] = 0;

                    return false;
                }
            }

            ld.Special = nspecial;
            for (i = 0; i < 5; i++)
                ld.Args[i] = (byte)args[i];
            return res;
        }

        static byte MapLineID(ushort id)
        {
            if (id == 0)
            {
                return 0;
            }
            else if (LineIDMaps[id] == 0)
            {
                // Try to find a good lineid for this one (preferably the same)
                int newid = FreeLineID(id);

                LineIDMaps[id] = (byte)newid;
                ReverseLineIDMaps[newid] = id;
            }

            return LineIDMaps[id];
        }

        static byte FreeLineID(ushort id)
        {
            if (id < 256 && ReverseLineIDMaps[id] == 0)
            {
                return (byte)id;
            }
            else
            {
                int i;

                for (i = 0; i < 255; i++)
                {
                    if (ReverseLineIDMaps[i] == 0)
                    {
                        Console.WriteLine($"LineID {id} remapped to {i}");
                        return (byte)i;
                    }
                }
            }

            Console.WriteLine($"FIX THIS: Could not remap lineid {id} (using 0)!");
            SomeProblems = true;
            return 0;
        }

        static byte MapTag(ushort tag)
        {
            if (tag == 0)
            {
                return 0;
            }
            else if (SectorTags[tag] == 0)
            {
                // Try to find a good tag for this one (preferably the same)
                int newtag = FreeTag(tag);

                SectorTags[tag] = (byte)newtag;
                UsedTags[newtag / 8] = (byte)(UsedTags[newtag / 8] | 1 << (newtag & 7));
            }

            return SectorTags[tag];
        }

        static ushort FreeTag(ushort tag)
        {
            if (tag < 256 && ((UsedTags[tag / 8] & (1 << (tag & 7))) == 0))
            {
                return tag;
            }
            else
            {
                int i;

                if (tag == 666 || tag == 667)
                    Console.Write("WARNING! ");

                for (i = 128; i < 256; i++)
                {
                    if ((UsedTags[i / 8] & (1 << (i & 7))) == 0)
                    {
                        Console.Write($"Tag {tag} remapped to {i}\n");
                        return (ushort)i;
                    }
                }
                for (i = 127; i >= 0; i--)
                {
                    if ((UsedTags[i / 8] & (1 << (i & 7))) == 0)
                    {
                        Console.Write($"Tag {tag} remapped to {i}\n");
                        return (ushort)i;
                    }
                }
            }
            Console.Write($"FIX THIS: Could not remap tag {tag} (using 0)!\n");
            SomeProblems = true;
            return 0;
        }

    }
}
