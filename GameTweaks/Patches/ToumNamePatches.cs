using GameTweaks.Modifiers;
using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Utilities;

namespace GameTweaks.Patches;

[HarmonyPatch]
public static class ToumNamePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateStatusSymbols))]
    public static void UpdateStatusSymbolsPrefix(ref string name, PlayerControl player)
    {
        if (player.HasModifier<SovereignModifier>())
        {
            name += $" {TweakPalette.ElectionColor.ToTextColor()}①</color>";
        }
    }
}