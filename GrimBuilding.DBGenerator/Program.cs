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

                var uiSkills = (await Task.WhenAll(dbr.GetStringValues("tabSkillButtons").Select(skillPath => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", skillPath)))).ConfigureAwait(false)).ToList();
                var rawSkills = (await Task.WhenAll(uiSkills.Select(uiSkillDbr => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", uiSkillDbr.GetStringValue("skillName"))))).ConfigureAwait(false)).ToList();

                bool changes;
                do
                {
                    changes = false;

                    var buffSkillsToChange = rawSkills.Index().Where(skill => !skill.Value.ContainsKey("skillDisplayName") && skill.Value.ContainsKey("buffSkillName")).ToList();
                    var buffSkills = await Task.WhenAll(buffSkillsToChange
                        .Select(skill => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", skill.Value.GetStringValue("buffSkillName"))))).ConfigureAwait(false);

                    var petSkillsToChange = rawSkills.Index().Where(skill => !skill.Value.ContainsKey("skillDisplayName") && skill.Value.ContainsKey("petSkillName")).ToList();
                    var petSkills = await Task.WhenAll(petSkillsToChange
                        .Select(skill => DbrParser.FromPathAsync(Path.Combine(gdDbPath, "database", skill.Value.GetStringValue("petSkillName"))))).ConfigureAwait(false);

                    changes = buffSkills.Any() || petSkills.Any();

                    foreach (var (buffSkill, buffSkillOriginalIndex) in buffSkills.Zip(buffSkillsToChange.Select(w => w.Key), (buffSkill, idx) => (buffSkill, idx)))
                        rawSkills[buffSkillOriginalIndex] = buffSkill;

                    foreach (var (buffSkill, petSkillOriginalIndex) in petSkills.Zip(petSkillsToChange.Select(w => w.Key), (petSkill, idx) => (petSkill, idx)))
                        rawSkills[petSkillOriginalIndex] = buffSkill;

                } while (changes);

                result[idx] = new PlayerClass
                {
                    Name = skillTags[dbr.GetStringValue("skillTabTitle")],
                    Skills = rawSkills.Zip(uiSkills, (raw, ui) => (raw, ui))
                        .Select(w => new PlayerSkill
                        {
                            Name = skillTags[w.raw.GetStringValue("skillDisplayName")],
                            Description = skillTags[w.raw.GetStringValue("skillBaseDescription")],
                            BitmapUpPath = w.raw.GetStringValue("skillUpBitmapName"),
                            BitmapDownPath = w.raw.GetStringValue("skillDownBitmapName"),
                            BitmapFrameDownPath = w.ui.TryGetStringValue("bitmapNameDown", 0, out var frameDownValue) ? frameDownValue : null,
                            BitmapFrameUpPath = w.ui.TryGetStringValue("bitmapNameUp", 0, out var frameUpValue) ? frameUpValue : null,
                            BitmapFrameInFocusPath = w.ui.TryGetStringValue("bitmapNameInFocus", 0, out var frameInFocusValue) ? frameInFocusValue : null,
                            MaximumLevel = (int)w.raw.GetDoubleValue("skillMaxLevel"),
                            UltimateLevel = w.raw.TryGetDoubleValue("skillUltimateLevel", 0, out var ultimateLevel)
                                ? (int?)ultimateLevel : null,
                            MasteryLevelRequirement = w.raw.TryGetDoubleValue("skillMasteryLevelRequired", 0, out var masteryLevel)
                                ? (int?)masteryLevel : null,
                            Circular = Convert.ToBoolean(w.ui.GetDoubleValue("isCircular")),
                            PositionX = (int)w.ui.GetDoubleValue("bitmapPositionX"),
                            PositionY = (int)w.ui.GetDoubleValue("bitmapPositionY"),
                            BitmapSkillConnectionOffPaths = w.raw.GetStringValues("skillConnectionOff")?.ToArray() ?? Array.Empty<string>(),
                        }).ToArray(),
                };

                await Task.WhenAll(result[idx].Skills.Select(async skill =>
                {
                    (skill.BitmapUp, skill.BitmapUpPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapUpPath).ConfigureAwait(false);
                    (skill.BitmapDown, skill.BitmapDownPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapDownPath).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(skill.BitmapFrameDownPath))
                        (skill.BitmapFrameDown, skill.BitmapFrameDownPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapFrameDownPath).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(skill.BitmapFrameUpPath))
                        (skill.BitmapFrameUp, skill.BitmapFrameUpPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapFrameUpPath).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(skill.BitmapFrameInFocusPath))
                        (skill.BitmapFrameInFocus, skill.BitmapFrameInFocusPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapFrameInFocusPath).ConfigureAwait(false);

                    skill.BitmapSkillConnectionsOff = new byte[skill.BitmapSkillConnectionOffPaths.Length][];
                    for (int idx = 0; idx < skill.BitmapSkillConnectionOffPaths.Length; ++idx)
                        if (!string.IsNullOrWhiteSpace(skill.BitmapSkillConnectionOffPaths[idx]))
                            (skill.BitmapSkillConnectionsOff[idx], skill.BitmapSkillConnectionOffPaths[idx]) =
                                await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapSkillConnectionOffPaths[idx]).ConfigureAwait(false);
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

            var dbFullFileName = Path.Combine(args[1], DatabaseFileName);
            try { File.Delete(dbFullFileName); } catch { }

            using var db = new LiteDatabase(dbFullFileName, mapper);

            var skillCollection = db.GetCollection<PlayerSkill>();
            skillCollection.InsertBulk(classes.SelectMany(c => c.Skills));
            skillCollection.EnsureIndex(c => c.Name);

            var classCollection = db.GetCollection<PlayerClass>();
            classCollection.InsertBulk(classes);
            classCollection.EnsureIndex(c => c.Name);

            var filesUploaded = new HashSet<string>();

            classes.SelectMany(c => c.Skills).ForEach(s =>
            {
                void Upload(string path, byte[] data)
                {
                    if (!(data is null) && filesUploaded.Add(path))
                        using (var stream = new MemoryStream(data))
                            db.FileStorage.Upload(path, path, stream);
                }

                Upload(s.BitmapDownPath, s.BitmapDown);
                Upload(s.BitmapUpPath, s.BitmapUp);
                Upload(s.BitmapFrameDownPath, s.BitmapFrameDown);
                Upload(s.BitmapFrameUpPath, s.BitmapFrameUp);
                Upload(s.BitmapFrameInFocusPath, s.BitmapFrameInFocus);
                for (int idx = 0; idx < s.BitmapSkillConnectionOffPaths.Length; ++idx)
                    Upload(s.BitmapSkillConnectionOffPaths[idx], s.BitmapSkillConnectionsOff[idx]);
            });
        }
    }
}
