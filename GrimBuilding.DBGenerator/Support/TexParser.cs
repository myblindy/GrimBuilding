using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator.Support
{
    class TexParser
    {
        public static async Task<(byte[] bytes, string newPath)> ExtractPng(string resPath, string relativePath)
        {
            using var stream = File.OpenRead(Path.Combine(resPath, relativePath));
            stream.Position = 8;            // skip the header, version and fps

            int frameLength = 0;
            stream.Read(MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateSpan(ref frameLength, 1)));

            var ddsBuffer = new byte[frameLength];
            stream.Read(ddsBuffer);
            ddsBuffer[3] = 0x20;

            using var memOutput = new MemoryStream();
            using var ddsImage = Dds.Create(ddsBuffer, new PfimConfig());

            await Task.Run(() =>
            {
                if (ddsImage.Compressed)
                    ddsImage.Decompress();

                switch (ddsImage.Format)
                {
                    case Pfim.ImageFormat.Rgba32:
                        Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height).SaveAsPng(memOutput);
                        break;
                    case Pfim.ImageFormat.Rgb24:
                        Image.LoadPixelData<Bgr24>(ddsImage.Data, ddsImage.Width, ddsImage.Height).SaveAsPng(memOutput);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }).ConfigureAwait(false);

            return (memOutput.ToArray(), Path.ChangeExtension(relativePath, "png"));
        }
    }
}