using GameTweaks.Modifiers;
using GameTweaks.Tweaks;
using GameTweaks.Utilities;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public class VipDisconnectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static void HandleDisconnectPrefix(PlayerControl player)
    {
        if (GameTweaksManager.Instance == null || !GameTweaksManager.Instance.IsActive<VipTweak>())
        {
            return;
        }
        if (!player.HasModifier<VipModifier>())
        {
            return;
        }

        if (AmongUsClient.Instance.AmHost)
            VipTweak.PickVip();
        TweakHelpers.Notify(MiraLocaleManager.Get("TweakVipDisconnected"), TweakPalette.VipColor);
    }
}