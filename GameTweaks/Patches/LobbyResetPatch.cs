using HarmonyLib;

namespace GameTweaks.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyResetPatch
{
    private static void Postfix()
    {
        GameTweaksManager.Instance = null;
    }
}