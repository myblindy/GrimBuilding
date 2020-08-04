using Nito.AsyncEx;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
        static readonly AsyncMonitor sync = new AsyncMonitor();
        static readonly Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();
        static readonly Dictionary<string, bool> cacheInit = new Dictionary<string, bool>();

        public static async Task<(byte[] bytes, string newPath)> ExtractPng(string resPath, string relativePath)
        {
            var release = await sync.EnterAsync();

            var newFileName = Path.ChangeExtension(relativePath, "png");
            if (cache.TryGetValue(newFileName, out var bytes))
            {
                release.Dispose();
                return (bytes, newFileName);
            }

            // not in the dictionary, take ownership of initializing this element
            if (!cacheInit.TryGetValue(newFileName, out var init) || !init)
            {
                // release the lock
                cacheInit[newFileName] = true;
                release.Dispose();

                // do the work
                using var stream = File.OpenRead(Path.Combine(resPath, relativePath));
                stream.Position = 8;            // skip the header, version and fps

                int frameLength = 0;
                stream.Read(MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateSpan(ref frameLength, 1)));

                var ddsBuffer = new byte[frameLength];
                stream.Read(ddsBuffer);
                ddsBuffer[3] = 0x20;

                using var memOutput = new MemoryStream();
                using var ddsImage = Dds.Create(ddsBuffer, new PfimConfig());

                if (ddsImage.Compressed)
                    ddsImage.Decompress();

                switch (ddsImage.Format)
                {
                    case ImageFormat.Rgba32:
                        Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height).SaveAsPng(memOutput);
                        break;
                    case ImageFormat.Rgb24:
                        Image.LoadPixelData<Bgr24>(ddsImage.Data, ddsImage.Width, ddsImage.Height).SaveAsPng(memOutput);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // reacquire the lock and complete the load
                using (await sync.EnterAsync())
                {
                    cache[newFileName] = memOutput.ToArray();
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