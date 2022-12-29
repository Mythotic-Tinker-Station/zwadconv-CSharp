using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Threading;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static zwadconv_CSharp.Constants;

namespace zwadconv_CSharp
{
    class Program
    {
        static readonly byte MaxSteps = 2;

        static readonly WAD Wad_PWAD = new();
        static string WadName = string.Empty;

        static string InputPath = string.Empty;
        public static string OutputPath = string.Empty;

        static void Main(string[] args)
        {
            bool promptForExit = false;

            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.wad");

            if (args.Length < 2)
            {
                ExitOnError($"Usage: {Process.GetCurrentProcess().MainModule.FileName.Split(Path.DirectorySeparatorChar)[^1]} <input> <output>");
            }
            else
            {
                InputPath = args[0].Replace("/", "\\");
                OutputPath = args[1].Replace("/", "\\");
            }

            if (!File.Exists(InputPath))
            {
                ExitOnError("File doesn't exist:", InputPath);
            }

            WadName = InputPath.Split('\\')[^1];
            Console.WriteLine($"File: {InputPath.Split('\\')[^1]}");

            byte[] pwadBytes = File.ReadAllBytes(InputPath);

            string wadType = pwadBytes.ReadToString(0, 4);
            Console.WriteLine($"Type: {wadType}");

            if (!wadType.Equals("IWAD") && !wadType.Equals("PWAD"))
            {
                ExitOnError("File isn't a WAD:", InputPath);
            }

            int lumpCount = BitConverter.ToInt32(new[] { pwadBytes[4], pwadBytes[5], pwadBytes[6], pwadBytes[7] });
            Console.WriteLine($"Lumps: {lumpCount}");
            Wad_PWAD.Lumps = new(lumpCount);

            int dirOffset = BitConverter.ToInt32(new[] { pwadBytes[8], pwadBytes[9], pwadBytes[10], pwadBytes[11] });
            Console.WriteLine($"Lump directory starts at {dirOffset} bytes\n");

            DateTime start = DateTime.Now;

            Console.WriteLine($"Processing {WadName}.");

            bool inMap = false;
            Map map = new();

            List<Map> mapsToProcess = new();

            for (int i = dirOffset, pos = 0; i < pwadBytes.Length; pos++)
            {
                int offset = BitConverter.ToInt32(new[] { pwadBytes[i++], pwadBytes[i++], pwadBytes[i++], pwadBytes[i++] });
                int size = BitConverter.ToInt32(new[] { pwadBytes[i++], pwadBytes[i++], pwadBytes[i++], pwadBytes[i++] });
                string name = string.Concat(new[] { (char)pwadBytes[i++], (char)pwadBytes[i++], (char)pwadBytes[i++], (char)pwadBytes[i++], (char)pwadBytes[i++], (char)pwadBytes[i++], (char)pwadBytes[i++], (char)pwadBytes[i++] }).Trim('\0');
                
                Wad_PWAD.Lumps[pos] = new Lump()
                {
                    Name = name,
                    Offset = offset,
                    Size = size
                };
            }

            Console.WriteLine($"Step 1 of 2: Gathering maps.");
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < Wad_PWAD.Lumps.Count; i++)
            {
                Lump lump = Wad_PWAD.Lumps[i];
                string lumpName = lump.Name;
                int lumpOffset = lump.Offset;
                int lumpSize = lump.Size;

                // Check for the end of a map
                if (inMap &&
                    (!lumpName.Equals("THINGS") &&
                    !lumpName.Equals("LINEDEFS") &&
                    !lumpName.Equals("SIDEDEFS") &&
                    !lumpName.Equals("VERTEXES") &&
                    !lumpName.Equals("SEGS") &&
                    !lumpName.Equals("SSECTORS") &&
                    !lumpName.Equals("NODES") &&
                    !lumpName.Equals("SECTORS") &&
                    !lumpName.Equals("REJECT") &&
                    !lumpName.Equals("BLOCKMAP") &&
                    !lumpName.Equals("MAPINFO") &&
                    !lumpName.Equals("BEHAVIOR") &&
                    !lumpName.Contains("SCRIPT") &&
                    !lumpName.Equals("TEXTMAP") &&
                    !lumpName.Equals("ZNODES") &&
                    !lumpName.Equals("DIALOGUE") &&
                    !lumpName.Equals("ENDMAP") &&
                    !lumpName.StartsWith("GL_")) || i == Wad_PWAD.Lumps.Count - 1)
                {
                    if (i == Wad_PWAD.Lumps.Count - 1)
                    {
                        map.Lumps.Add(map.Lumps.Count, lump);
                    }

                    mapsToProcess.Add(map);

                    inMap = false;
                    map = new();

                    goto EndLoop;
                }

                if (lumpSize > 0)
                {
                    if (i + 1 < lumpCount && lump.Name.Equals("THINGS"))
                    {
                        inMap = true;
                    }

                    if (inMap)
                    {
                        if (map.Name.Equals(""))
                        {
                            map.Name = Wad_PWAD.Lumps[i - 1].Name;
                            map.Lumps.Add(map.Lumps.Count, lump);
                        }
                        else
                        {
                            if (map.Format is not MapFormat.UDMF && lumpName.Equals("BEHAVIOR"))
                            {
                                map.Format = MapFormat.Hexen;
                            }
                            else if (lumpName.Equals("TEXTMAP"))
                            {
                                map.Format = MapFormat.UDMF;
                            }

                            map.Lumps.Add(map.Lumps.Count, lump);
                        }

                        goto EndLoop;
                    }
                }

                if (inMap)
                {
                    map.Lumps.Add(map.Lumps.Count, lump);
                }

            EndLoop:
                Console.Title = $"(1/{MaxSteps}) Lumps: {i + 1}/{lumpCount} ({(i + 1) / sw.Elapsed.TotalSeconds:0.##} Lumps/s)";
            }

            sw.Stop();
            //Console.Title = $"(3/3) Lumps: {i + 1}/{lumpCount} ({(i + 1) / sw.Elapsed.TotalSeconds:0.##} Lumps/s)";
            Console.WriteLine($"Finished step 1.\n");

            Console.WriteLine($"Step 2 of 2: Processing {mapsToProcess.Count} maps.");

            List<byte> data = new();
            List<byte> directory = new();
            int currentOffset = 12;
            int totalLumps = 0;

            sw = Stopwatch.StartNew();
            for (int i = 0; i < mapsToProcess.Count; i++)
            {
                map = mapsToProcess[i];

                if (map.Format is MapFormat.UDMF)
                {
                    Console.WriteLine($"{map.Name} is UDMF format");
                }
                else if (map.Format is MapFormat.Hexen)
                {
                    Console.WriteLine($"{map.Name} is already Hexen format");
                }
                else if (map.Format is MapFormat.Doom)
                {
                    Console.WriteLine($"-- Converting {map.Name} to Hexen format");
                    MapConverter.ToHexen(pwadBytes, map, ref data, ref directory, ref currentOffset, ref totalLumps);
                    Console.WriteLine("-- Done.");
                }

                Console.Title = $"(2/{MaxSteps}) Lumps: {i + 1}/{mapsToProcess.Count} ({(i + 1) / sw.Elapsed.TotalSeconds:0.##} Lumps/s)";
            }

            sw.Stop();
            Console.WriteLine($"Finished step 2.\n");

            Console.WriteLine($"Total lumps: {totalLumps}.");
            Console.WriteLine($"Directory location: {data.Count + 12}.\n");

            Console.WriteLine("Writing output file.");
            byte[] header = new byte[12] { (byte)'P', (byte)'W', (byte)'A', (byte)'D', 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

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

            File.WriteAllBytes(OutputPath, fullFile.ToArray());

            TimeSpan total = DateTime.Now - start;
            Console.WriteLine($"Time Taken: {(total.TotalMinutes > 1 ? $"{(int)Math.Floor(total.TotalMinutes)} minute{((int)Math.Floor(total.TotalMinutes) != 1 ? "s" : "")} and " : "")}{total.Seconds}.{total.Milliseconds} seconds.");

            if (promptForExit)
            {
                Console.Write("Press any key to exit...");
                Console.ReadKey(true);
                Console.Write('\n');
            }
        }

        static void ExitOnError(params string[] errors)
        {
            foreach (string error in errors)
            {
                Console.WriteLine(error);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey(true);
            Console.Write('\n');
            Environment.Exit(1);
        }

        static void Log(string text)
        {
            Console.Write(text);

            StreamWriter logger = new(@$".\Logs\{WadName}.txt");
            logger.Write(text);
            logger.Close();
        }

        static void LogLine(string text)
        {
            Console.WriteLine(text);

            StreamWriter logger = new(@$".\Logs\{WadName}.txt");
            logger.WriteLine(text);
            logger.Close();
        }
    }

    public static class Extentions
    {
        public static string ReadToString(this byte[] data, int start, int amount)
        {
            string temp = string.Empty;

            try
            {
                for (int i = start; i < start + amount; i++)
                {
                    temp += (char)data[i];
                }
            }
            catch
            {

            }

            return temp;
        }

        public static string ReadToString(this byte[] data)
        {
            string temp = string.Empty;

            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    temp += (char)data[i];
                }
            }
            catch
            {

            }

            return temp;
        }

        public static byte[] ToByteArray(this string input)
        {
            byte[] temp = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                temp[i] = (byte)input[i];
            }

            return temp;
        }
    }
}
