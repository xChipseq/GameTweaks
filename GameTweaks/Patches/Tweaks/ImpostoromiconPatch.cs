using GameTweaks.Modifiers;
using GameTweaks.Tweaks;
using GameTweaks.Utilities;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public static class ImpostoromiconPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last+1)]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    private static void UpdatePostfix(HudManager __instance)
    {
        if (MeetingHud.Instance)
        {
            return;
        }
        if (GameTweaksManager.Instance == null || !GameTweaksManager.Instance.IsActive<ImpostoromiconTweak>())
        {
            return;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer.HasDied())
        {
            return;
        }
        if (!localPlayer.IsImpostor())
        {
            return;
        }

        var hasImpostoromicon = localPlayer.HasModifier<ImpostoromiconModifier>();
        TweakHelpers.ToggleKillButtons(hasImpostoromicon, false, false);
        if (PlayerControl.LocalPlayer.IsRole<HerbalistRole>())
            CustomButtonSingleton<HerbalistAbilityKillButton>.Instance.Button!.ToggleVisible(hasImpostoromicon);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    private static bool DoClickPrefix()
    {
        if (MeetingHud.Instance)
        {
            return true;
        }
        if (GameTweaksManager.Instance == null || !GameTweaksManager.Instance.IsActive<ImpostoromiconTweak>())
        {
            return true;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer.HasDied())
        {
            return true;
        }
        if (!localPlayer.IsImpostor())
        {
            return true;
        }

        var hasImpostoromicon = localPlayer.HasModifier<ImpostoromiconModifier>();
        return hasImpostoromicon;
    }
}