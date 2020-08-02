using GrimBuilding.Common.Support;
using GrimBuilding.DBGenerator.Support;
using LiteDB;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator
{
    class Program
    {
        private const string DatabaseFileName = "data.db";

        static IEnumerable<string> GetExpansionPaths(string gdBasePath, string[] expansionPaths, string relativePath)
        {
            yield return Path.Combine(gdBasePath, relativePath);
            foreach (var expansionPath in expansionPaths)
                yield return Path.Combine(expansionPath, relativePath);
        }

        public static async Task<PlayerClass[]> GetPlayerClassesAsync(string gdDbPath, TagParser skillTags)
        {
            var dirs = Directory.EnumerateDirectories(Path.Combine(gdDbPath, @"database\records\ui\skills")).Where(path => Regex.IsMatch(path, @"[/\\]class\d+$")).ToArray();
            var result = new PlayerClass[dirs.Length];

            await Task.WhenAll(dirs.Select(async (dir, idx) =>
            {
                var dbr = await DbrParser.FromPathAsync(Path.Combine(dir, "classtable.dbr")).ConfigureAwait(false);

                var rawSkills = (await Task.WhenAll((await Task.WhenAll(dbr.GetStringValues("tabSkillButtons").Select(skillPath => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", skillPath)))).ConfigureAwait(false))
                    .Select(uiSkillDbr => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", uiSkillDbr.GetStringValue("skillName"))))).ConfigureAwait(false)).ToList();

                bool changes;
                do
                {
                    changes = false;

                    var buffSkills = await Task.WhenAll(rawSkills
                        .Where(skill => !skill.ContainsKey("skillDisplayName") && skill.ContainsKey("buffSkillName"))
                        .Select(skill => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", skill.GetStringValue("buffSkillName"))))).ConfigureAwait(false);

                    var petSkills = await Task.WhenAll(rawSkills
                        .Where(skill => !skill.ContainsKey("skillDisplayName") && skill.ContainsKey("petSkillName"))
                        .Select(skill => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", skill.GetStringValue("petSkillName"))))).ConfigureAwait(false);

                    changes = buffSkills.Any() || petSkills.Any();

                    if (changes)
                    {
                        rawSkills.RemoveAll(skill => !skill.ContainsKey("skillDisplayName"));
                        rawSkills.AddRange(buffSkills);
                        rawSkills.AddRange(petSkills);
                    }
                } while (changes);

                result[idx] = new PlayerClass
                {
                    Name = skillTags[dbr.GetStringValue("skillTabTitle")],
                    Skills = rawSkills.Select(skillDbr => new PlayerSkill
                    {
                        Name = skillTags[skillDbr.GetStringValue("skillDisplayName")],
                        BitmapUpPath = skillDbr.GetStringValue("skillUpBitmapName"),
                        BitmapDownPath = skillDbr.GetStringValue("skillDownBitmapName")
                    }).ToArray(),
                };

                await Task.WhenAll(result[idx].Skills.Select(async skill =>
                {
                    (skill.BitmapUp, skill.BitmapUpPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapUpPath).ConfigureAwait(false);
                    (skill.BitmapDown, skill.BitmapDownPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapDownPath).ConfigureAwait(false);
                })).ConfigureAwait(false);
            })).ConfigureAwait(false);

            return result;
        }
        
        static async Task Main(string[] args)
        {
            var expansionPaths = Directory.GetDirectories(args[0], "gdx*");
            var skillTags = await TagParser.FromArcFilesAsync(GetExpansionPaths(args[0], expansionPaths, @"resources\text_en.arc")).ConfigureAwait(false);
            var classes = await GetPlayerClassesAsync(args[0], skillTags).ConfigureAwait(false);

            var mapper = new BsonMapper();
            mapper.Entity<PlayerSkill>().Ignore(s => s.BitmapDown).Ignore(s => s.BitmapUp);

            try { File.Delete(DatabaseFileName); } catch { }

            using var db = new LiteDatabase(DatabaseFileName, mapper);
            var collection = db.GetCollection<PlayerClass>();
            collection.InsertBulk(classes);
            collection.EnsureIndex(c => c.Name);

            classes.SelectMany(c => c.Skills).ForEach(s =>
            {
                using (var stream = new MemoryStream(s.BitmapDown))
                    db.FileStorage.Upload(s.BitmapDownPath, s.BitmapDownPath, stream);
                using (var stream = new MemoryStream(s.BitmapUp))
                    db.FileStorage.Upload(s.BitmapUpPath, s.BitmapUpPath, stream);
            });
        }
    }
}
