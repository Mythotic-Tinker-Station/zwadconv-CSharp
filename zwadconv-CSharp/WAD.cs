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
        public Dictionary<int, Lump> Lumps = new();

        // Specific lumps we need
        public Dictionary<int, Color> Playpal = new();
        public Dictionary<int, string> Pnames = new();

        public List<string> Patches = new();
        public List<string> UsedPatches = new();
        public List<string> Flats = new();
        public List<string> UsedFlats = new();

        public int SoundCount = 0; // DMX/WAV/OGG/FLAC
        public int MusicCount = 0; // MIDI
        public List<string> MusInfoLumps = new();
        public List<string> SndDefs = new();
    }
}
