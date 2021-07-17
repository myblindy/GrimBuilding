using GrimBuilding.Common.Support;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrimBuilding.Common
{
    public class GdDbContext : DbContext
    {
        public DbSet<PlayerAffinity> PlayerAffinities { get; set; }
        public DbSet<PlayerSkill> PlayerSkills { get; set; }
        public DbSet<PlayerClass> PlayerClasses { get; set; }
        public DbSet<PlayerConstellation> PlayerConstellations { get; set; }
        public DbSet<PlayerConstellationNebula> PlayerConstellationNebulas { get; set; }
        public DbSet<PlayerClassCombination> PlayerClassCombinations { get; set; }
        public DbSet<ItemRarityTextStyle> ItemRarityTextStyles { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<EquipSlot> EquipSlots { get; set; }
        public DbSet<FileData> Files { get; set; }
        public DbSet<BaseStats> BaseStats { get; set; }
        public DbSet<PlayerResistance> PlayerResistances { get; set; }

        readonly string fullDbPath = "data.db";
        public GdDbContext(string path = null) => fullDbPath = path ?? fullDbPath;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Data Source={fullDbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerSkill>().Property(w => w.BitmapSkillConnectionOffPaths)
                .HasConversion(w => JsonSerializer.Serialize(w, (JsonSerializerOptions)null), w => JsonSerializer.Deserialize<List<string>>(w, (JsonSerializerOptions)null));
            modelBuilder.Entity<PlayerConstellation>().Property(w => w.SkillRequirements)
                .HasConversion(w => JsonSerializer.Serialize(w, (JsonSerializerOptions)null), w => JsonSerializer.Deserialize<List<int>>(w, (JsonSerializerOptions)null));
        }
    }
}
