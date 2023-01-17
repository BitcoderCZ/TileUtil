using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemPlus.Utils;

namespace TileUtil
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Execute(args);
        }

        public static void User()
        {
            while (true) {
                Console.Write("Input: ");
                string input = Console.ReadLine();
                Execute(
                    Regex
                    .Matches(input, @"(?<match>\w+)|\""(?<match>[\w\s]*)""")
                    .Cast<Match>()
                    .Select(m => m.Groups["match"].Value).ToArray()
                    );
            }
        }

        private static void Execute(string[] args)
        {
            if (args == null || args.Length < 1 || (args != null && args.Length > 0 && args[0].ToLower() == "help")) {
                Console.WriteLine("help - shows help\n" +
                                  "extract_dir {directory} - Extract all supertiles in directory\n" +
                                  "extract {path(.tiles file)} - Extracts super tile file\n" +
                                  "convert_tile_dir {directory} - Convert tiles in directory to pngs\n" +
                                  "convert_tile - Convert tile to png");
                return;
            }

            string command = args[0].ToLower();

            if (command == "extract_dir" && args.Length > 1) {
                string dir = args[1];
                if (!Directory.Exists(dir)) {
                    Console.WriteLine($"Directory {dir} doesn't exist");
                    return;
                }
                string[] files = Directory.GetFiles(dir);
                for (int i = 0; i < files.Length; i++)
                    if (Path.GetExtension(files[i]) == ".tiles")
                        try {
                            SuperTileExtractor.Extract(files[i]);
                        }
                        catch (Exception e) {
                            Console.WriteLine($"Failed to extract {Path.GetFileName(files[i])}, Exception: {e}");
                        }
                Console.WriteLine("Done, press any key to exit...");
                Console.ReadKey(true);
            }
            else if (command == "extract" && args.Length > 1) {
                string path = args[1];
                if (!File.Exists(path)) {
                    Console.WriteLine($"File {path} doesn't exist");
                    return;
                }
                else if (Path.GetExtension(path) != ".tiles") {
                    Console.WriteLine($"You must select .tiles file, not \"{Path.GetExtension(path)}\"");
                    return;
                }
                try {
                    SuperTileExtractor.Extract(path);
                }
                catch (Exception e) {
                    Console.WriteLine($"Failed to extract {Path.GetFileName(path)}, Exception: {e}");
                }
                Console.WriteLine("Done, press any key to exit...");
                Console.ReadKey(true);
            }
            else if (command == "convert_tile_dir" && args.Length > 1) {
                string dir = args[1];
                if (!Directory.Exists(dir)) {
                    Console.WriteLine($"Directory {dir} doesn't exist");
                    return;
                }
                string saveTo = Directory.GetParent(dir).FullName + "/" + new DirectoryInfo(dir).Name + "Png/";
                Directory.CreateDirectory(saveTo);
                string[] files = Directory.GetFiles(dir);

                int y = Console.CursorTop;
                int c = 0;

                for (int i = 0; i < files.Length; i++)
                    if (Path.GetExtension(files[i]) == ".tile") {
                        try {
                            TileImage img = TileImage.LoadTile(files[i]);
                            img.SavePng(saveTo + Path.GetFileNameWithoutExtension(files[i]) + ".png");
                        }
                        catch (Exception e) {
                            Console.WriteLine($"Failed to convert {Path.GetFileName(files[i])}, Exception: {e}");
                        }
                        c++;
                        int _y = Console.CursorTop;
                        Console.SetCursorPosition(0, y);
                        Console.WriteLine($"Converted {c}");
                        Console.SetCursorPosition(0, _y);
                    }
                Console.WriteLine("Done, press any key to exit...");
                Console.ReadKey(true);
            }
            else if (command == "convert_tile" && args.Length > 1) {
                string path = args[1];
                if (!File.Exists(path)) {
                    Console.WriteLine($"File {path} doesn't exist");
                    return;
                }
                else if (Path.GetExtension(path) != ".tile") {
                    Console.WriteLine($"You must select .tile file, not \"{Path.GetExtension(path)}\"");
                    return;
                }
                try {
                    TileImage img = TileImage.LoadTile(path);
                    img.SavePng(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".png");
                }
                catch (Exception e) {
                    Console.WriteLine($"Failed to convert {Path.GetFileName(path)}, Exception: {e}");
                }
                Console.WriteLine("Done, press any key to exit...");
                Console.ReadKey(true);
            }
            else if (command == "user" && args.Length == 1) {
                User();
            }
            else
                Console.WriteLine($"Command {command} with {args.Length} parameters doesn't exist, type \"help\" for command info");
        }
    }
}
