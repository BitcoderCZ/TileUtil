using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileUtil
{
    public static class Util
    {
        public static readonly int CoreCount = Environment.ProcessorCount;
        public static readonly ParallelOptions DefaultParallelOp = new ParallelOptions() { MaxDegreeOfParallelism = CoreCount };

        public static byte[][] Split(this byte[] array, byte[] separator)
        {
            List<int> splits = new List<int>();
            object objLock = new object();
            byte firstByte = separator[0];
            Parallel.For(0, array.Length - separator.Length, DefaultParallelOp, (int i) =>
            {
                if (array[i] == firstByte) {
                    for (int j = 1; j < separator.Length; j++)
                        if (array[i + j] != separator[j])
                            return;

                    lock (objLock)
                        splits.Add(i);
                }
            });

            splits.Sort();

            List<byte[]> arrays = new List<byte[]>();

            int startIndex = 0;
            for (int i = 0; i < splits.Count; i++) {
                arrays.Add(array.SubArary(startIndex, splits[i] - startIndex));
                startIndex = splits[i] + separator.Length;
            }

            if (array.Length - startIndex > 0)
                arrays.Add(array.SubArary(startIndex, array.Length - startIndex));

            return arrays.ToArray();
        }

        public static T[] SubArary<T>(this T[] array, int index, int lenght)
        {
            T[] newArray = new T[lenght];
            Array.Copy(array, index, newArray, 0, lenght);
            return newArray;
        }
    }
}
