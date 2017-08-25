using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS
{
    public static class GZip
    {
        public static byte[] Compress(byte[] inputData, int index, int count)
        {
            if (inputData == null)
                throw new ArgumentNullException(nameof(inputData), "must be non-null");

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BinaryWriter(new GZipStream(compressIntoMs, CompressionMode.Compress)))
                {
                    gzs.Write(inputData, index, count);
                }
                return compressIntoMs.ToArray();
            }
        }

        public static byte[] Decompress(byte[] inputData, int index, int count)
        {
            if (inputData == null)
                throw new ArgumentNullException(nameof(inputData), " must be non-null");

            using (var compressedMs = new MemoryStream(inputData, index, count))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BinaryReader(new GZipStream(compressedMs, CompressionMode.Decompress)))
                    {
                        byte[] chunk = gzs.ReadBytes(1024);
                        while (chunk.Length > 0)
                        {
                            decompressedMs.Write(chunk, 0, chunk.Length);
                            chunk = gzs.ReadBytes(1024);
                        }
                    }
                    return decompressedMs.ToArray();
                }
            }
        }
    }
}
