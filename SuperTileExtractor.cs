using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemPlus.Utils;
using SystemPlus.Vectors;

namespace TileUtil
{
    public static class SuperTileExtractor
    {
        public static void Extract(string path)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            string[] nameSplit = Path.GetFileNameWithoutExtension(path).Split('_');

            Vector2Int baseCoord = new Vector2Int(int.Parse(nameSplit[1]), int.Parse(nameSplit[0]));
            baseCoord *= 64;

            string outputDir = $"{new FileInfo(path).DirectoryName}/{baseCoord.x}_{baseCoord.y}/";
            Directory.CreateDirectory(outputDir);

            byte[] bytes = File.ReadAllBytes(path);

            byte[][] splitTiles = bytes.Split(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // split by png header

            Console.WriteLine($"Extracting {Path.GetFileName(path)}");
            Console.WriteLine($"Detected {splitTiles.Length - 1} tiles");

            int maxIndexStringLength = (splitTiles.Length - 1).ToString().Length;

            int c = 1;
            object objLock = new object();
            Parallel.For(1, splitTiles.Length, Util.DefaultParallelOp, (int i) =>
            {
                string tileName = $"{baseCoord.x + (i - 1) % 64}_{baseCoord.y + (i - 1) / 64}";

                lock (objLock) {
                    Console.WriteLine($"[{new string('0', maxIndexStringLength - c.ToString().Length)}{c}/{splitTiles.Length - 1}] {tileName}");
                    c++;
                }

                // load bitmap without creating file
                byte[] imageBytes = new byte[splitTiles[i].Length + 8];
                Array.Copy(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, imageBytes, 8);
                Array.Copy(splitTiles[i], 0, imageBytes, 8, splitTiles[i].Length);

                MemoryStream ms = new MemoryStream(imageBytes);
                Bitmap bm = (Bitmap)Image.FromStream(ms);
                ms.Dispose();
                DirectBitmap db = DirectBitmap.LoadFromBm(bm, false);
                bm.Dispose();

                TileImage tm = new TileImage(db);
                db.Dispose();
                tm.Save(outputDir + tileName + "_16.tile");
            });

            watch.Stop();
            Console.WriteLine($"Extracted in {watch.Elapsed.Seconds}s");
        }
    }
}
