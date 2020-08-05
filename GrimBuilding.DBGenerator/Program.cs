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

        static readonly string[] navigationProperties = new[] { "buffSkillName", "petSkillName" };

        public static async Task<(PlayerAffinity[] affinities, PlayerConstellation[] constellations)> GetPlayerConstellationsAsync(string gdDbPath, TagParser skillTags)
        {
            var devotionMasterList = await DbrParser.FromPathAsync(gdDbPath, "database", @"records\ui\skills\devotion\devotion_mastertable.dbr").ConfigureAwait(false);

            var affinities = (await Task.WhenAll(devotionMasterList.GetAllStringsOfFormat("affinity{0:00}Rollover")
                .Select(async kvp => await DbrParser.FromPathAsync(gdDbPath, "database", kvp.values.First()).ConfigureAwait(false))).ConfigureAwait(false))
                .Select(parser => new PlayerAffinity
                {
                    Name = skillTags[parser.GetStringValue("Line1Tag")],
                    Description = skillTags[parser.GetStringValue("Line2Tag")],
                })
                .ToArray();

            var constellations = await Task.WhenAll((await Task.WhenAll(devotionMasterList.GetAllStringsOfFormat("devotionConstellation{0}")
                    .Select(async kvpConstellation => await DbrParser.FromPathAsync(gdDbPath, "database", kvpConstellation.values.First(), navigationProperties).ConfigureAwait(false))).ConfigureAwait(false))
                .Select(async constellation =>
                {
                    var backgroundImageParser = constellation.ContainsKey("constellationBackground")
                        ? await DbrParser.FromPathAsync(gdDbPath, "database", constellation.GetStringValue("constellationBackground")).ConfigureAwait(false)
                        : null;

                    return new PlayerConstellation
                    {
                        Name = skillTags[constellation.GetStringValue("constellationDisplayTag")],
                        Description = skillTags[constellation.GetStringValue("constellationInfoTag")],

                        BitmapPath = backgroundImageParser?.GetStringValue("bitmapName"),
                        PositionX = backgroundImageParser == null ? 0 : (int)backgroundImageParser.GetDoubleValue("bitmapPositionX"),
                        PositionY = backgroundImageParser == null ? 0 : (int)backgroundImageParser.GetDoubleValue("bitmapPositionY"),

                        Skills = (await Task.WhenAll((await Task.WhenAll(constellation.GetAllStringsOfFormat("devotionButton{0}").Select(async kvpDevotion => await DbrParser.FromPathAsync(gdDbPath, "database", kvpDevotion.values.First(), navigationProperties).ConfigureAwait(false))).ConfigureAwait(false))
                                .Select(async uiParser => (uiParser, baseSkillParser: await DbrParser.FromPathAsync(gdDbPath, "database", uiParser.GetStringValue("skillName"), navigationProperties).ConfigureAwait(false)))).ConfigureAwait(false))
                            .Select(parsers => new PlayerSkill
                            {
                                Name = skillTags[parsers.baseSkillParser.GetStringValue("skillDisplayName")],
                                PositionX = (int)parsers.uiParser.GetDoubleValue("bitmapPositionX"),
                                PositionY = (int)parsers.uiParser.GetDoubleValue("bitmapPositionY"),
                                BitmapFrameDownPath = parsers.uiParser.TryGetStringValue("bitmapNameDown", 0, out var frameDownValue) ? frameDownValue : null,
                                BitmapFrameUpPath = parsers.uiParser.TryGetStringValue("bitmapNameUp", 0, out var frameUpValue) ? frameUpValue : null,
                                BitmapFrameInFocusPath = parsers.uiParser.TryGetStringValue("bitmapNameInFocus", 0, out var frameInFocusValue) ? frameInFocusValue : null,
                            })
                            .ToArray(),

                        RequiredAffinities = constellation.GetAllStringsOfFormat("affinityRequiredName{0}")
                            .Select(kvpRequired => (affinities.First(a => a.Name == kvpRequired.values.First()), quantity: (int)constellation.GetDoubleValue($"{kvpRequired.key[..^5]}{kvpRequired.key[^1]}")))
                            .Where(w => w.quantity > 0)
                            .ToArray(),
                        RewardedAffinities = constellation.GetAllStringsOfFormat("affinityGivenName{0}")
                            .Select(kvpRewarded => (affinities.First(a => a.Name == kvpRewarded.values.First()), quantity: (int)constellation.GetDoubleValue($"{kvpRewarded.key[..^5]}{kvpRewarded.key[^1]}")))
                            .Where(w => w.quantity > 0)
                            .ToArray(),
                    };
                })).ConfigureAwait(false);

            await Task.WhenAll(constellations.SelectMany(c => c.Skills.Select(async skill =>
              {
                  if (!string.IsNullOrWhiteSpace(skill.BitmapFrameDownPath))
                      (skill.BitmapFrameDown, skill.BitmapFrameDownPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapFrameDownPath).ConfigureAwait(false);
                  if (!string.IsNullOrWhiteSpace(skill.BitmapFrameUpPath))
                      (skill.BitmapFrameUp, skill.BitmapFrameUpPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapFrameUpPath).ConfigureAwait(false);
                  if (!string.IsNullOrWhiteSpace(skill.BitmapFrameInFocusPath))
                      (skill.BitmapFrameInFocus, skill.BitmapFrameInFocusPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), skill.BitmapFrameInFocusPath).ConfigureAwait(false);
              })).Concat(constellations.Select(async constellation =>
              {
                  if (!string.IsNullOrWhiteSpace(constellation.BitmapPath))
                      (constellation.Bitmap, constellation.BitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), constellation.BitmapPath).ConfigureAwait(false);
              }))).ConfigureAwait(false);

            return (affinities, constellations);
        }

        public static async Task<PlayerClass[]> GetPlayerClassesAsync(string gdDbPath, TagParser skillTags)
        {
            var dirs = Directory.EnumerateDirectories(Path.Combine(gdDbPath, @"database\records\ui\skills")).Where(path => Regex.IsMatch(path, @"[/\\]class\d+$")).ToArray();
            var result = new PlayerClass[dirs.Length];

            await Task.WhenAll(dirs.Select(async (dir, idx) =>
            {
                var masterClassList = await DbrParser.FromPathAsync(dir, "", "classtable.dbr").ConfigureAwait(false);

                var uiSkills = (await Task.WhenAll(masterClassList.GetStringValues("tabSkillButtons").Select(skillPath => DbrParser.FromPathAsync(gdDbPath, "database", skillPath))).ConfigureAwait(false)).ToList();
                var rawSkills = (await Task.WhenAll(uiSkills.Select(uiSkillDbr => DbrParser.FromPathAsync(gdDbPath, "database", uiSkillDbr.GetStringValue("skillName"), navigationProperties))).ConfigureAwait(false)).ToList();

                result[idx] = new PlayerClass
                {
                    Name = skillTags[masterClassList.GetStringValue("skillTabTitle")],
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
            var (affinities, constellations) = await GetPlayerConstellationsAsync(args[0], skillTags).ConfigureAwait(false);

            var mapper = new BsonMapper();

            var dbFullFileName = Path.Combine(args[1], DatabaseFileName);
            try { File.Delete(dbFullFileName); } catch { }

            using var db = new LiteDatabase(dbFullFileName, mapper);

            var affinityCollection = db.GetCollection<PlayerAffinity>();
            affinityCollection.InsertBulk(affinities);
            affinityCollection.EnsureIndex(a => a.Name);

            var skillCollection = db.GetCollection<PlayerSkill>();
            skillCollection.InsertBulk(classes.SelectMany(c => c.Skills).Concat(constellations.SelectMany(c => c.Skills)));
            skillCollection.EnsureIndex(c => c.Name);

            var classCollection = db.GetCollection<PlayerClass>();
            classCollection.InsertBulk(classes);
            classCollection.EnsureIndex(c => c.Name);

            var constellationCollection = db.GetCollection<PlayerConstellation>();
            constellationCollection.InsertBulk(constellations);
            constellationCollection.EnsureIndex(c => c.Name);

            var filesUploaded = new HashSet<string>();

            void Upload(string path, byte[] data)
            {
                if (!(data is null) && filesUploaded.Add(path))
                    using (var stream = new MemoryStream(data))
                        db.FileStorage.Upload(path, path, stream);
            }

            classes.SelectMany(c => c.Skills).Concat(constellations.SelectMany(c => c.Skills))
                .ForEach(s =>
                {
                    Upload(s.BitmapDownPath, s.BitmapDown);
                    Upload(s.BitmapUpPath, s.BitmapUp);
                    Upload(s.BitmapFrameDownPath, s.BitmapFrameDown);
                    Upload(s.BitmapFrameUpPath, s.BitmapFrameUp);
                    Upload(s.BitmapFrameInFocusPath, s.BitmapFrameInFocus);
                    if (!(s.BitmapSkillConnectionOffPaths is null))
                        for (int idx = 0; idx < s.BitmapSkillConnectionOffPaths.Length; ++idx)
                            Upload(s.BitmapSkillConnectionOffPaths[idx], s.BitmapSkillConnectionsOff[idx]);
                });
            constellations.ForEach(c => Upload(c.BitmapPath, c.Bitmap));
        }
    }
}
