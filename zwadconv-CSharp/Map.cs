using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zwadconv_CSharp
{
    class Map
    {
        public string Name = "";
        public MapFormat Format = MapFormat.Doom;
        public Dictionary<int, Lump> Lumps = new();
        public MapInfo MapInfo = new();
    }

    enum MapFormat
    {
        Doom,
        Hexen,
        UDMF
    }

    class MapInfo // https://zdoom.org/wiki/MAPINFO
    {
        // Get name from Map Object.
        public string MapTitle = string.Empty;
        public bool MapTitleIsDefined = false; // has "lookup"
        public bool DefaultMap = false;
        public bool GameDefaults = false;

        public string TitlePatch = string.Empty;
        public string Next = string.Empty;
        public string SecretNext = string.Empty;
        public string Sky1 = string.Empty;
        public string EnterPic = string.Empty;
        public string ExitPic = string.Empty;
        public string Music = string.Empty;
        public string InterMusic = string.Empty;

        public bool NoJump = false;
        public bool NoCrouch = false;
        public bool Map07Special = false;

        public int SuckTime = -1;
    }
}
