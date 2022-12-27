namespace zwadconv_CSharp
{
    public class MapConverterTypes
    {
        // Assume these types are Hexen, unless it's specified.
        class MapThing_Doom
        {
            public short X; // 0 1
            public short Y; // 2 3
            public short Angle; // 4 5
            public short Type; // 6 7
            public short Options; // 8 9
        }

        public class MapThing_Hexen
        {
            public ushort ID;
            public short X;
            public short Y;
            public short Z;
            public ushort Angle;
            public ushort Type;
            public ushort Flags;
            public byte Special;
            public byte[] Args = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        public class MapLineDef_Doom
        {
            public ushort Vertex1;
            public ushort Vertex2;
            public ushort Flags;
            public ushort Special;
            public ushort Tag;
            public ushort[] SideNum = new ushort[2] { 0x00, 0x00 };
        }

        public class MapLineDef_Hexen
        {
            public ushort Vertex1;
            public ushort Vertex2;
            public ushort Flags;
            public byte Special;
            public byte[] Args = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };
            public ushort[] SideNum = new ushort[2] { 0x00, 0x00 }; // Front sidedef, Back sidedef
        }

        public class MapSideDef
        {
            public short OffsetX;
            public short OffsetY;
            public char[] TopTexture = new char[8] { (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00 };
            public char[] BottomTexture = new char[8] { (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00 };
            public char[] MidTexture = new char[8] { (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00, (char)0x00 };
            // Front sector, towards viewer.
            public short Sector;
        }

        public class MapVertex
        {
            public short X;
            public short Y;

            public short NewRef; // From Tribeam's extractor
        }

        public class MapSeg
        {
            public short Vertex1;
            public short Vertex2;
            public short Angle;
            public short LineDef;
            public short Side;
            public short Offset;
        }

        public class MapSubSector
        {
            public short NumSegs;
            // Index of first one, segs are stored sequentially.
            public ushort FirstSeg;
        }

        public class MapNode
        {
            // Partition line from (x, y) to (x+dx, y+dy)
            public short X;
            public short Y;
            public short DX;
            public short DY;

            // shorting box for each child,
            // clip against view frustum.
            public short[,] BoundingBox = new short[2, 4] { { 0x00, 0x00, 0x00, 0x00 }, { 0x00, 0x00, 0x00, 0x00 } };

            // If NF_SUBSECTOR its a subsector,
            // else it's a node of another subtree.
            public ushort[] Children = new ushort[2] { 0x00, 0x00 };
        }

        public class MapSector
        {
            public short FloorHeight;
            public short CeilingHeight;
            public byte[] FloorPic = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            public byte[] CeilingPic = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            public short LightLevel;
            public short Special;
            public ushort Tag;
        }
    }
}