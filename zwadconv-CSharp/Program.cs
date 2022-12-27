using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using static zwadconv_CSharp.Constants;

namespace zwadconv_CSharp
{
    class Program
    {
        static readonly byte MaxSteps = 2;

        static readonly WAD Wad_PWAD = new();
        static string WadName = string.Empty;

        static void Main(string[] args)
        {
            bool promptForExit = false;

            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.wad");

            string wadPath;
            if (args.Length == 0)
            {
                promptForExit = true;

                if (files.Length == 0) ExitOnError("Can't find any .wad files.");
                if (files.Length == 1)
                {
                    wadPath = files[0];
                }
                else
                {
                WadPrompt:
                    Console.WriteLine("Pick a wad:");

                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}. {files[i].Split('\\')[^1]} ({new FileInfo(files[i]).Length / 1024d:N}KiB)");
                    }

                    Console.Write("\nMake a selection: ");
                    string line = Console.ReadLine();
                    Console.Write("\n");

                    if (line != null)
                    {
                        if (int.TryParse(line, out int sel))
                        {
                            sel -= 1;

                            if (sel >= 0 && sel < files.Length)
                            {
                                wadPath = files[sel];
                                goto AfterArgs;
                            }
                        }
                    }

                    Console.WriteLine($"Invalid selection: {line}");
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.Write("\n");
                    goto WadPrompt;
                }
            }
            else
            {
                wadPath = args[0].Replace("/", "\\");
            }

        AfterArgs:
            if (!File.Exists(wadPath))
            {
                ExitOnError("File doesn't exist:", wadPath);
            }

            WadName = string.Concat(wadPath.Split('\\')[^1].SkipLast(4));
            Console.WriteLine($"File: {wadPath.Split('\\')[^1]}");

            byte[] pwadBytes = File.ReadAllBytes(wadPath);

            string wadType = pwadBytes.ReadToString(0, 4);
            Console.WriteLine($"Type: {wadType}");

            if (!wadType.Equals("IWAD") && !wadType.Equals("PWAD"))
            {
                ExitOnError("File isn't a WAD:", wadPath);
            }

            int lumpCount = BitConverter.ToInt32(new[] { pwadBytes[4], pwadBytes[5], pwadBytes[6], pwadBytes[7] });
            Console.WriteLine($"Lumps: {lumpCount}");
            Wad_PWAD.Lumps = new(lumpCount);

            int dirOffset = BitConverter.ToInt32(new[] { pwadBytes[8], pwadBytes[9], pwadBytes[10], pwadBytes[11] });
            Console.WriteLine($"Lump directory starts at {dirOffset} bytes\n");

            DateTime start = DateTime.Now;

            Console.WriteLine($"Processing {WadName}.wad.");

            bool inMap = false;
            Map map = new();

            List<Map> mapsToProcess = new();

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
            Console.WriteLine($"Finished step 2 complete @ {lumpCount / sw.Elapsed.TotalSeconds:0.##} Lumps/s\n");

            Console.WriteLine($"Step 2 of 2: Processing maps.");
            sw = Stopwatch.StartNew();
            for (int i = 0; i < mapsToProcess.Count; i++)
            {
                map = mapsToProcess[i];

                if (map.Format is not MapFormat.UDMF)
                {
                    Console.WriteLine($"-- {map.Name} ({map.Format})");
                    MapConverter.ToHexen(pwadBytes, map/*, Wad_PWAD*/);
                    Console.WriteLine("-- Done.");
                }

                Console.Title = $"(2/{MaxSteps}) Lumps: {i + 1}/{mapsToProcess.Count} ({(i + 1) / sw.Elapsed.TotalSeconds:0.##} Lumps/s)";
            }

            sw.Stop();
            Console.WriteLine($"Finished step 2 complete @ {mapsToProcess.Count / sw.Elapsed.TotalSeconds:0.##} Lumps/s\n");

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
