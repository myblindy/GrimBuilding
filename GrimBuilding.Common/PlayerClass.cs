﻿using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerSkill
    {
        public string Name { get; set; }
        public string BitmapUpPath { get; set; }
        public byte[] BitmapUp { get; set; }
        public string BitmapDownPath { get; set; }
        public byte[] BitmapDown { get; set; }
    }

    public class PlayerClass
    {
        public string Name { get; set; }
        public PlayerSkill[] Skills { get; set; }
    }
}