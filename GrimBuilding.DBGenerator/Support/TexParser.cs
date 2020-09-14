using GrimBuilding.Codecs;
using Nito.AsyncEx;
using Pfim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator.Support
{
    class TexParser
    {
        static readonly AsyncMonitor sync = new();
        static readonly Dictionary<string, byte[]> cache = new();
        static readonly Dictionary<string, bool> cacheInit = new();

        static int fileCount;
        public static int FileCount => fileCount;

        public static async Task<(byte[] bytes, string newPath)> ExtractWebP(string resPath, string relativePath)
        {
            Interlocked.Increment(ref fileCount);

            var release = await sync.EnterAsync();

            var newFileName = Path.ChangeExtension(relativePath, "webp");
            if (cache.TryGetValue(newFileName, out var bytes))
            {
                release.Dispose();
                return (bytes, newFileName);
            }

            // not in the dictionary, take ownership of initializing this element
            byte[] encodedBytes = default;
            if (!cacheInit.TryGetValue(newFileName, out var init) || !init)
            {
                // release the lock
                cacheInit[newFileName] = true;
                release.Dispose();

                // do the work
                await Task.Run(() =>
                {
                    using var stream = File.OpenRead(Path.Combine(resPath, relativePath));
                    stream.Position = 8;            // skip the header, version and fps

                    int frameLength = 0;
                    stream.Read(MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateSpan(ref frameLength, 1)));

                    var ddsBuffer = new byte[frameLength];
                    stream.Read(ddsBuffer);
                    ddsBuffer[3] = 0x20;

                    using var ddsImage = Dds.Create(ddsBuffer, new PfimConfig());

                    if (ddsImage.Compressed)
                        ddsImage.Decompress();

                    var pixels = ddsImage.Width * ddsImage.Height;
                    var quality = pixels > 1200 * 1200 ? 75
                        : pixels > 600 * 600 ? 85
                        : 95;

                    switch (ddsImage.Format)
                    {
                        case ImageFormat.Rgba32:
                            WebP.EncodeRGBA(ddsImage.Data, ddsImage.Width, ddsImage.Height, ddsImage.Stride, quality, out encodedBytes);
                            break;
                        case ImageFormat.Rgb24:
                            WebP.EncodeRGB(ddsImage.Data, ddsImage.Width, ddsImage.Height, ddsImage.Stride, quality, out encodedBytes);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }).ConfigureAwait(false);

                if (encodedBytes is null)
                    throw new InvalidOperationException();

                // reacquire the lock and complete the load
                using (await sync.EnterAsync())
                {
                    cache[newFileName] = encodedBytes;
                    cacheInit[newFileName] = false;
                }

                // return the values
                return (cache[newFileName], newFileName);
            }
            else
            {
                // wait for someone else to finish the load
                while (true)
                {
                    release.Dispose();
                    await Task.Delay(TimeSpan.Zero).ConfigureAwait(false);

                    release = await sync.EnterAsync();

                    init = cacheInit[newFileName];
                    if (!init) break;
                }

                // release the lock again and return the values
                release.Dispose();
                return (cache[newFileName], newFileName);
            }
        }
    }
}