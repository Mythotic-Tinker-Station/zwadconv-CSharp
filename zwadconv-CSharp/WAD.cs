using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zwadconv_CSharp
{
    class WAD
    {
        // All the lumps
        public Dictionary<int, Lump> Lumps = new Dictionary<int, Lump>();

        // Specific lumps we need
        public Dictionary<int, Color>  Playpal = new Dictionary<int, Color>();
        public Dictionary<int, string> Pnames  = new Dictionary<int, string>();

        public List<string> Patches     = new List<string>();
        public List<string> UsedPatches = new List<string>();
        public List<string> Flats       = new List<string>();
        public List<string> UsedFlats   = new List<string>();

        public int          SoundCount   = 0; // DMX/WAV/OGG/FLAC
        public int          MusicCount   = 0; // MIDI
        public List<string> MusInfoLumps = new List<string>();
        public List<string> SndDefs      = new List<string>();
    }
}
