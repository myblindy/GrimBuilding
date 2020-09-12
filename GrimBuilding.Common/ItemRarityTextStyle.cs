namespace GrimBuilding.Common.Support
{
    public class ItemRarityTextStyle
    {
        public int Id { get; set; }

        public ItemRarity Rarity { get; set; }

        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }

        public string FontName { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
    }
}