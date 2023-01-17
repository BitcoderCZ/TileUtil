using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemPlus;
using SystemPlus.Utils;

namespace TileUtil
{
    public class TileImage
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public readonly byte[] Data;

        public TileImage(DirectBitmap db) : this((ushort)db.Width, (ushort)db.Height, FromBitmapBytes(db.Data))
        { }

        private TileImage(ushort width, ushort height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        public static TileImage LoadTile(string path)
        {
            byte[] _data;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using (DeflateStream ds = new DeflateStream(fs, CompressionMode.Decompress, true))
            using (MemoryStream ms = new MemoryStream()) {
                ds.CopyTo(ms);
                _data = ms.ToArray();
            }
            byte[] data = new byte[_data.Length - 4];
            Array.Copy(_data, 8, data, 0, data.Length);
            ushort width = BitConverter.ToUInt16(_data, 0);
            ushort height = BitConverter.ToUInt16(_data, 2);
            return new TileImage(width, height, data);
        }

        public static TileImage LoadPng(string path)
        {
            DirectBitmap db = DirectBitmap.Load(path, false);
            TileImage ti = new TileImage(db);
            db.Dispose();
            return ti;
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, new byte[0]); // clear file in case it exists
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            byte[] bytes = new byte[4 + Data.Length];
            Array.Copy(BitConverter.GetBytes(Width), bytes, 2);
            Array.Copy(BitConverter.GetBytes(Height), 0, bytes, 2, 2);
            Array.Copy(Data, 0, bytes, 4, Data.Length);
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;
            using (DeflateStream ds = new DeflateStream(fs, CompressionLevel.Optimal, true)) // create compression stream
                ms.CopyTo(ds); // copress ms to fs

            fs.Flush();
            fs.Dispose();
            ms.Dispose();
        }

        public void SavePng(string path)
        {
            DirectBitmap db = new DirectBitmap(Width, Height);

            for (int i = 0; i < Data.Length; i++) {
                db.Data[i * 2] =      SmallToNormal((byte)(Data[i] & 0b_0000_1111));
                db.Data[i * 2 + 1] = SmallToNormal((byte)((Data[i] & 0b_1111_0000) >> 4));
            }

            db.Bitmap.Save(path);
            db.Dispose();
        }

        private static byte NormalToSmall(int val) => (byte)(val & 0b_1111);
        private static int SmallToNormal(byte _val)
        {
            uint val = (uint)(_val | (_val << 4));
            return (int)((uint)0b_11111111_00000000_00000000_00000000 | val | (val << 8) | (val << 16)); // alpha always 255
        }

        private static byte[] FromBitmapBytes(int[] pixels)
        {
            byte[] bytes = new byte[pixels.Length / 2];

            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = NormalToSmall(pixels[i * 2]);
                bytes[i] |= (byte)(NormalToSmall(pixels[i * 2 + 1]) << 4);
            }

            return bytes;
        }
    }
}
