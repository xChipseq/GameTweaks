using GameTweaks.Modifiers;
using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Neutral;

namespace GameTweaks.Patches;

[HarmonyPatch]
public static class CheckPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VampireBiteButton), "ConvertCheck")]
    private static void ConvertCheckPostfix(PlayerControl target, ref bool __result)
    {
        if (target.HasModifier<VipModifier>())
        {
            __result = false;
        }
    }
}