using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BLTAdoptAHero.Actions.Util
{
    public static class ItemModifierAccess
    {
        public static void SetName(this ItemModifier item, TextObject name) =>
            AccessTools.Property(typeof(ItemModifier), nameof(ItemModifier.Name)).SetValue(item, name);
        public static void SetDamageModifier(this ItemModifier item, int value)
        {
            AccessTools.DeclaredProperty(typeof(ItemModifier), "Damage").SetValue(item, value);
        }

        public static void SetSpeedModifier(this ItemModifier item, int value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "Speed").SetValue(item, value);
        public static void SetMissileSpeedModifier(this ItemModifier item, int value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "MissileSpeed").SetValue(item, value);
        public static void SetArmorModifier(this ItemModifier item, int value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "Armor").SetValue(item, value);
        public static void SetHitPointsModifier(this ItemModifier item, short value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "HitPoints").SetValue(item, value);
        public static void SetStackCountModifier(this ItemModifier item, short value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "StackCount").SetValue(item, value);
        public static void SetMountSpeedModifier(this ItemModifier item, float value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "MountSpeed").SetValue(item, value);
        public static void SetManeuverModifier(this ItemModifier item, float value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "Maneuver").SetValue(item, value);
        public static void SetChargeDamageModifier(this ItemModifier item, float value) =>
            AccessTools.DeclaredProperty(typeof(ItemModifier), "ChargeDamage").SetValue(item, value);
        public static void SetMountHitPointsModifier(this ItemModifier item, float value) => AccessTools.DeclaredProperty(typeof(ItemModifier), "MountHitPoints").SetValue(item, value);
    }
}