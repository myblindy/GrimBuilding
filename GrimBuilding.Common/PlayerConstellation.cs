﻿using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerConstellation
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string BitmapPath { get; set; }
        [BsonIgnore]
        public byte[] Bitmap { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public PlayerAffinityQuantity[] RequiredAffinities { get; set; }

        public PlayerAffinityQuantity[] RewardedAffinities { get; set; }

        [BsonRef]
        public PlayerSkill[] Skills { get; set; }

        /// <summary>
        /// An array of skill requirements, one for each skill except the first (since it has no requirements in the constellation). 
        /// </summary>
        public int[] SkillRequirements { get; set; }
    }

    public class PlayerAffinityQuantity
    {
        [BsonRef]
        public PlayerAffinity Type { get; set; }
        public int Quantity { get; set; }
    }
}