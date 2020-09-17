
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrimBuilding.Common.Support
{
    public class PlayerConstellation
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string BitmapPath { get; set; }
        [NotMapped]
        public byte[] Bitmap { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public List<PlayerAffinityQuantity> RequiredAffinities { get; set; }

        public List<PlayerAffinityQuantity> RewardedAffinities { get; set; }

        public List<PlayerSkill> Skills { get; set; }

        /// <summary>
        /// An array of skill requirements, one for each skill except the first (since it has no requirements in the constellation). 
        /// </summary>
        public List<int> SkillRequirements { get; set; }
    }

    public class PlayerAffinityQuantity
    {
        public int Id { get; set; }
        public PlayerAffinity Type { get; set; }
        public int Quantity { get; set; }
    }
}