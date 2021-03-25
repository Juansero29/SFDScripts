using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScriptInjector.Extensions
{
    public static class ByteExtensions
    {
        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Array.Empty<int>();

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }
            return list.Count == 0 ? Array.Empty<int>() : list.ToArray();
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }


        public static byte[] CombineByteArrays(params byte[][] byteArraysToCombine)
        {
            if (byteArraysToCombine == null || byteArraysToCombine.Length <= 0) return Array.Empty<byte>();

            var finalByteArraySize = default(int);
            for (int i = 0; i < byteArraysToCombine.Length; i++)
            {
                finalByteArraySize += byteArraysToCombine[i].Length;
            }

            var finalByteArray = new byte[finalByteArraySize];
            var lastArraySize = default(int);
            for (int i = 0; i < byteArraysToCombine.Length; i++)
            {
                var currentArray = byteArraysToCombine[i];
                var currentArraySize = currentArray.Length;
                Array.Copy(currentArray, 0, finalByteArray, lastArraySize, currentArraySize);
                lastArraySize += currentArraySize;
            }
            return finalByteArray;
        }

    }
}
