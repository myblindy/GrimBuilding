using GrimBuilding.Common.Support;
using GrimBuilding.DBGenerator.Support;
using LiteDB;
using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        static async Task<(PlayerAffinity[] affinities, PlayerConstellation[] constellations, PlayerConstellationNebula[] nebulas)> GetPlayerConstellationsAsync(string gdDbPath, TagParser skillTags)
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

                        SkillRequirements = constellation.GetAllDoublesOfFormat("devotionLinks{0}", 2).Select(w => (int)w.values.First()).ToArray(),

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
                            .Select(kvpRequired => new PlayerAffinityQuantity
                            {
                                Type = affinities.First(a => a.Name == kvpRequired.values.First()),
                                Quantity = (int)constellation.GetDoubleValue($"{kvpRequired.key[..^5]}{kvpRequired.key[^1]}")
                            })
                            .Where(w => w.Quantity > 0)
                            .ToArray(),

                        RewardedAffinities = constellation.GetAllStringsOfFormat("affinityGivenName{0}")
                            .Select(kvpRewarded => new PlayerAffinityQuantity
                            {
                                Type = affinities.First(a => a.Name == kvpRewarded.values.First()),
                                Quantity = (int)constellation.GetDoubleValue($"{kvpRewarded.key[..^5]}{kvpRewarded.key[^1]}")
                            })
                            .Where(w => w.Quantity > 0)
                            .ToArray(),
                    };
                })).ConfigureAwait(false);

            var nebulas = (await Task.WhenAll(devotionMasterList.GetStringValues("nebulaSections").Select(async nebulaPath => await DbrParser.FromPathAsync(gdDbPath, "database", nebulaPath).ConfigureAwait(false))).ConfigureAwait(false))
                .Select(parser => new PlayerConstellationNebula
                {
                    BitmapPath = parser.GetStringValue("bitmapName"),
                    PositionX = (int)parser.GetDoubleValue("bitmapPositionX"),
                    PositionY = (int)parser.GetDoubleValue("bitmapPositionY"),
                })
                .ToArray();

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
                }).Concat(nebulas.Select(async nebula =>
                    {
                        (nebula.Bitmap, nebula.BitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), nebula.BitmapPath).ConfigureAwait(false);
                    })))).ConfigureAwait(false);

            return (affinities, constellations, nebulas);
        }

        static async Task<(PlayerClass[], ConcurrentDictionary<string, PlayerSkill>)> GetPlayerClassesAsync(string gdDbPath, TagParser skillTags)
        {
            var dirs = Directory.EnumerateDirectories(Path.Combine(gdDbPath, @"database\records\ui\skills")).Where(path => Regex.IsMatch(path, @"[/\\]class\d+$")).ToArray();
            var playerClass = new PlayerClass[dirs.Length];
            var skillDictionary = new ConcurrentDictionary<string, PlayerSkill>();

            await Task.WhenAll(dirs.Select(async (dir, idx) =>
            {
                var masterClassList = await DbrParser.FromPathAsync(dir, "", "classtable.dbr").ConfigureAwait(false);

                var uiSkills = (await Task.WhenAll(masterClassList.GetStringValues("tabSkillButtons").Select(skillPath => DbrParser.FromPathAsync(gdDbPath, "database", skillPath))).ConfigureAwait(false)).ToList();
                var originalRawSkillsPaths = uiSkills.Select(uiSkillDbr => uiSkillDbr.GetStringValue("skillName")).ToList();
                var rawSkills = (await Task.WhenAll(uiSkills.Select(uiSkillDbr => DbrParser.FromPathAsync(gdDbPath, "database", uiSkillDbr.GetStringValue("skillName"), navigationProperties))).ConfigureAwait(false)).ToList();

                playerClass[idx] = new PlayerClass
                {
                    Name = skillTags[masterClassList.GetStringValue("skillTabTitle")],
                    BitmapPath = (await DbrParser.FromPathAsync(gdDbPath, "database", masterClassList.GetStringValue("skillPaneMasteryBitmap")).ConfigureAwait(false))
                        .GetStringValue("bitmapName"),
                    Skills = rawSkills.Zip(uiSkills, (raw, ui) => (raw, ui)).Zip(originalRawSkillsPaths, (w, originalPath) => (w.raw, w.ui, originalPath))
                        .Select(w =>
                        {
                            var skill = new PlayerSkill
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
                            };

                            var baseStats = new List<BaseStats>();
                            for (int i = 0; ; ++i)
                            {
                                var baseStatsInstance = new BaseStats();
                                if (!LoadBaseStats(baseStatsInstance, w.raw, i))
                                    break;
                                baseStats.Add(baseStatsInstance);
                            }
                            skill.BaseStatLevels = baseStats.ToArray();

                            return skillDictionary[w.originalPath] = skill;
                        }).ToArray(),
                };

                await Task.WhenAll(playerClass[idx].Skills
                    .Select(async skill =>
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

                (playerClass[idx].Bitmap, playerClass[idx].BitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), playerClass[idx].BitmapPath).ConfigureAwait(false);

            })).ConfigureAwait(false);

            return (playerClass, skillDictionary);
        }

        static bool LoadBaseStats(BaseStats baseStats, DbrParser dbr, int index = 0)
        {
            var attributeScalePercent = dbr.GetDoubleValueOrDefault("attributeScalePercent") / 100.0;
            baseStats.AttributeScalePercent = attributeScalePercent;

            bool anyChange = false;
            anyChange |= (baseStats.LevelRequirement = (int)dbr.GetDoubleValueOrDefault("levelRequirement", index)) != 0;
            anyChange |= (baseStats.PhysiqueRequirement = dbr.GetDoubleValueOrDefault("strengthRequirement", index)) != 0;
            anyChange |= (baseStats.CunningRequirement = dbr.GetDoubleValueOrDefault("dexterityRequirement", index)) != 0;
            anyChange |= (baseStats.SpiritRequirement = dbr.GetDoubleValueOrDefault("intelligenceRequirement", index)) != 0;
            anyChange |= (baseStats.Life = dbr.GetDoubleValueOrDefault("characterLife", index)) != 0;
            anyChange |= (baseStats.LifeModifier = dbr.GetDoubleValueOrDefault("characterLifeModifier", index)) != 0;
            anyChange |= (baseStats.LifeRegeneration = dbr.GetDoubleValueOrDefault("characterLifeRegen", index)) != 0;
            anyChange |= (baseStats.LifeRegenerationModifier = dbr.GetDoubleValueOrDefault("characterLifeRegenModifier", index)) != 0;
            anyChange |= (baseStats.Energy = dbr.GetDoubleValueOrDefault("characterMana", index)) != 0;
            anyChange |= (baseStats.EnergyModifier = dbr.GetDoubleValueOrDefault("characterManaModifier", index)) != 0;
            anyChange |= (baseStats.EnergyRegeneration = dbr.GetDoubleValueOrDefault("characterManaRegen", index)) != 0;
            anyChange |= (baseStats.EnergyRegenerationModifier = dbr.GetDoubleValueOrDefault("characterManaRegenModifier", index)) != 0;
            anyChange |= (baseStats.Physique = dbr.GetDoubleValueOrDefault("characterStrength", index)) != 0;
            anyChange |= (baseStats.PhysiqueModifier = dbr.GetDoubleValueOrDefault("characterStrengthModifier", index)) != 0;
            anyChange |= (baseStats.Cunning = dbr.GetDoubleValueOrDefault("characterDexterity", index)) != 0;
            anyChange |= (baseStats.CunningModifier = dbr.GetDoubleValueOrDefault("characterDexterityModifier", index)) != 0;
            anyChange |= (baseStats.Spirit = dbr.GetDoubleValueOrDefault("characterIntelligence", index)) != 0;
            anyChange |= (baseStats.SpiritModifier = dbr.GetDoubleValueOrDefault("characterIntelligenceModifier", index)) != 0;
            anyChange |= (baseStats.Armor = dbr.GetDoubleValueOrDefault("defensiveProtection", index)) != 0;
            anyChange |= (baseStats.ArmorChance = dbr.GetDoubleValueOrDefault("defensiveProtectionChance", index)) != 0;
            anyChange |= (baseStats.ArmorModifier = dbr.GetDoubleValueOrDefault("defensiveProtectionModifier", index)) != 0;
            anyChange |= (baseStats.ArmorModifierChance = dbr.GetDoubleValueOrDefault("defensiveProtectionModifierChance", index)) != 0;
            anyChange |= (baseStats.ArmorAbsorptionModifier = dbr.GetDoubleValueOrDefault("defensiveAbsorptionModifier", index)) != 0;
            anyChange |= (baseStats.ResistBleed = dbr.GetDoubleValueOrDefault("defensiveBleeding", index)) != 0;
            anyChange |= (baseStats.ResistFire = dbr.GetDoubleValueOrDefault("defensiveFire", index)) != 0;
            anyChange |= (baseStats.ResistCold = dbr.GetDoubleValueOrDefault("defensiveCold", index)) != 0;
            anyChange |= (baseStats.ResistAether = dbr.GetDoubleValueOrDefault("defensiveAether", index)) != 0;
            anyChange |= (baseStats.ResistChaos = dbr.GetDoubleValueOrDefault("defensiveChaos", index)) != 0;
            anyChange |= (baseStats.ResistElemental = dbr.GetDoubleValueOrDefault("defensiveElementalResistance", index)) != 0;
            anyChange |= (baseStats.ResistKnockdown = dbr.GetDoubleValueOrDefault("defensiveKnockdown", index)) != 0;
            anyChange |= (baseStats.ResistVitality = dbr.GetDoubleValueOrDefault("defensiveLife", index)) != 0;
            anyChange |= (baseStats.ResistLightning = dbr.GetDoubleValueOrDefault("defensiveLightning", index)) != 0;
            anyChange |= (baseStats.ResistPhysical = dbr.GetDoubleValueOrDefault("defensivePhysical", index)) != 0;
            anyChange |= (baseStats.ResistPierce = dbr.GetDoubleValueOrDefault("defensivePierce", index)) != 0;
            anyChange |= (baseStats.ResistPoison = dbr.GetDoubleValueOrDefault("defensivePoison", index)) != 0;
            anyChange |= (baseStats.ResistStun = dbr.GetDoubleValueOrDefault("defensiveStun", index)) != 0;
            anyChange |= (baseStats.ResistSlow = dbr.GetDoubleValueOrDefault("defensiveTotalSpeedResistance", index)) != 0;
            anyChange |= (baseStats.ResistDisruption = dbr.GetDoubleValueOrDefault("defensiveDisruption", index)) != 0;
            anyChange |= (baseStats.MaxResistFire = dbr.GetDoubleValueOrDefault("defensiveFireMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistCold = dbr.GetDoubleValueOrDefault("defensiveColdMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistAether = dbr.GetDoubleValueOrDefault("defensiveAetherMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistChaos = dbr.GetDoubleValueOrDefault("defensiveChaosMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistAll = dbr.GetDoubleValueOrDefault("defensiveElementalResistanceMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistVitality = dbr.GetDoubleValueOrDefault("defensiveLifeMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistLightning = dbr.GetDoubleValueOrDefault("defensiveLightningMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistPhysical = dbr.GetDoubleValueOrDefault("defensivePhysicalMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistPierce = dbr.GetDoubleValueOrDefault("defensivePierceMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistPoison = dbr.GetDoubleValueOrDefault("defensivePoisonMaxResist", index)) != 0;
            anyChange |= (baseStats.MaxResistStun = dbr.GetDoubleValueOrDefault("defensiveStunMaxResist", index)) != 0;
            anyChange |= (baseStats.BlockValue = dbr.GetDoubleValueOrDefault("defensiveBlock", index)) != 0;
            anyChange |= (baseStats.BlockChance = dbr.GetDoubleValueOrDefault("defensiveBlockChance", index)) != 0;
            anyChange |= (baseStats.BlockRecoveryTime = dbr.GetDoubleValueOrDefault("blockRecoveryTime", index)) != 0;
            anyChange |= (baseStats.ShieldBlockChanceModifier = dbr.GetDoubleValueOrDefault("defensiveBlockModifier", index)) != 0;
            anyChange |= (baseStats.ShieldDamageBlockModifier = dbr.GetDoubleValueOrDefault("defensiveBlockAmountModifier", index)) != 0;
            anyChange |= (baseStats.OffensiveAetherModifier = dbr.GetDoubleValueOrDefault("offensiveAetherModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveChaosModifier = dbr.GetDoubleValueOrDefault("offensiveChaosModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveColdModifier = dbr.GetDoubleValueOrDefault("offensiveColdModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveFireModifier = dbr.GetDoubleValueOrDefault("offensiveFireModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveElementalModifier = dbr.GetDoubleValueOrDefault("offensiveElementalModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveKnockdownModifier = dbr.GetDoubleValueOrDefault("offensiveKnockdownModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveVitalityModifier = dbr.GetDoubleValueOrDefault("offensiveLifeModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveLightningModifier = dbr.GetDoubleValueOrDefault("offensiveLightningModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePhysicalModifier = dbr.GetDoubleValueOrDefault("offensivePhysicalModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePierceModifier = dbr.GetDoubleValueOrDefault("offensivePierceModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePoisonModifier = dbr.GetDoubleValueOrDefault("offensivePoisonModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveStunModifier = dbr.GetDoubleValueOrDefault("offensiveStunModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveAetherBaseMin = dbr.GetDoubleValueOrDefault("offensiveAetherMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveChaosBaseMin = dbr.GetDoubleValueOrDefault("offensiveChaosMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveColdBaseMin = dbr.GetDoubleValueOrDefault("offensiveColdMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveFireBaseMin = dbr.GetDoubleValueOrDefault("offensiveFireMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveElementalBaseMin = dbr.GetDoubleValueOrDefault("offensiveElementalMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveKnockdownBaseMin = dbr.GetDoubleValueOrDefault("offensiveKnockdownMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveVitalityBaseMin = dbr.GetDoubleValueOrDefault("offensiveLifeMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveLightningBaseMin = dbr.GetDoubleValueOrDefault("offensiveLightningMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePhysicalBaseMin = dbr.GetDoubleValueOrDefault("offensivePhysicalMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePierceBaseMin = dbr.GetDoubleValueOrDefault("offensivePierceMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePoisonBaseMin = dbr.GetDoubleValueOrDefault("offensivePoisonMin", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveAetherBaseMax = dbr.GetDoubleValueOrDefault("offensiveAetherMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveChaosBaseMax = dbr.GetDoubleValueOrDefault("offensiveChaosMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveColdBaseMax = dbr.GetDoubleValueOrDefault("offensiveColdMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveFireBaseMax = dbr.GetDoubleValueOrDefault("offensiveFireMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveElementalBaseMax = dbr.GetDoubleValueOrDefault("offensiveElementalMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveKnockdownBaseMax = dbr.GetDoubleValueOrDefault("offensiveKnockdownMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveVitalityBaseMax = dbr.GetDoubleValueOrDefault("offensiveLifeMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveLightningBaseMax = dbr.GetDoubleValueOrDefault("offensiveLightningMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePhysicalBaseMax = dbr.GetDoubleValueOrDefault("offensivePhysicalMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePierceBaseMax = dbr.GetDoubleValueOrDefault("offensivePierceMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePoisonBaseMax = dbr.GetDoubleValueOrDefault("offensivePoisonMax", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveBleedDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowBleedingModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveBleedDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowBleedingDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensiveBleedDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowBleedingMin", index)) != 0;
            anyChange |= (baseStats.OffensiveColdDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowColdModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveColdDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowColdDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensiveColdDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowColdMin", index)) != 0;
            anyChange |= (baseStats.OffensiveFireDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowFireModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveFireDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowFireDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensiveFireDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowFireMin", index)) != 0;
            anyChange |= (baseStats.OffensiveVitalityDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowLifeModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveVitalityDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowLifeDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensiveVitalityDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowLifeMin", index)) != 0;
            anyChange |= (baseStats.OffensiveLightningDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowLightningModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensiveLightningDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowLightningDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensiveLightningDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowLightningMin", index)) != 0;
            anyChange |= (baseStats.OffensivePhysicalDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowPhysicalModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePhysicalDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowPhysicalDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensivePhysicalDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowPhysicalMin", index)) != 0;
            anyChange |= (baseStats.OffensivePoisonDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowPoisonModifier", index) * (1 + attributeScalePercent)) != 0;
            anyChange |= (baseStats.OffensivePoisonDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowPoisonDurationMin", index)) != 0;
            anyChange |= (baseStats.OffensivePoisonDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowPoisonMin", index)) != 0;
            anyChange |= (baseStats.OffensiveAbility = dbr.GetDoubleValueOrDefault("characterOffensiveAbility", index)) != 0;
            anyChange |= (baseStats.OffensiveAbilityModifier = dbr.GetDoubleValueOrDefault("characterOffensiveAbilityModifier", index)) != 0;
            anyChange |= (baseStats.DefensiveAbility = dbr.GetDoubleValueOrDefault("characterDefensiveAbility", index)) != 0;
            anyChange |= (baseStats.DefensiveAbilityModifier = dbr.GetDoubleValueOrDefault("characterDefensiveAbilityModifier", index)) != 0;
            anyChange |= (baseStats.AttackSpeedModifier = dbr.GetDoubleValueOrDefault("characterAttackSpeedModifier", index)) != 0;
            anyChange |= (baseStats.CastSpeedModifier = dbr.GetDoubleValueOrDefault("characterSpellCastSpeedModifier", index)) != 0;
            anyChange |= (baseStats.SkillCooldownReduction = dbr.GetDoubleValueOrDefault("skillCooldownReduction", index)) != 0;
            anyChange |= (baseStats.RunSpeedModifier = dbr.GetDoubleValueOrDefault("characterRunSpeedModifier", index)) != 0;
            anyChange |= (baseStats.TotalDamageModifier = dbr.GetDoubleValueOrDefault("offensiveDamageMultModifier", index)) != 0;
            anyChange |= (baseStats.WeaponDamageModifier = dbr.GetDoubleValueOrDefault("weaponDamagePct", index)) != 0;
            anyChange |= (baseStats.EnergyCostModifier = dbr.GetDoubleValueOrDefault("skillManaCostReduction", index)) != 0;
            anyChange |= (baseStats.EnergyCost = dbr.GetDoubleValueOrDefault("skillManaCost", index)) != 0;
            anyChange |= (baseStats.LifeMonitorPercent = dbr.GetDoubleValueOrDefault("lifeMonitorPercent", index)) != 0;
            anyChange |= (baseStats.SkillDuration = dbr.GetDoubleValueOrDefault("skillActiveDuration", index)) != 0;
            anyChange |= (baseStats.SkillCooldown = dbr.GetDoubleValueOrDefault("skillCooldownTime", index)) != 0;
            anyChange |= (baseStats.RestoreLifePercent = dbr.GetDoubleValueOrDefault("skillLifePercent", index)) != 0;
            anyChange |= (baseStats.DamageAbsorptionPercent = dbr.GetDoubleValueOrDefault("damageAbsorptionPercent", index)) != 0;

            return anyChange;
        }

        static async Task<Item[]> GetItemsAsync(string gdDbPath, IDictionary<string, PlayerSkill> skillDictionary, TagParser skillTags)
        {
            var itemTypeMapping = new Dictionary<string, ItemType>
            {
                ["ItemArtifact"] = ItemType.Relic,
                ["ArmorProtective_Feet"] = ItemType.Feet,
                ["ArmorProtective_Hands"] = ItemType.Hands,
                ["ArmorProtective_Head"] = ItemType.Head,
                ["ArmorProtective_Chest"] = ItemType.Chest,
                ["ArmorProtective_Shoulders"] = ItemType.Shoulders,
                ["ArmorProtective_Waist"] = ItemType.Belt,
                ["ArmorJewelry_Medal"] = ItemType.Medal,
                ["ArmorJewelry_Amulet"] = ItemType.Amulet,
                ["ArmorJewelry_Ring"] = ItemType.Ring,
                ["WeaponArmor_Offhand"] = ItemType.OffhandFocus,
                ["WeaponArmor_Shield"] = ItemType.Shield,
                ["WeaponMelee_Sword"] = ItemType.WeaponOneHandedSword,
                ["WeaponMelee_Mace"] = ItemType.WeaponOneHandedMace,
                ["WeaponMelee_Axe"] = ItemType.WeaponOneHandedAxe,
                ["WeaponHunting_Ranged1h"] = ItemType.WeaponOneHandedGun,
                ["WeaponMelee_Dagger"] = ItemType.WeaponDagger,
                ["WeaponMelee_Sword2h"] = ItemType.WeaponTwoHandedSword,
                ["WeaponMelee_Mace2h"] = ItemType.WeaponTwoHandedMace,
                ["WeaponMelee_Axe2h"] = ItemType.WeaponTwoHandedAxe,
                ["WeaponHunting_Ranged2h"] = ItemType.WeaponTwoHandedGun,
            };

            var dirs = Directory.GetFiles(Path.Combine(gdDbPath, @"database\records\items"), "*.dbr", SearchOption.AllDirectories);
            var results = new ConcurrentBag<Item>();

            await Task.WhenAll(dirs.Select(async file =>
            {
                if (file.Contains("enemygear", StringComparison.OrdinalIgnoreCase))
                    return;

                var dbr = await DbrParser.FromPathAsync("", "", file).ConfigureAwait(false);

                if (dbr.TryGetStringValue("Class", 0, out var @class) && itemTypeMapping.TryGetValue(@class, out var itemType))
                {
                    // TODO parse loot tables for this info, instead of bailing out on items with inexistant names
                    if (!dbr.TryGetStringValue("itemNameTag", 0, out var itemTagName) || !skillTags.TryGetValue(itemTagName, out var name))
                        if (dbr.TryGetStringValue("itemSkillName", 0, out var itemSkillName))
                        {
                            var skillDbr = await DbrParser.FromPathAsync(gdDbPath, "database", itemSkillName, navigationProperties).ConfigureAwait(false);
                            name = skillTags[skillDbr.GetStringValue("skillDisplayName")];
                        }
                        else
                            return;

                    Item item = new Item
                    {
                        Name = name,
                        Description = dbr.TryGetStringValue("itemText", 0, out var tagItemText) && skillTags.TryGetValue(tagItemText, out var itemText) ? itemText : null,
                        ItemLevel = (int)dbr.GetDoubleValue("itemLevel"),
                        BitmapPath = dbr.TryGetStringValue("bitmap", 0, out var bitmap) ? bitmap : dbr.GetStringValue("artifactBitmap"),
                        Type = itemType,
                        ArmorClassificationText = dbr.GetStringValueOrDefault("armorClassification"),

                        Rarity = dbr.TryGetStringValue("itemClassification", 0, out var itemClassification) ? Enum.Parse<ItemRarity>(itemClassification) : ItemRarity.Broken,
                        ArtifactRarity = dbr.TryGetStringValue("artifactClassification", 0, out var artifactClassification) ? Enum.Parse<ItemArtifactRarity>(artifactClassification) : ItemArtifactRarity.None,
                        ItemStyleText = dbr.TryGetStringValue("itemStyleTag", 0, out var itemStyleTag) ? skillTags[itemStyleTag] : null,

                        SkillsWithQuantity = dbr.GetAllStringsOfFormat("augmentSkillName{0}")
                            .Select(kvp => new PlayerSkillAugmentWithQuantity
                            {
                                Skill = skillDictionary[kvp.values.First()],
                                Quantity = (int)dbr.GetDoubleValueOrDefault($"augmentSkillLevel{kvp.key[^1]}"),
                            })
                            .Where(sq => sq.Quantity != 0)
                            .ToList(),
                    };

                    LoadBaseStats(item, dbr);

                    (item.Bitmap, item.BitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), item.BitmapPath).ConfigureAwait(false);
                    results.Add(item);
                }
            })).ConfigureAwait(false);

            return results.ToArray();
        }

        static async Task<(EquipSlot[], ItemRarityTextStyle[])> GetGameDataAsync(string gdDbPath)
        {
            var masterCharacterTableParser = await DbrParser.FromPathAsync(gdDbPath, "database", @"records\ui\character\character_mastertable.dbr").ConfigureAwait(false);

            var equipSlots = await Task.WhenAll(Enum.GetValues(typeof(EquipSlotType)).Cast<EquipSlotType>().Select(val => (val, name: Enum.GetName(typeof(EquipSlotType), val)))
                .Select(async type =>
                {
                    var parser = await DbrParser.FromPathAsync(gdDbPath, "database", masterCharacterTableParser.GetStringValue($"equip{(type.val == EquipSlotType.Relic ? "Artifact" : type.name)}")).ConfigureAwait(false);
                    var slot = new EquipSlot
                    {
                        Type = type.val,
                        PositionX = (int)parser.GetDoubleValue("itemX"),
                        PositionY = (int)parser.GetDoubleValue("itemY"),
                        Width = (int)parser.GetDoubleValue("itemXSize"),
                        Height = (int)parser.GetDoubleValue("itemYSize"),
                        SilhouetteBitmapPath = parser.GetStringValue("silhouette"),
                    };
                    (slot.SilhouetteBitmap, slot.SilhouetteBitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), parser.GetStringValue("silhouette")).ConfigureAwait(false);
                    return slot;
                })).ConfigureAwait(false);

            var gameEngineParser = await DbrParser.FromPathAsync(gdDbPath, "database", @"records/game/gameengine.dbr").ConfigureAwait(false);

            var itemRarityTextStyles = await Task.WhenAll(Enum.GetValues(typeof(ItemRarity)).Cast<ItemRarity>().Select(val => (val, name: Enum.GetName(typeof(ItemRarity), val)))
                .Select(async rarity =>
                {
                    var parser = await DbrParser.FromPathAsync(gdDbPath, "database", gameEngineParser.GetStringValue($"ItemName{rarity.name}")).ConfigureAwait(false);
                    return new ItemRarityTextStyle
                    {
                        Bold = parser.GetDoubleValue("fontAttribBold") != 0,
                        Italic = parser.GetDoubleValue("fontAttribItalic") != 0,
                        R = parser.GetDoubleValue("fontColorRed"),
                        G = parser.GetDoubleValue("fontColorGreen"),
                        B = parser.GetDoubleValue("fontColorBlue"),
                        FontName = parser.GetStringValue("fontName"),
                        Rarity = rarity.val,
                    };
                })).ConfigureAwait(false);

            return (equipSlots, itemRarityTextStyles);
        }

        static async Task Main(string[] args)
        {
            var skillTags = await TagParser.FromArcFilesAsync(Directory.GetFiles(args[0], "*.arc", SearchOption.AllDirectories)).ConfigureAwait(false);

            var (classes, skillDictionary) = await GetPlayerClassesAsync(args[0], skillTags).ConfigureAwait(false);

            PlayerAffinity[] affinities = default;
            PlayerConstellation[] constellations = default;
            PlayerConstellationNebula[] nebulas = default;
            Item[] items = default;
            EquipSlot[] equipSlots = default;
            ItemRarityTextStyle[] itemRarityTextStyles = default;

            await Task.WhenAll(new Func<Task>[]
            {
                async () => (equipSlots, itemRarityTextStyles) = await GetGameDataAsync(args[0]).ConfigureAwait(false),
                async () => (affinities, constellations, nebulas) = await GetPlayerConstellationsAsync(args[0], skillTags).ConfigureAwait(false),
                async () => items = await GetItemsAsync(args[0], skillDictionary, skillTags).ConfigureAwait(false),
            }.Select(fn => fn())).ConfigureAwait(false);

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

            var nebulaCollection = db.GetCollection<PlayerConstellationNebula>();
            nebulaCollection.InsertBulk(nebulas);

            var itemsCollection = db.GetCollection<Item>();
            itemsCollection.InsertBulk(items);
            itemsCollection.EnsureIndex(c => c.Name);

            var equipSlotCollection = db.GetCollection<EquipSlot>();
            equipSlotCollection.InsertBulk(equipSlots);
            equipSlotCollection.EnsureIndex(e => e.Type);

            var itemRarityTextStyleCollection = db.GetCollection<ItemRarityTextStyle>();
            itemRarityTextStyleCollection.InsertBulk(itemRarityTextStyles);
            itemRarityTextStyleCollection.EnsureIndex(e => e.Rarity);

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
            nebulas.ForEach(c => Upload(c.BitmapPath, c.Bitmap));
            classes.ForEach(c => Upload(c.BitmapPath, c.Bitmap));
            items.ForEach(i => Upload(i.BitmapPath, i.Bitmap));
            equipSlots.ForEach(i => Upload(i.SilhouetteBitmapPath, i.SilhouetteBitmap));

            // now do class combinations
            var classCombinationCollection = db.GetCollection<PlayerClassCombination>();
            classCombinationCollection.InsertBulk(Enumerable.Range(1, classes.Length).SelectMany(c1 => Enumerable.Range(1, classes.Length).Exclude(c1 - 1, 1)
                .Select(c2 =>
                    new PlayerClassCombination
                    {
                        ClassName1 = skillTags[$"tagSkillClassName{c1:00}"],
                        ClassName2 = skillTags[$"tagSkillClassName{c2:00}"],
                        Name = skillTags[$"tagSkillClassName{Math.Min(c1, c2):00}{Math.Max(c1, c2):00}"],
                    })));

            Console.WriteLine($"Parsed {DbrParser.FileCount} DBR files and {TexParser.FileCount} TEX files.");
        }
    }
}
