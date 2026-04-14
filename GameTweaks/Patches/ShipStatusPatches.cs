using HarmonyLib;

namespace GameTweaks.Patches;

[HarmonyPatch(typeof(ShipStatus))]
public static class ShipStatusPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    private static void AwakePostfix(ShipStatus __instance)
    {
        if (__instance.GetComponent<GameTweaksManager>())
        {
            return;
        }

        __instance.gameObject.AddComponent<GameTweaksManager>();
    }
}