﻿
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrimBuilding.Common.Support
{
    public class PlayerSkill
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string BitmapUpPath { get; set; }
        [NotMapped]
        public byte[] BitmapUp { get; set; }
        public string BitmapDownPath { get; set; }
        [NotMapped]
        public byte[] BitmapDown { get; set; }

        public string BitmapFrameUpPath { get; set; }
        [NotMapped]
        public byte[] BitmapFrameUp { get; set; }
        public string BitmapFrameInFocusPath { get; set; }
        [NotMapped]
        public byte[] BitmapFrameInFocus { get; set; }
        public string BitmapFrameDownPath { get; set; }
        [NotMapped]
        public byte[] BitmapFrameDown { get; set; }

        public List<string> BitmapSkillConnectionOffPaths { get; set; }
        [NotMapped]
        public byte[][] BitmapSkillConnectionsOff { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int MaximumLevel { get; set; }
        public int? UltimateLevel { get; set; }
        public int? MasteryLevelRequirement { get; set; }
        public bool Circular { get; set; }

        public List<BaseStats> BaseStatLevels { get; set; }
    }
}