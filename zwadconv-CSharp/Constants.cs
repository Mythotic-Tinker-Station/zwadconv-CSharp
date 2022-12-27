using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zwadconv_CSharp
{
    class Constants
    {
        public static ushort BTF_NOTSINGLE = 0x0010; // (TF_COOPERATIVE|TF_DEATHMATCH)
        public static ushort BTF_NOTDEATHMATCH = 0x0020; // (TF_SINGLE|TF_COOPERATIVE)
        public static ushort BTF_NOTCOOPERATIVE = 0x0040; // (TF_SINGLE|TF_DEATHMATCH)

        public static ushort MTF_SINGLE = 0x0100;
        public static ushort MTF_COOPERATIVE = 0x0200;
        public static ushort MTF_DEATHMATCH = 0x0400;

        // -- LineDef attributes.
        public static ushort ML_BLOCKING = 0x0001;
        public static ushort ML_BLOCKMONSTERS = 0x0002;
        public static ushort ML_TWOSIDED = 0x0004;
        public static ushort ML_DONTPEGTOP = 0x0008;
        public static ushort ML_DONTPEGBOTTOM = 0x0010;
        public static ushort ML_SECRET = 0x0020;
        public static ushort ML_SOUNDBLOCK = 0x0040;
        public static ushort ML_DONTDRAW = 0x0080;
        public static ushort ML_MAPPED = 0x0100;
        public static ushort ML_REPEATABLE = 0x0200;

        public static ushort ML_ACTIVATIONMASK = 0x1c00;
        public static ushort ML_ACTIVATECROSS = 0x0000;
        public static ushort ML_ACTIVATEUSE = 0x0400;
        public static ushort ML_ACTIVATEMONSTERCROSS = 0x0800;
        public static ushort ML_ACTIVATEPROJECTILEHIT = 0x0c00;
        public static ushort ML_ACTIVATEPUSH = 0x1000;
        public static ushort ML_ACTIVATEPROJECTILECROSS = 0x1400;
        public static ushort ML_ACTIVATEUSETHROUGH = 0x1800;

        public static ushort ML_MONSTERSCANACTIVATE = 0x2000;
        public static ushort ML_PASSUSEORG = 0x0200;
        // --

        public static ushort DAMAGE_MASK = 0x0300;
        public static ushort SECRET_MASK = 0x0400;
        public static ushort FRICTION_MASK = 0x0800;
        public static ushort PUSH_MASK = 0x1000;

        public static uint PCD_TERMINATE = 1;
        public static uint PCD_LSPEC1DIRECT = 9;
        public static uint PCD_LSPEC2DIRECT = 10;
        public static uint PCD_LSPEC3DIRECT = 11;
        public static uint PCD_LSPEC4DIRECT = 12;
        public static uint PCD_LSPEC5DIRECT = 13;
    }
}
