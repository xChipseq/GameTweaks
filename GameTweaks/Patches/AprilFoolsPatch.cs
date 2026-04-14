using HarmonyLib;

namespace GameTweaks.Patches;

[HarmonyPriority(Priority.Last)]
[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldFlipSkeld))]
public static class AprilFoolsPatch
{
    private static void Postfix(ref bool __result)
    {
        __result = !GameTweaksPlugin.DevMode; // pmo
    }
}