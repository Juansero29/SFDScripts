using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScriptInjector.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToSizePrefixedByteArray(this string newScriptTextInBase64)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(newScriptTextInBase64);
            return stream.ToArray();
        }
    }
}
