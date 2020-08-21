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

                    var attributeScalePercent = dbr.GetDoubleValueOrDefault("attributeScalePercent") / 100.0;
                    Item item = new Item
                    {
                        Name = name,
                        Description = dbr.TryGetStringValue("itemText", 0, out var tagItemText) && skillTags.TryGetValue(tagItemText, out var itemText) ? itemText : null,
                        ItemLevel = (int)dbr.GetDoubleValue("itemLevel"),
                        AttributeScalePercent = attributeScalePercent,
                        BitmapPath = dbr.TryGetStringValue("bitmap", 0, out var bitmap) ? bitmap : dbr.GetStringValue("artifactBitmap"),
                        Type = itemType,
                        ArmorClassificationText = dbr.GetStringValueOrDefault("armorClassification"),

                        Rarity = dbr.TryGetStringValue("itemClassification", 0, out var itemClassification) ? Enum.Parse<ItemRarity>(itemClassification) : ItemRarity.Broken,
                        ArtifactRarity = dbr.TryGetStringValue("artifactClassification", 0, out var artifactClassification) ? Enum.Parse<ItemArtifactRarity>(artifactClassification) : ItemArtifactRarity.None,
                        ItemStyleText = dbr.TryGetStringValue("itemStyleTag", 0, out var itemStyleTag) ? skillTags[itemStyleTag] : null,

                        LevelRequirement = (int)dbr.GetDoubleValueOrDefault("levelRequirement"),
                        PhysiqueRequirement = dbr.GetDoubleValueOrDefault("strengthRequirement"),
                        CunningRequirement = dbr.GetDoubleValueOrDefault("dexterityRequirement"),
                        SpiritRequirement = dbr.GetDoubleValueOrDefault("intelligenceRequirement"),

                        Life = dbr.GetDoubleValueOrDefault("characterLife"),
                        LifeModifier = dbr.GetDoubleValueOrDefault("characterLifeModifier"),
                        LifeRegeneration = dbr.GetDoubleValueOrDefault("characterLifeRegen"),
                        LifeRegenerationModifier = dbr.GetDoubleValueOrDefault("characterLifeRegenModifier"),

                        Energy = dbr.GetDoubleValueOrDefault("characterMana"),
                        EnergyModifier = dbr.GetDoubleValueOrDefault("characterManaModifier"),
                        EnergyRegeneration = dbr.GetDoubleValueOrDefault("characterManaRegen"),
                        EnergyRegenerationModifier = dbr.GetDoubleValueOrDefault("characterManaRegenModifier"),

                        Physique = dbr.GetDoubleValueOrDefault("characterStrength"),
                        PhysiqueModifier = dbr.GetDoubleValueOrDefault("characterStrengthModifier"),
                        Cunning = dbr.GetDoubleValueOrDefault("characterDexterity"),
                        CunningModifier = dbr.GetDoubleValueOrDefault("characterDexterityModifier"),
                        Spirit = dbr.GetDoubleValueOrDefault("characterIntelligence"),
                        SpiritModifier = dbr.GetDoubleValueOrDefault("characterIntelligenceModifier"),

                        Armor = dbr.GetDoubleValueOrDefault("defensiveProtection"),
                        ArmorChance = dbr.GetDoubleValueOrDefault("defensiveProtectionChance"),
                        ArmorModifier = dbr.GetDoubleValueOrDefault("defensiveProtectionModifier"),
                        ArmorModifierChance = dbr.GetDoubleValueOrDefault("defensiveProtectionModifierChance"),
                        ArmorAbsorptionModifier = dbr.GetDoubleValueOrDefault("defensiveAbsorptionModifier"),

                        ResistBleed = dbr.GetDoubleValueOrDefault("defensiveBleeding"),
                        ResistFire = dbr.GetDoubleValueOrDefault("defensiveFire"),
                        ResistCold = dbr.GetDoubleValueOrDefault("defensiveCold"),
                        ResistAether = dbr.GetDoubleValueOrDefault("defensiveAether"),
                        ResistChaos = dbr.GetDoubleValueOrDefault("defensiveChaos"),
                        ResistElemental = dbr.GetDoubleValueOrDefault("defensiveElementalResistance"),
                        ResistKnockdown = dbr.GetDoubleValueOrDefault("defensiveKnockdown"),
                        ResistVitality = dbr.GetDoubleValueOrDefault("defensiveLife"),
                        ResistLightning = dbr.GetDoubleValueOrDefault("defensiveLightning"),
                        ResistPhysical = dbr.GetDoubleValueOrDefault("defensivePhysical"),
                        ResistPierce = dbr.GetDoubleValueOrDefault("defensivePierce"),
                        ResistPoison = dbr.GetDoubleValueOrDefault("defensivePoison"),
                        ResistStun = dbr.GetDoubleValueOrDefault("defensiveStun"),
                        ResistSlow = dbr.GetDoubleValueOrDefault("defensiveTotalSpeedResistance"),
                        ResistDisruption = dbr.GetDoubleValueOrDefault("defensiveDisruption"),

                        MaxResistFire = dbr.GetDoubleValueOrDefault("defensiveFireMaxResist"),
                        MaxResistCold = dbr.GetDoubleValueOrDefault("defensiveColdMaxResist"),
                        MaxResistAether = dbr.GetDoubleValueOrDefault("defensiveAetherMaxResist"),
                        MaxResistChaos = dbr.GetDoubleValueOrDefault("defensiveChaosMaxResist"),
                        MaxResistAll = dbr.GetDoubleValueOrDefault("defensiveElementalResistanceMaxResist"),
                        MaxResistVitality = dbr.GetDoubleValueOrDefault("defensiveLifeMaxResist"),
                        MaxResistLightning = dbr.GetDoubleValueOrDefault("defensiveLightningMaxResist"),
                        MaxResistPhysical = dbr.GetDoubleValueOrDefault("defensivePhysicalMaxResist"),
                        MaxResistPierce = dbr.GetDoubleValueOrDefault("defensivePierceMaxResist"),
                        MaxResistPoison = dbr.GetDoubleValueOrDefault("defensivePoisonMaxResist"),
                        MaxResistStun = dbr.GetDoubleValueOrDefault("defensiveStunMaxResist"),

                        BlockValue = dbr.GetDoubleValueOrDefault("defensiveBlock"),
                        BlockChance = dbr.GetDoubleValueOrDefault("defensiveBlockChance"),
                        BlockRecoveryTime = dbr.GetDoubleValueOrDefault("blockRecoveryTime"),
                        ShieldBlockChanceModifier = dbr.GetDoubleValueOrDefault("defensiveBlockModifier"),
                        ShieldDamageBlockModifier = dbr.GetDoubleValueOrDefault("defensiveBlockAmountModifier"),

                        OffensiveAetherModifier = dbr.GetDoubleValueOrDefault("offensiveAetherModifier") * (1 + attributeScalePercent),
                        OffensiveChaosModifier = dbr.GetDoubleValueOrDefault("offensiveChaosModifier") * (1 + attributeScalePercent),
                        OffensiveColdModifier = dbr.GetDoubleValueOrDefault("offensiveColdModifier") * (1 + attributeScalePercent),
                        OffensiveFireModifier = dbr.GetDoubleValueOrDefault("offensiveFireModifier") * (1 + attributeScalePercent),
                        OffensiveKnockdownModifier = dbr.GetDoubleValueOrDefault("offensiveKnockdownModifier") * (1 + attributeScalePercent),
                        OffensiveVitalityModifier = dbr.GetDoubleValueOrDefault("offensiveLifeModifier") * (1 + attributeScalePercent),
                        OffensiveLightningModifier = dbr.GetDoubleValueOrDefault("offensiveLightningModifier") * (1 + attributeScalePercent),
                        OffensivePhysicalModifier = dbr.GetDoubleValueOrDefault("offensivePhysicalModifier") * (1 + attributeScalePercent),
                        OffensivePierceModifier = dbr.GetDoubleValueOrDefault("offensivePierceModifier") * (1 + attributeScalePercent),
                        OffensivePoisonModifier = dbr.GetDoubleValueOrDefault("offensivePoisonModifier") * (1 + attributeScalePercent),
                        OffensiveStunModifier = dbr.GetDoubleValueOrDefault("offensiveStunModifier") * (1 + attributeScalePercent),

                        OffensiveAetherBaseMin = dbr.GetDoubleValueOrDefault("offensiveAetherMin") * (1 + attributeScalePercent),
                        OffensiveChaosBaseMin = dbr.GetDoubleValueOrDefault("offensiveChaosMin") * (1 + attributeScalePercent),
                        OffensiveColdBaseMin = dbr.GetDoubleValueOrDefault("offensiveColdMin") * (1 + attributeScalePercent),
                        OffensiveFireBaseMin = dbr.GetDoubleValueOrDefault("offensiveFireMin") * (1 + attributeScalePercent),
                        OffensiveKnockdownBaseMin = dbr.GetDoubleValueOrDefault("offensiveKnockdownMin") * (1 + attributeScalePercent),
                        OffensiveVitalityBaseMin = dbr.GetDoubleValueOrDefault("offensiveLifeMin") * (1 + attributeScalePercent),
                        OffensiveLightningBaseMin = dbr.GetDoubleValueOrDefault("offensiveLightningMin") * (1 + attributeScalePercent),
                        OffensivePhysicalBaseMin = dbr.GetDoubleValueOrDefault("offensivePhysicalMin") * (1 + attributeScalePercent),
                        OffensivePierceBaseMin = dbr.GetDoubleValueOrDefault("offensivePierceMin") * (1 + attributeScalePercent),
                        OffensivePoisonBaseMin = dbr.GetDoubleValueOrDefault("offensivePoisonMin") * (1 + attributeScalePercent),

                        OffensiveAetherBaseMax = dbr.GetDoubleValueOrDefault("offensiveAetherMax") * (1 + attributeScalePercent),
                        OffensiveChaosBaseMax = dbr.GetDoubleValueOrDefault("offensiveChaosMax") * (1 + attributeScalePercent),
                        OffensiveColdBaseMax = dbr.GetDoubleValueOrDefault("offensiveColdMax") * (1 + attributeScalePercent),
                        OffensiveFireBaseMax = dbr.GetDoubleValueOrDefault("offensiveFireMax") * (1 + attributeScalePercent),
                        OffensiveKnockdownBaseMax = dbr.GetDoubleValueOrDefault("offensiveKnockdownMax") * (1 + attributeScalePercent),
                        OffensiveVitalityBaseMax = dbr.GetDoubleValueOrDefault("offensiveLifeMax") * (1 + attributeScalePercent),
                        OffensiveLightningBaseMax = dbr.GetDoubleValueOrDefault("offensiveLightningMax") * (1 + attributeScalePercent),
                        OffensivePhysicalBaseMax = dbr.GetDoubleValueOrDefault("offensivePhysicalMax") * (1 + attributeScalePercent),
                        OffensivePierceBaseMax = dbr.GetDoubleValueOrDefault("offensivePierceMax") * (1 + attributeScalePercent),
                        OffensivePoisonBaseMax = dbr.GetDoubleValueOrDefault("offensivePoisonMax") * (1 + attributeScalePercent),

                        OffensiveBleedDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowBleedingModifier") * (1 + attributeScalePercent),
                        OffensiveBleedDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowBleedingDurationMin"),
                        OffensiveBleedDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowBleedingMin"),
                        OffensiveColdDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowColdModifier") * (1 + attributeScalePercent),
                        OffensiveColdDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowColdDurationMin"),
                        OffensiveColdDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowColdMin"),
                        OffensiveFireDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowFireModifier") * (1 + attributeScalePercent),
                        OffensiveFireDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowFireDurationMin"),
                        OffensiveFireDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowFireMin"),
                        OffensiveVitalityDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowLifeModifier") * (1 + attributeScalePercent),
                        OffensiveVitalityDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowLifeDurationMin"),
                        OffensiveVitalityDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowLifeMin"),
                        OffensiveLightningDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowLightningModifier") * (1 + attributeScalePercent),
                        OffensiveLightningDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowLightningDurationMin"),
                        OffensiveLightningDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowLightningMin"),
                        OffensivePhysicalDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowPhysicalModifier") * (1 + attributeScalePercent),
                        OffensivePhysicalDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowPhysicalDurationMin"),
                        OffensivePhysicalDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowPhysicalMin"),
                        OffensivePoisonDotModifier = dbr.GetDoubleValueOrDefault("offensiveSlowPoisonModifier") * (1 + attributeScalePercent),
                        OffensivePoisonDotDuration = dbr.GetDoubleValueOrDefault("offensiveSlowPoisonDurationMin"),
                        OffensivePoisonDotTickDamage = dbr.GetDoubleValueOrDefault("offensiveSlowPoisonMin"),

                        OffensiveAbility = dbr.GetDoubleValueOrDefault("characterOffensiveAbility"),
                        OffensiveAbilityModifier = dbr.GetDoubleValueOrDefault("characterOffensiveAbilityModifier"),
                        DefensiveAbility = dbr.GetDoubleValueOrDefault("characterDefensiveAbility"),
                        DefensiveAbilityModifier = dbr.GetDoubleValueOrDefault("characterDefensiveAbilityModifier"),

                        AttackSpeedModifier = dbr.GetDoubleValueOrDefault("characterAttackSpeedModifier"),
                        CastSpeedModifier = dbr.GetDoubleValueOrDefault("characterSpellCastSpeedModifier"),
                        SkillCooldownReduction = dbr.GetDoubleValueOrDefault("skillCooldownReduction"),

                        RunSpeedModifier = dbr.GetDoubleValueOrDefault("characterRunSpeedModifier"),

                        SkillsWithQuantity = dbr.GetAllStringsOfFormat("augmentSkillName{0}")
                            .Select(kvp => new PlayerSkillAugmentWithQuantity
                            {
                                Skill = skillDictionary[kvp.values.First()],
                                Quantity = (int)dbr.GetDoubleValueOrDefault($"augmentSkillLevel{kvp.key[^1]}"),
                            })
                            .Where(sq => sq.Quantity != 0)
                            .ToList(),
                    };

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
