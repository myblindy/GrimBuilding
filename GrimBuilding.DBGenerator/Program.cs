using GrimBuilding.DBGenerator.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator
{
    class Program
    {
        static IEnumerable<string> GetExpansionPaths(string gdBasePath, string[] expansionPaths, string relativePath)
        {
            yield return Path.Combine(gdBasePath, relativePath);
            foreach (var expansionPath in expansionPaths)
                yield return Path.Combine(expansionPath, relativePath);
        }

        static async Task Main(string[] args)
        {
            var expansionPaths = Directory.GetDirectories(args[0], "gdx*");
            var skillTags = await TagParser.FromArcFilesAsync(GetExpansionPaths(args[0], expansionPaths, @"resources\text_en.arc")).ConfigureAwait(false);
            var classes = await PlayerClass.GetPlayerClassesAsync(args[0], skillTags).ConfigureAwait(false);
        }
    }
}
