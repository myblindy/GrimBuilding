﻿using LiteDB;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrimBuilding.Common.Support
{
    public class PlayerClass
    {
        public int PlayerClassId { get; set; }

        public string Name { get; set; }

        [BsonRef]
        public PlayerSkill[] Skills { get; set; }
    }
}