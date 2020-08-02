using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator.Support
{
    struct PlayerSkill
    {
        public string Name;
        public string BitmapUpPath, BitmapDownPath;
    }

    class PlayerClass
    {
        public string Name;
        public PlayerSkill[] Skills;

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
            })).ConfigureAwait(false);

            return result;
        }
    }
}