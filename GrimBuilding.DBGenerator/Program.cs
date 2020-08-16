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
                            .Select(kvpRequired => (affinities.First(a => a.Name == kvpRequired.values.First()), quantity: (int)constellation.GetDoubleValue($"{kvpRequired.key[..^5]}{kvpRequired.key[^1]}")))
                            .Where(w => w.quantity > 0)
                            .ToArray(),
                        RewardedAffinities = constellation.GetAllStringsOfFormat("affinityGivenName{0}")
                            .Select(kvpRewarded => (affinities.First(a => a.Name == kvpRewarded.values.First()), quantity: (int)constellation.GetDoubleValue($"{kvpRewarded.key[..^5]}{kvpRewarded.key[^1]}")))
                            .Where(w => w.quantity > 0)
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

        static async Task<PlayerClass[]> GetPlayerClassesAsync(string gdDbPath, TagParser skillTags)
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
                    BitmapPath = (await DbrParser.FromPathAsync(gdDbPath, "database", masterClassList.GetStringValue("skillPaneMasteryBitmap")).ConfigureAwait(false))
                        .GetStringValue("bitmapName"),
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

                await Task.WhenAll(result[idx].Skills
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

                (result[idx].Bitmap, result[idx].BitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), result[idx].BitmapPath).ConfigureAwait(false);

            })).ConfigureAwait(false);

            return result;
        }

        static async Task<Item[]> GetItemsAsync(string gdDbPath, TagParser skillTags)
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
                ["WeaponMelee_Sword1h"] = ItemType.WeaponOneHandedSword,
                ["WeaponMelee_Mace1h"] = ItemType.WeaponOneHandedMace,
                ["WeaponMelee_Axe1h"] = ItemType.WeaponOneHandedAxe,
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
                        AttributeScalePercent = dbr.GetDoubleValueOrDefault("attributeScalePercent"),
                        BitmapPath = dbr.TryGetStringValue("bitmap", 0, out var bitmap) ? bitmap : dbr.GetStringValue("artifactBitmap"),
                        Type = itemType,

                        Rarity = dbr.TryGetStringValue("itemClassification", 0, out var itemClassification) ? Enum.Parse<ItemRarity>(itemClassification) : ItemRarity.Junk,
                        ArtifactRarity = dbr.TryGetStringValue("artifactClassification", 0, out var artifactClassification) ? Enum.Parse<ItemArtifactRarity>(artifactClassification) : ItemArtifactRarity.None,

                        LevelRequirement = (int)dbr.GetDoubleValueOrDefault("levelRequirement"),
                        PhysiqueRequirement = dbr.GetDoubleValueOrDefault("strengthRequirement"),
                        CunningRequirement = dbr.GetDoubleValueOrDefault("dexterityRequirement"),
                        SpiritRequirement = dbr.GetDoubleValueOrDefault("intelligenceRequirement"),

                        Life = dbr.GetDoubleValueOrDefault("characterLife"),
                        LifeModifier = dbr.GetDoubleValueOrDefault("characterLifeModifier"),
                        LifeRegeneration = dbr.GetDoubleValueOrDefault("characterLifeRegen"),
                        LifeRegenerationModifier = dbr.GetDoubleValueOrDefault("characterLifeRegenModifier"),

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
                    };

                    (item.Bitmap, item.BitmapPath) = await TexParser.ExtractPng(Path.Combine(gdDbPath, "resources"), item.BitmapPath).ConfigureAwait(false);
                    results.Add(item);
                }
            })).ConfigureAwait(false);

            return results.ToArray();
        }

        static async Task<EquipSlot[]> GetEquipSlotsAsync(string gdDbPath)
        {
            var masterParser = await DbrParser.FromPathAsync(gdDbPath, "database", @"records\ui\character\character_mastertable.dbr").ConfigureAwait(false);

            return await Task.WhenAll(Enum.GetValues(typeof(EquipSlotType)).Cast<EquipSlotType>().Select(val => (val, name: Enum.GetName(typeof(EquipSlotType), val)))
                .Select(async type =>
                {
                    var parser = await DbrParser.FromPathAsync(gdDbPath, "database", masterParser.GetStringValue($"equip{(type.val == EquipSlotType.Relic ? "Artifact" : type.name)}")).ConfigureAwait(false);
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
        }

        static async Task Main(string[] args)
        {
            var skillTags = await TagParser.FromArcFilesAsync(Directory.GetFiles(args[0], "*.arc", SearchOption.AllDirectories)).ConfigureAwait(false);

            PlayerClass[] classes = default;
            PlayerAffinity[] affinities = default;
            PlayerConstellation[] constellations = default;
            PlayerConstellationNebula[] nebulas = default;
            Item[] items = default;
            EquipSlot[] equipSlots = default;

            await Task.WhenAll(new[]
            {
                (Func<Task>)(async () =>classes = await GetPlayerClassesAsync(args[0], skillTags).ConfigureAwait(false)),
                async () => equipSlots = await GetEquipSlotsAsync(args[0]).ConfigureAwait(false),
                async () => (affinities, constellations, nebulas) = await GetPlayerConstellationsAsync(args[0], skillTags).ConfigureAwait(false),
                async () => items = await GetItemsAsync(args[0], skillTags).ConfigureAwait(false),
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
        }
    }
}
