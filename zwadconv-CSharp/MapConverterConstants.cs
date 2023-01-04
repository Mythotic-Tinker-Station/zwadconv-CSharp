using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static zwadconv_CSharp.Constants;

namespace zwadconv_CSharp
{
    class MapConverterConstants
    {
        // xlat.h: Translate old linedefs to new linedefs
        //		   Pulled from the ZDoom source itself

        public enum KeyType
        {
            NoKey,
            RCard,
            BCard,
            YCard,
            RSkull,
            BSkull,
            YSkull,

            AnyKey = 100,
            AllKeys = 101,

            CardIsSkull = 128
        };

        // Speeds for ceilings/crushers (x/8 units per tic)
        // (Hexen crushers go up at half the speed that they go down)
        public static short C_SLOW = 8;
        public static short C_NORMAL = 16;
        public static short C_FAST = 32;
        public static short C_TURBO = 64;

        public static short CEILWAIT = 150;

        // Speeds for floors (x/8 units per tic)
        public static short F_SLOW = 8;
        public static short F_NORMAL = 16;
        public static short F_FAST = 32;
        public static short F_TURBO = 64;

        // Speeds for doors (x/8 units per tic)
        public static short D_SLOW = 16;
        public static short D_NORMAL = 32;
        public static short D_FAST = 64;
        public static short D_TURBO = 128;

        public static short VDOORWAIT = 150;

        // Speeds for stairs (x/8 units per tic)
        public static short S_SLOW = 2;
        public static short S_NORMAL = 4;
        public static short S_FAST = 16;
        public static short S_TURBO = 32;

        // Speeds for plats (Hexen plats stop 8 units above the floor)
        public static short P_SLOW = 8;
        public static short P_NORMAL = 16;
        public static short P_FAST = 32;
        public static short P_TURBO = 64;

        public static short PLATWAIT = 105;

        public static short ELEVATORSPEED = 32;

        // Speeds for donut slime and pillar (x/8 units per tic)
        public static short DORATE = 4;

        // Texture scrollers operate at a rate of x/64 units per tic.
        public static short SCROLL_UNIT = 64;

        // Define masks, shifts, for fields in generalized linedef types
        // (from BOOM's p_psec.h file)

        public static short GenFloorBase = 0x6000;
        public static short GenCeilingBase = 0x4000;
        public static short GenDoorBase = 0x3c00;
        public static short GenLockedBase = 0x3800;
        public static short GenLiftBase = 0x3400;
        public static short GenStairsBase = 0x3000;
        public static short GenCrusherBase = 0x2F80;

        public static short ZDoomStaticInits = 333;
        public static short NUM_STATIC_INITS = 3;

        // Define names for the TriggerType field of the general linedefs

        public enum TriggerType
        {
            WalkOnce,
            WalkMany,
            SwitchOnce,
            SwitchMany,
            GunOnce,
            GunMany,
            PushOnce,
            PushMany
        };


        public static byte TAG = 123;   // Special value that gets replaced with the line tag
        public static byte LINETAG = 124; // Special value that gets replaced with the line id (tag)

        public static byte WALK = (byte)(ML_ACTIVATECROSS >> 8);
        public static byte USE = (byte)(ML_ACTIVATEUSE >> 8);
        public static byte SHOOT = (byte)(ML_ACTIVATEPROJECTILEHIT >> 8);
        public static byte MONST = (byte)(ML_MONSTERSCANACTIVATE >> 8);
        public static byte MONWALK = (byte)(ML_ACTIVATEMONSTERCROSS >> 8);
        public static byte REP = (byte)(ML_REPEATABLE >> 8);

        // Line special translation structure

        public class SpecialTranslation
        {
            public byte Flags;
            public LineSpecial NewSpecial;
            public byte NumParams;
            public byte[] Args = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };

            public static implicit operator SpecialTranslation((int Flags, LineSpecial Special, int NumParams) inParams)
			{
				return new SpecialTranslation
				{
					Flags      = (byte)inParams.Flags,
					NewSpecial = inParams.Special,
					NumParams  = (byte)inParams.NumParams
				};
			}

			public static implicit operator SpecialTranslation((int Flags, LineSpecial Special, int NumParams, int[] Args) inParams)
            {
                SpecialTranslation xlat = new SpecialTranslation { Flags = (byte)inParams.Flags, NewSpecial = inParams.Special, NumParams = (byte)inParams.NumParams };
                for (int i = 0; i < inParams.Args.Length; i++)
                {
                    xlat.Args[i] = (byte)inParams.Args[i];
                }

                return xlat;
            }

            public static implicit operator SpecialTranslation((int Flags, LineSpecial Special, int NumParams, short[] Args) inParams)
            {
                SpecialTranslation xlat = new SpecialTranslation { Flags = (byte)inParams.Flags, NewSpecial = inParams.Special, NumParams = (byte)inParams.NumParams };
                for (int i = 0; i < inParams.Args.Length; i++)
                {
                    xlat.Args[i] = (byte)inParams.Args[i];
                }

                return xlat;
            }

            public static implicit operator SpecialTranslation((int Flags, LineSpecial Special, int NumParams, byte[] Args) inParams)
            {
                SpecialTranslation xlat = new SpecialTranslation { Flags = (byte)inParams.Flags, NewSpecial = inParams.Special, NumParams = (byte)inParams.NumParams };
                for (int i = 0; i < inParams.Args.Length; i++)
                {
                    xlat.Args[i] = inParams.Args[i];
                }

                return xlat;
            }
        }

        public static SpecialTranslation[] SpecialTranslations = {
/*   0 */ ( 0,  0,  0,  new[] { TAG } ),
/*   1 */ ( USE|MONST|REP,  LineSpecial.Door_Raise,                  3, new[]{ 0, D_SLOW, VDOORWAIT } ),
/*   2 */ ( WALK,           LineSpecial.Door_Open,                   2, new[]{ TAG, D_SLOW } ),
/*   3 */ ( WALK,           LineSpecial.Door_Close,                  2, new[]{ TAG, D_SLOW } ),
/*   4 */ ( WALK|MONST,     LineSpecial.Door_Raise,                  3, new[]{ TAG, D_SLOW, VDOORWAIT } ),
/*   5 */ ( WALK,           LineSpecial.Floor_RaiseToLowestCeiling,  2, new[]{ TAG, F_SLOW } ),
/*	 6 */ ( WALK,           LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_NORMAL, C_NORMAL, 10 } ),
/*   7 */ ( USE,            LineSpecial.Stairs_BuildUpDoom,          3, new[]{ TAG, S_SLOW, 8 } ),
/*   8 */ ( WALK,           LineSpecial.Stairs_BuildUpDoom,          3, new[]{ TAG, S_SLOW, 8 } ),
/*	 9 */ ( USE,            LineSpecial.Floor_Donut,                 3, new[]{ TAG, DORATE, DORATE } ),
/*  10 */ ( WALK|MONST,     LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_FAST, PLATWAIT, 0 } ),
/*  11 */ ( USE,            LineSpecial.Exit_Normal,                 1, new[]{ 0 } ),
/*  12 */ ( WALK,           LineSpecial.Light_MaxNeighbor,           1, new[]{ TAG } ),
/*  13 */ ( WALK,           LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 255 } ),
/*  14 */ ( USE,            LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 4 } ),
/*  15 */ ( USE,            LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 3 } ),
/*  16 */ ( WALK,           LineSpecial.Door_CloseWaitOpen,          3, new[]{ TAG, D_SLOW, 240 } ),
/*  17 */ ( WALK,           LineSpecial.Light_StrobeDoom,            3, new[]{ TAG, 5, 35 } ),
/*  18 */ ( USE,            LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_SLOW } ),
/*  19 */ ( WALK,           LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_SLOW, 128 } ),
/*  20 */ ( USE,            LineSpecial.Plat_RaiseAndStayTx0,        2, new[]{ TAG, P_SLOW/2 } ),
/*  21 */ ( USE,            LineSpecial.Plat_DownWaitUpStayLip,      3, new[]{ TAG, P_FAST, PLATWAIT } ),
/*  22 */ ( WALK,           LineSpecial.Plat_RaiseAndStayTx0,        2, new[]{ TAG, P_SLOW/2 } ),
/*  23 */ ( USE,            LineSpecial.Floor_LowerToLowest,         2, new[]{ TAG, F_SLOW } ),
/*  24 */ ( SHOOT,          LineSpecial.Floor_RaiseToLowestCeiling,  2, new[]{ TAG, F_SLOW } ),
/*  25 */ ( WALK,           LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/*  26 */ ( USE|REP,        LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_SLOW, VDOORWAIT, (byte)(KeyType.BCard | KeyType.CardIsSkull) } ),
/*  27 */ ( USE|REP,        LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_SLOW, VDOORWAIT, (byte)(KeyType.YCard | KeyType.CardIsSkull) } ),
/*  28 */ ( USE|REP,        LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_SLOW, VDOORWAIT, (byte)(KeyType.RCard | KeyType.CardIsSkull) } ),
/*  29 */ ( USE,            LineSpecial.Door_Raise,                  3, new[]{ TAG, D_SLOW, VDOORWAIT } ),
/*  30 */ ( WALK,           LineSpecial.Floor_RaiseByTexture,        2, new[]{ TAG, F_SLOW } ),
/*  31 */ ( USE,            LineSpecial.Door_Open,                   2, new[]{ 0, D_SLOW } ),
/*  32 */ ( USE|MONST,      LineSpecial.Door_LockedRaise,            4, new[]{ 0, D_SLOW, 0, (byte)(KeyType.BCard | KeyType.CardIsSkull) } ),
/*  33 */ ( USE|MONST,      LineSpecial.Door_LockedRaise,            4, new[]{ 0, D_SLOW, 0, (byte)(KeyType.RCard | KeyType.CardIsSkull) } ),
/*  34 */ ( USE|MONST,      LineSpecial.Door_LockedRaise,            4, new[]{ 0, D_SLOW, 0, (byte)(KeyType.YCard | KeyType.CardIsSkull) } ),
/*  35 */ ( WALK,           LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 35 } ),
/*  36 */ ( WALK,           LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_FAST, 136 } ),
/*  37 */ ( WALK,           LineSpecial.Floor_LowerToLowestTxTy,     2, new[]{ TAG, F_SLOW } ),
/*  38 */ ( WALK,           LineSpecial.Floor_LowerToLowest,         2, new[]{ TAG, F_SLOW } ),
/*  39 */ ( WALK|MONST,     LineSpecial.Teleport,                    1, new[]{ TAG } ),
/*  40 */ ( WALK,           LineSpecial.Generic_Ceiling,             5, new[]{ TAG, C_SLOW, 0, 1, 8 } ),
/*  41 */ ( USE,            LineSpecial.Ceiling_LowerToFloor,        2, new[]{ TAG, C_SLOW } ),
/*  42 */ ( USE|REP,        LineSpecial.Door_Close,                  2, new[]{ TAG, D_SLOW } ),
/*  43 */ ( USE|REP,        LineSpecial.Ceiling_LowerToFloor,        2, new[]{ TAG, C_SLOW } ),
/*  44 */ ( WALK,           LineSpecial.Ceiling_LowerAndCrush,       3, new[]{ TAG, C_SLOW, 0 } ),
/*  45 */ ( USE|REP,        LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_SLOW, 128 } ),
/*  46 */ ( SHOOT|REP|MONST,LineSpecial.Door_Open,                   2, new[]{ TAG, D_SLOW } ),
/*  47 */ ( SHOOT,          LineSpecial.Plat_RaiseAndStayTx0,        2, new[]{ TAG, P_SLOW/2 } ),
/*  48 */ ( 0,              LineSpecial.Scroll_Texture_Left,         1, new[]{ SCROLL_UNIT } ),
/*  49 */ ( USE,            LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/*  50 */ ( USE,            LineSpecial.Door_Close,                  2, new[]{ TAG, D_SLOW } ),
/*  51 */ ( USE,            LineSpecial.Exit_Secret,                 1, new[]{ 0 } ),
/*  52 */ ( WALK,           LineSpecial.Exit_Normal,                 1, new[]{ 0 } ),
/*  53 */ ( WALK,           LineSpecial.Plat_PerpetualRaiseLip,      4, new[]{ TAG, P_SLOW, PLATWAIT, 0 } ),
/*  54 */ ( WALK,           LineSpecial.Plat_Stop,                   1, new[]{ TAG } ),
/*  55 */ ( USE,            LineSpecial.Floor_RaiseAndCrush,         3, new[]{ TAG, F_SLOW, 10 } ),
/*  56 */ ( WALK,           LineSpecial.Floor_RaiseAndCrush,         3, new[]{ TAG, F_SLOW, 10 } ),
/*  57 */ ( WALK,           LineSpecial.Ceiling_CrushStop,           1, new[]{ TAG } ),
/*  58 */ ( WALK,           LineSpecial.Floor_RaiseByValue,          3, new[]{ TAG, F_SLOW, 24 } ),
/*  59 */ ( WALK,           LineSpecial.Floor_RaiseByValueTxTy,      3, new[]{ TAG, F_SLOW, 24 } ),
/*  60 */ ( USE|REP,        LineSpecial.Floor_LowerToLowest,         2, new[]{ TAG, F_SLOW } ),
/*  61 */ ( USE|REP,        LineSpecial.Door_Open,                   2, new[]{ TAG, D_SLOW } ),
/*  62 */ ( USE|REP,        LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_FAST, PLATWAIT, 0 } ),
/*  63 */ ( USE|REP,        LineSpecial.Door_Raise,                  3, new[]{ TAG, D_SLOW, VDOORWAIT } ),
/*  64 */ ( USE|REP,        LineSpecial.Floor_RaiseToLowestCeiling,  2, new[]{ TAG, F_SLOW } ),
/*  65 */ ( USE|REP,        LineSpecial.Floor_RaiseAndCrush,         3, new[]{ TAG, F_SLOW, 10 } ),
/*  66 */ ( USE|REP,        LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 3 } ),
/*  67 */ ( USE|REP,        LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 4 } ),
/*  68 */ ( USE|REP,        LineSpecial.Plat_RaiseAndStayTx0,        2, new[]{ TAG, P_SLOW/2 } ),
/*  69 */ ( USE|REP,        LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_SLOW } ),
/*  70 */ ( USE|REP,        LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_FAST, 136 } ),
/*  71 */ ( USE,            LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_FAST, 136 } ),
/*  72 */ ( WALK|REP,       LineSpecial.Ceiling_LowerAndCrush,       3, new[]{ TAG, C_SLOW, 0 } ),
/*  73 */ ( WALK|REP,       LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/*  74 */ ( WALK|REP,       LineSpecial.Ceiling_CrushStop,           1, new[]{ TAG } ),
/*  75 */ ( WALK|REP,       LineSpecial.Door_Close,                  2, new[]{ TAG, D_SLOW } ),
/*  76 */ ( WALK|REP,       LineSpecial.Door_CloseWaitOpen,          3, new[]{ TAG, D_SLOW, 240 } ),
/*  77 */ ( WALK|REP,       LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_NORMAL, C_NORMAL, 10 } ),
/*  78 */ ( USE|REP,        LineSpecial.Floor_TransferNumeric,       1, new[]{ TAG } ),			// <- BOOM special
/*  79 */ ( WALK|REP,       LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 35 } ),
/*  80 */ ( WALK|REP,       LineSpecial.Light_MaxNeighbor,           1, new[]{ TAG } ),
/*  81 */ ( WALK|REP,       LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 255 } ),
/*  82 */ ( WALK|REP,       LineSpecial.Floor_LowerToLowest,         2, new[]{ TAG, F_SLOW } ),
/*  83 */ ( WALK|REP,       LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_SLOW, 128 } ),
/*  84 */ ( WALK|REP,       LineSpecial.Floor_LowerToLowestTxTy,     2, new[]{ TAG, F_SLOW } ),
/*  85 */ ( 0,              LineSpecial.Scroll_Texture_Right,        1, new[]{ SCROLL_UNIT } ), // <- BOOM special
/*  86 */ ( WALK|REP,       LineSpecial.Door_Open,                   2, new[]{ TAG, D_SLOW } ),
/*  87 */ ( WALK|REP,       LineSpecial.Plat_PerpetualRaiseLip,      4, new[]{ TAG, P_SLOW, PLATWAIT, 0 } ),
/*  88 */ ( WALK|REP|MONST, LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_FAST, PLATWAIT, 0 } ),
/*  89 */ ( WALK|REP,       LineSpecial.Plat_Stop,                   1, new[]{ TAG } ),
/*  90 */ ( WALK|REP,       LineSpecial.Door_Raise,                  3, new[]{ TAG, D_SLOW, VDOORWAIT } ),
/*  91 */ ( WALK|REP,       LineSpecial.Floor_RaiseToLowestCeiling,  2, new[]{ TAG, F_SLOW } ),
/*  92 */ ( WALK|REP,       LineSpecial.Floor_RaiseByValue,          3, new[]{ TAG, F_SLOW, 24 } ),
/*  93 */ ( WALK|REP,       LineSpecial.Floor_RaiseByValueTxTy,      3, new[]{ TAG, F_SLOW, 24 } ),
/*  94 */ ( WALK|REP,       LineSpecial.Floor_RaiseAndCrush,         3, new[]{ TAG, F_SLOW, 10 } ),
/*  95 */ ( WALK|REP,       LineSpecial.Plat_RaiseAndStayTx0,        2, new[]{ TAG, P_SLOW/2 } ),
/*  96 */ ( WALK|REP,       LineSpecial.Floor_RaiseByTexture,        2, new[]{ TAG, F_SLOW } ),
/*  97 */ ( WALK|REP|MONST, LineSpecial.Teleport,                    1, new[]{ TAG } ),
/*  98 */ ( WALK|REP,       LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_FAST, 136 } ),
/*  99 */ ( USE|REP,        LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_FAST, 0, (byte)(KeyType.BCard | KeyType.CardIsSkull) } ),
/* 100 */ ( WALK,           LineSpecial.Stairs_BuildUpDoom,          5, new[]{ TAG, S_TURBO, 16, 0, 0 } ),
/* 101 */ ( USE,            LineSpecial.Floor_RaiseToLowestCeiling,  2, new[]{ TAG, F_SLOW } ),
/* 102 */ ( USE,            LineSpecial.Floor_LowerToHighest,        3, new[]{ TAG, F_SLOW, 128 } ),
/* 103 */ ( USE,            LineSpecial.Door_Open,                   2, new[]{ TAG, D_SLOW } ),
/* 104 */ ( WALK,           LineSpecial.Light_MinNeighbor,           1, new[]{ TAG } ),
/* 105 */ ( WALK|REP,       LineSpecial.Door_Raise,                  3, new[]{ TAG, D_FAST, VDOORWAIT } ),
/* 106 */ ( WALK|REP,       LineSpecial.Door_Open,                   2, new[]{ TAG, D_FAST } ),
/* 107 */ ( WALK|REP,       LineSpecial.Door_Close,                  2, new[]{ TAG, D_FAST } ),
/* 108 */ ( WALK,           LineSpecial.Door_Raise,                  3, new[]{ TAG, D_FAST, VDOORWAIT } ),
/* 109 */ ( WALK,           LineSpecial.Door_Open,                   2, new[]{ TAG, D_FAST } ),
/* 110 */ ( WALK,           LineSpecial.Door_Close,                  2, new[]{ TAG, D_FAST } ),
/* 111 */ ( USE,            LineSpecial.Door_Raise,                  3, new[]{ TAG, D_FAST, VDOORWAIT } ),
/* 112 */ ( USE,            LineSpecial.Door_Open,                   2, new[]{ TAG, D_FAST } ),
/* 113 */ ( USE,            LineSpecial.Door_Close,                  2, new[]{ TAG, D_FAST } ),
/* 114 */ ( USE|REP,        LineSpecial.Door_Raise,                  3, new[]{ TAG, D_FAST, VDOORWAIT } ),
/* 115 */ ( USE|REP,        LineSpecial.Door_Open,                   2, new[]{ TAG, D_FAST } ),
/* 116 */ ( USE|REP,        LineSpecial.Door_Close,                  2, new[]{ TAG, D_FAST } ),
/* 117 */ ( USE|REP,        LineSpecial.Door_Raise,                  3, new[]{ 0, D_FAST, VDOORWAIT } ),
/* 118 */ ( USE,            LineSpecial.Door_Open,                   2, new[]{ 0, D_FAST } ),
/* 119 */ ( WALK,           LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_SLOW } ),
/* 120 */ ( WALK|REP,       LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_TURBO, PLATWAIT, 0 } ),
/* 121 */ ( WALK,           LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_TURBO, PLATWAIT, 0 } ),
/* 122 */ ( USE,            LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_TURBO, PLATWAIT, 0 } ),
/* 123 */ ( USE|REP,        LineSpecial.Plat_DownWaitUpStayLip,      4, new[]{ TAG, P_TURBO, PLATWAIT, 0 } ),
/* 124 */ ( WALK,           LineSpecial.Exit_Secret,                 1, new[]{ 0 } ),
/* 125 */ ( MONWALK,        LineSpecial.Teleport,                    1, new[]{ TAG } ),
/* 126 */ ( MONWALK|REP,    LineSpecial.Teleport,                    1, new[]{ TAG } ),
/* 127 */ ( USE,            LineSpecial.Stairs_BuildUpDoom,          5, new[]{ TAG, S_TURBO, 16, 0, 0 } ),
/* 128 */ ( WALK|REP,       LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_SLOW } ),
/* 129 */ ( WALK|REP,       LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_FAST } ),
/* 130 */ ( WALK,           LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_FAST } ),
/* 131 */ ( USE,            LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_FAST } ),
/* 132 */ ( USE|REP,        LineSpecial.Floor_RaiseToNearest,        2, new[]{ TAG, F_FAST } ),
/* 133 */ ( USE,            LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_FAST, 0, (byte)(KeyType.BCard | KeyType.CardIsSkull) } ),
/* 134 */ ( USE|REP,        LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_FAST, 0, (byte)(KeyType.RCard | KeyType.CardIsSkull) } ),
/* 135 */ ( USE,            LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_FAST, 0, (byte)(KeyType.RCard | KeyType.CardIsSkull) } ),
/* 136 */ ( USE|REP,        LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_FAST, 0, (byte)(KeyType.YCard | KeyType.CardIsSkull) } ),
/* 137 */ ( USE,            LineSpecial.Door_LockedRaise,            4, new[]{ TAG, D_FAST, 0, (byte)(KeyType.YCard | KeyType.CardIsSkull) } ),
/* 138 */ ( USE|REP,        LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 255 } ),
/* 139 */ ( USE|REP,        LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 35 } ),
/* 140 */ ( USE,            LineSpecial.Floor_RaiseByValueTimes8,    3, new[]{ TAG, F_SLOW, 64 } ),
/* 141 */ ( WALK,           LineSpecial.Ceiling_CrushAndRaiseSilentA,4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),

/****** The following are all new to BOOM ******/

/* 142 */ ( WALK,           LineSpecial.Floor_RaiseByValueTimes8,    3, new[]{ TAG, F_SLOW, 64 } ),
/* 143 */ ( WALK,           LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 3 } ),
/* 144 */ ( WALK,           LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 4 } ),
/* 145 */ ( WALK,           LineSpecial.Ceiling_LowerToFloor,        2, new[]{ TAG, C_SLOW } ),
/* 146 */ ( WALK,           LineSpecial.Floor_Donut,                 3, new[]{ TAG, DORATE, DORATE } ),
/* 147 */ ( WALK|REP,       LineSpecial.Floor_RaiseByValueTimes8,    3, new[]{ TAG, F_SLOW, 64 } ),
/* 148 */ ( WALK|REP,       LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 3 } ),
/* 149 */ ( WALK|REP,       LineSpecial.Plat_UpByValueStayTx,        3, new[]{ TAG, P_SLOW/2, 4 } ),
/* 150 */ ( WALK|REP,       LineSpecial.Ceiling_CrushAndRaiseSilentA,4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/* 151 */ ( WALK|REP,       LineSpecial.FloorAndCeiling_LowerRaise,  3, new[]{ TAG, F_SLOW, C_SLOW } ),
/* 152 */ ( WALK|REP,       LineSpecial.Ceiling_LowerToFloor,        2, new[]{ TAG, C_SLOW } ),
/* 153 */ ( WALK,           LineSpecial.Floor_TransferTrigger,       1, new[]{ TAG } ),
/* 154 */ ( WALK|REP,       LineSpecial.Floor_TransferTrigger,       1, new[]{ TAG } ),
/* 155 */ ( WALK|REP,       LineSpecial.Floor_Donut,                 3, new[]{ TAG, DORATE, DORATE } ),
/* 156 */ ( WALK|REP,       LineSpecial.Light_StrobeDoom,            3, new[]{ TAG, 5, 35 } ),
/* 157 */ ( WALK|REP,       LineSpecial.Light_MinNeighbor,           1, new[]{ TAG } ),
/* 158 */ ( USE,            LineSpecial.Floor_RaiseByTexture,        2, new[]{ TAG, F_SLOW } ),
/* 159 */ ( USE,            LineSpecial.Floor_LowerToLowestTxTy,     2, new[]{ TAG, F_SLOW } ),
/* 160 */ ( USE,            LineSpecial.Floor_RaiseByValueTxTy,      3, new[]{ TAG, F_SLOW, 24 } ),
/* 161 */ ( USE,            LineSpecial.Floor_RaiseByValue,          3, new[]{ TAG, F_SLOW, 24 } ),
/* 162 */ ( USE,            LineSpecial.Plat_PerpetualRaiseLip,      4, new[]{ TAG, P_SLOW, PLATWAIT, 0 } ),
/* 163 */ ( USE,            LineSpecial.Plat_Stop,                   1, new[]{ TAG } ),
/* 164 */ ( USE,            LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_NORMAL, C_NORMAL, 10 } ),
/* 165 */ ( USE,            LineSpecial.Ceiling_CrushAndRaiseSilentA,4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/* 166 */ ( USE,            LineSpecial.FloorAndCeiling_LowerRaise,  3, new[]{ TAG, F_SLOW, C_SLOW } ),
/* 167 */ ( USE,            LineSpecial.Ceiling_LowerAndCrush,       3, new[]{ TAG, C_SLOW, 0 } ),
/* 168 */ ( USE,            LineSpecial.Ceiling_CrushStop,           1, new[]{ TAG } ),
/* 169 */ ( USE,            LineSpecial.Light_MaxNeighbor,           1, new[]{ TAG } ),
/* 170 */ ( USE,            LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 35 } ),
/* 171 */ ( USE,            LineSpecial.Light_ChangeToValue,         2, new[]{ TAG, 255 } ),
/* 172 */ ( USE,            LineSpecial.Light_StrobeDoom,            3, new[]{ TAG, 5, 35 } ),
/* 173 */ ( USE,            LineSpecial.Light_MinNeighbor,           1, new[]{ TAG } ),
/* 174 */ ( USE|MONST,      LineSpecial.Teleport,                    1, new[]{ TAG } ),
/* 175 */ ( USE,            LineSpecial.Door_CloseWaitOpen,          3, new[]{ TAG, F_SLOW, 240 } ),
/* 176 */ ( USE|REP,        LineSpecial.Floor_RaiseByTexture,        2, new[]{ TAG, F_SLOW } ),
/* 177 */ ( USE|REP,        LineSpecial.Floor_LowerToLowestTxTy,     2, new[]{ TAG, F_SLOW } ),
/* 178 */ ( USE|REP,        LineSpecial.Floor_RaiseByValueTimes8,    3, new[]{ TAG, F_SLOW, 64 } ),
/* 179 */ ( USE|REP,        LineSpecial.Floor_RaiseByValueTxTy,      3, new[]{ TAG, F_SLOW, 24 } ),
/* 180 */ ( USE|REP,        LineSpecial.Floor_RaiseByValue,          3, new[]{ TAG, F_SLOW, 24 } ),
/* 181 */ ( USE|REP,        LineSpecial.Plat_PerpetualRaiseLip,      4, new[]{ TAG, P_SLOW, PLATWAIT, 0 } ),
/* 182 */ ( USE|REP,        LineSpecial.Plat_Stop,                   1, new[]{ TAG } ),
/* 183 */ ( USE|REP,        LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_NORMAL, C_NORMAL, 10 } ),
/* 184 */ ( USE|REP,        LineSpecial.Ceiling_CrushAndRaiseA,      4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/* 185 */ ( USE|REP,        LineSpecial.Ceiling_CrushAndRaiseSilentA,4, new[]{ TAG, C_SLOW, C_SLOW, 10 } ),
/* 186 */ ( USE|REP,        LineSpecial.FloorAndCeiling_LowerRaise,  3, new[]{ TAG, F_SLOW, C_SLOW } ),
/* 187 */ ( USE|REP,        LineSpecial.Ceiling_LowerAndCrush,       3, new[]{ TAG, C_SLOW, 0 } ),
/* 188 */ ( USE|REP,        LineSpecial.Ceiling_CrushStop,           1, new[]{ TAG } ),
/* 189 */ ( USE,            LineSpecial.Floor_TransferTrigger,       1, new[]{ TAG } ),
/* 190 */ ( USE|REP,        LineSpecial.Floor_TransferTrigger,       1, new[]{ TAG } ),
/* 191 */ ( USE|REP,        LineSpecial.Floor_Donut,                 3, new[]{ TAG, DORATE, DORATE } ),
/* 192 */ ( USE|REP,        LineSpecial.Light_MaxNeighbor,           1, new[]{ TAG } ),
/* 193 */ ( USE|REP,        LineSpecial.Light_StrobeDoom,            3, new[]{ TAG, 5, 35 } ),
/* 194 */ ( USE|REP,        LineSpecial.Light_MinNeighbor,           1, new[]{ TAG } ),
/* 195 */ ( USE|REP|MONST,  LineSpecial.Teleport,                    1, new[]{ TAG } ),
/* 196 */ ( USE|REP,        LineSpecial.Door_CloseWaitOpen,          3, new[]{ TAG, D_SLOW, 240 } ),
/* 197 */ ( SHOOT,          LineSpecial.Exit_Normal,                 1, new[]{ 0 } ),
/* 198 */ ( SHOOT,          LineSpecial.Exit_Secret,                 1, new[]{ 0 } ),
/* 199 */ ( WALK,           LineSpecial.Ceiling_LowerToLowest,       2, new[]{ TAG, C_SLOW } ),
/* 200 */ ( WALK,           LineSpecial.Ceiling_LowerToHighestFloor, 2, new[]{ TAG, C_SLOW } ),
/* 201 */ ( WALK|REP,       LineSpecial.Ceiling_LowerToLowest,       2, new[]{ TAG, C_SLOW } ),
/* 202 */ ( WALK|REP,       LineSpecial.Ceiling_LowerToHighestFloor, 2, new[]{ TAG, C_SLOW } ),
/* 203 */ ( USE,            LineSpecial.Ceiling_LowerToLowest,       2, new[]{ TAG, C_SLOW } ),
/* 204 */ ( USE,            LineSpecial.Ceiling_LowerToHighestFloor, 2, new[]{ TAG, C_SLOW } ),
/* 205 */ ( USE|REP,        LineSpecial.Ceiling_LowerToLowest,       2, new[]{ TAG, C_SLOW } ),
/* 206 */ ( USE|REP,        LineSpecial.Ceiling_LowerToHighestFloor, 2, new[]{ TAG, C_SLOW } ),
/* 207 */ ( WALK|MONST,     LineSpecial.Teleport_NoFog,              1, new[]{ TAG } ),
/* 208 */ ( WALK|REP|MONST, LineSpecial.Teleport_NoFog,              1, new[]{ TAG } ),
/* 209 */ ( USE|MONST,      LineSpecial.Teleport_NoFog,              1, new[]{ TAG } ),
/* 210 */ ( USE|REP|MONST,  LineSpecial.Teleport_NoFog,              1, new[]{ TAG } ),
/* 211 */ ( USE|REP,        LineSpecial.Plat_ToggleCeiling,          1, new[]{ TAG } ),
/* 212 */ ( WALK|REP,       LineSpecial.Plat_ToggleCeiling,          1, new[]{ TAG } ),
/* 213 */ ( 0,              LineSpecial.Transfer_FloorLight,         1, new[]{ TAG } ),
/* 214 */ ( 0,              LineSpecial.Scroll_Ceiling,              5, new[]{ TAG, 6, 0, 0, 0 } ),
/* 215 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 6, 0, 0, 0 } ),
/* 216 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 6, 1, 0, 0 } ),
/* 217 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 6, 2, 0, 0 } ),
/* 218 */ ( 0,              LineSpecial.Scroll_Texture_Model,        2, new[]{ LINETAG, 2 } ),
/* 219 */ ( WALK,           LineSpecial.Floor_LowerToNearest,        2, new[]{ TAG, F_SLOW } ),
/* 220 */ ( WALK|REP,       LineSpecial.Floor_LowerToNearest,        2, new[]{ TAG, F_SLOW } ),
/* 221 */ ( USE,            LineSpecial.Floor_LowerToNearest,        2, new[]{ TAG, F_SLOW } ),
/* 222 */ ( USE|REP,        LineSpecial.Floor_LowerToNearest,        2, new[]{ TAG, F_SLOW } ),
/* 223 */ ( 0,              LineSpecial.Sector_SetFriction,          2, new[]{ TAG, 0 } ),
/* 224 */ ( 0,              LineSpecial.Sector_SetWind,              4, new[]{ TAG, 0, 0, 1 } ),
/* 225 */ ( 0,              LineSpecial.Sector_SetCurrent,           4, new[]{ TAG, 0, 0, 1 } ),
/* 226 */ ( 0,              LineSpecial.PointPush_SetForce,          4, new[]{ TAG, 0, 0, 1 } ),
/* 227 */ ( WALK,           LineSpecial.Elevator_RaiseToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 228 */ ( WALK|REP,       LineSpecial.Elevator_RaiseToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 229 */ ( USE,            LineSpecial.Elevator_RaiseToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 230 */ ( USE|REP,        LineSpecial.Elevator_RaiseToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 231 */ ( WALK,           LineSpecial.Elevator_LowerToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 232 */ ( WALK|REP,       LineSpecial.Elevator_LowerToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 233 */ ( USE,            LineSpecial.Elevator_LowerToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 234 */ ( USE|REP,        LineSpecial.Elevator_LowerToNearest,     2, new[]{ TAG, ELEVATORSPEED } ),
/* 235 */ ( WALK,           LineSpecial.Elevator_MoveToFloor,        2, new[]{ TAG, ELEVATORSPEED } ),
/* 236 */ ( WALK|REP,       LineSpecial.Elevator_MoveToFloor,        2, new[]{ TAG, ELEVATORSPEED } ),
/* 237 */ ( USE,            LineSpecial.Elevator_MoveToFloor,        2, new[]{ TAG, ELEVATORSPEED } ),
/* 238 */ ( USE|REP,        LineSpecial.Elevator_MoveToFloor,        2, new[]{ TAG, ELEVATORSPEED } ),
/* 239 */ ( WALK,           LineSpecial.Floor_TransferNumeric,       1, new[]{ TAG } ),
/* 240 */ ( WALK|REP,       LineSpecial.Floor_TransferNumeric,       1, new[]{ TAG } ),
/* 241 */ ( USE,            LineSpecial.Floor_TransferNumeric,       1, new[]{ TAG } ),
/* 242 */ ( 0,              LineSpecial.Transfer_Heights,            1, new[]{ TAG } ),
/* 243 */ ( WALK|MONST,     LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 0 } ),
/* 244 */ ( WALK|REP|MONST, LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 0 } ),
/* 245 */ ( 0,              LineSpecial.Scroll_Ceiling,              5, new[]{ TAG, 5, 0, 0, 0 } ),
/* 246 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 5, 0, 0, 0 } ),
/* 247 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 5, 1, 0, 0 } ),
/* 248 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 5, 2, 0, 0 } ),
/* 249 */ ( 0,              LineSpecial.Scroll_Texture_Model,        2, new[]{ LINETAG, 1 } ),
/* 250 */ ( 0,              LineSpecial.Scroll_Ceiling,              5, new[]{ TAG, 4, 0, 0, 0 } ),
/* 251 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 4, 0, 0, 0 } ),
/* 252 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 4, 1, 0, 0 } ),
/* 253 */ ( 0,              LineSpecial.Scroll_Floor,                5, new[]{ TAG, 4, 2, 0, 0 } ),
/* 254 */ ( 0,              LineSpecial.Scroll_Texture_Model,        2, new[]{ LINETAG, 0 } ),
/* 255 */ ( 0,              LineSpecial.Scroll_Texture_Offsets,      0),
/* 256 */ ( WALK|REP,       LineSpecial.Stairs_BuildUpDoom,          5, new[]{ TAG, S_SLOW, 8, 0, 0 } ),
/* 257 */ ( WALK|REP,       LineSpecial.Stairs_BuildUpDoom,          5, new[]{ TAG, S_TURBO, 16, 0, 0 } ),
/* 258 */ ( USE|REP,        LineSpecial.Stairs_BuildUpDoom,          5, new[]{ TAG, S_SLOW, 8, 0, 0 } ),
/* 259 */ ( USE|REP,        LineSpecial.Stairs_BuildUpDoom,          5, new[]{ TAG, S_TURBO, 16, 0, 0 } ),
/* 260 */ ( 0,              LineSpecial.TranslucentLine,             2, new[]{ LINETAG, 128 } ),
/* 261 */ ( 0,              LineSpecial.Transfer_CeilingLight,       1, new[]{ TAG } ),
/* 262 */ ( WALK|MONST,     LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 1 } ),
/* 263 */ ( WALK|REP|MONST, LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 1 } ),
/* 264 */ ( MONWALK,        LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 1 } ),
/* 265 */ ( MONWALK|REP,    LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 1 } ),
/* 266 */ ( MONWALK,        LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 0 } ),
/* 267 */ ( MONWALK|REP,    LineSpecial.Teleport_Line,               3, new[]{ TAG, TAG, 0 } ),
/* 268 */ ( MONWALK,        LineSpecial.Teleport_NoFog,              1, new[]{ TAG } ),
/* 269 */ ( MONWALK|REP,    LineSpecial.Teleport_NoFog,              1, new[]{ TAG } ),
/* 270 */ ( 0,              0,                                       0 ),
/* 271 */ ( 0,              LineSpecial.Static_Init,                 3, new[]{ TAG, 255, 0 } ),
/* 272 */ ( 0,              LineSpecial.Static_Init,                 3, new[]{ TAG, 255, 1 } )
};
        public static int NUM_SPECIALS = 272;
    }
}
