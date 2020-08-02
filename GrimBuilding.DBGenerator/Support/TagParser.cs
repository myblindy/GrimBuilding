using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator.Support
{
    class TagParser
    {
        readonly Dictionary<string, string> tags = new Dictionary<string, string>();

        private TagParser() { }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ArcHeader
        {
            public const uint MagicValue = 4411969; // ARC\0

            public uint Magic;
            public uint Version;
            public uint NumberOfFileEntries;
            public uint NumberOfDataRecords; // (NumberOfDataRecords / 12) = RecordTableSize
            public uint RecordTableSize;
            public uint StringTableSize;
            public uint RecordTableOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ArcToc
        {
            public uint EntryType;
            public uint FileOffset;
            public uint CompressedSize;
            public uint DecompressedSize;
            public uint DecompressedHash; // Adler32 hash of the decompressed file bytes
            public ulong FileTime;
            public uint FileParts;
            public uint FirstPartIndex;
            public uint StringEntryLength;
            public uint StringEntryOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ArcFilePart
        {
            public uint PartOffset;
            public uint CompressedSize;
            public uint DecompressedSize;
        }

        public static async Task<TagParser> FromArcFilesAsync(IEnumerable<string> paths)
        {
            var nameBuffer = new StringBuilder();
            var result = new TagParser();

            async Task ProcessFile(byte[] buffer)
            {
                using var reader = new StreamReader(new MemoryStream(buffer));

                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    int sepIdx = line.IndexOf('=');
                    if (sepIdx >= 0)
                        result.tags[line[..sepIdx]] = line[(sepIdx + 1)..];
                }
            }

            foreach (var path in paths)
            {
                using var reader = File.OpenRead(path);

                ArcHeader header = default;
                reader.Read(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1)));

                if (header.Magic == ArcHeader.MagicValue)
                {
                    for (int fileIndex = 0; fileIndex < header.NumberOfFileEntries; ++fileIndex)
                    {
                        reader.Position = header.RecordTableOffset + header.RecordTableSize + header.StringTableSize + fileIndex * 44;
                        ArcToc toc = default;
                        reader.Read(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref toc, 1)));

                        reader.Position = header.RecordTableOffset + header.RecordTableSize + toc.StringEntryOffset;

                        nameBuffer.Clear();
                        while (true)
                        {
                            var ch = reader.ReadByte();
                            if (ch == 0) break;
                            nameBuffer.Append((char)ch);
                        }
                        var fileName = nameBuffer.ToString();

                        if (!Regex.IsMatch(fileName, @"^tags\w*_.*\.txt"))
                            continue;

                        if (toc.EntryType == 1 && toc.CompressedSize == toc.DecompressedSize)
                        {
                            reader.Position = toc.FileOffset;
                            var buffer = new byte[toc.DecompressedSize];
                            await reader.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
                            await ProcessFile(buffer).ConfigureAwait(false);
                        }
                        else
                        {
                            var bufferList = new List<byte[]>((int)toc.FileParts);

                            for (int partIdx = 0; partIdx < toc.FileParts; ++partIdx)
                            {
                                reader.Position = header.RecordTableOffset + (toc.FirstPartIndex + partIdx) * Marshal.SizeOf<ArcFilePart>();
                                ArcFilePart part = default;
                                reader.Read(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref part, 1)));

                                if (part.CompressedSize == part.DecompressedSize)
                                {
                                    var buffer = new byte[part.DecompressedSize];
                                    reader.Position = part.PartOffset;
                                    await reader.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
                                    bufferList.Add(buffer);
                                }
                                else
                                {
                                    var inputBuffer = new byte[part.CompressedSize];
                                    reader.Position = part.PartOffset;
                                    await reader.ReadAsync(inputBuffer.AsMemory()).ConfigureAwait(false);

                                    var outputBuffer = new byte[part.DecompressedSize];

                                    LZ4Codec.Decode(inputBuffer, outputBuffer);
                                    bufferList.Add(outputBuffer);
                                }
                            }

                            if (toc.FileParts == 1)
                                await ProcessFile(bufferList[0]).ConfigureAwait(false);
                            else
                            {
                                var fullBuffer = new byte[bufferList.Sum(b => b.Length)];
                                for (int idx = 0; idx < bufferList.Count; ++idx)
                                    Buffer.BlockCopy(bufferList[idx], 0, fullBuffer, bufferList.Take(idx).Sum(b => b.Length), bufferList[idx].Length);
                                await ProcessFile(fullBuffer).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public string this[string key] => tags[key];
    }
}