using GrimBuilding.Common.Support;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrimBuilding.Converters
{
    class ItemTypeToDisplayText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is null ? null :
            (ItemType)value switch
            {
                ItemType.Feet => "Boots",
                ItemType.WeaponCrossbow => "Crossbow",
                ItemType.WeaponOneHandedAxe => "One-Handed Axe",
                ItemType.WeaponOneHandedSword => "One-Handed Sword",
                ItemType.WeaponOneHandedMace => "One-Handed Mace",
                ItemType.WeaponOneHandedGun => "One-Handed Ranged",
                ItemType.WeaponDagger => "One-Handed Dagger",
                ItemType.WeaponTwoHandedAxe => "Two-Handed Axe",
                ItemType.WeaponTwoHandedSword => "Two-Handed Sword",
                ItemType.WeaponTwoHandedMace => "Two-Handed Mace",
                ItemType.WeaponTwoHandedGun => "Two-Handed Ranged",
                ItemType.OffhandFocus => "Off-Hand",
                _ => value.ToString(),
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}