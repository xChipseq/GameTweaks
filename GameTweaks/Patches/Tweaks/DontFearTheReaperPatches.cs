using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameTweaks.Tweaks;
using HarmonyLib;
using MiraAPI.Hud;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Modules;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public class DontFearTheReaperPatches
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static void HudUpdatePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }
        if (GameTweaksManager.Instance == null || !GameTweaksManager.Instance.IsActive<DontFearTheReaperTweak>())
        {
            return;
        }
        if (!DontFearTheReaperTweak.AbilitiesActive)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        var role = local.GetRoleWhenAlive();
        foreach (var button in CustomButtonManager.Buttons)
        {
            if (!button.Enabled(role))
            {
                continue;
            }

            button.Button?.ToggleVisible(true);
            button.FixedUpdateHandler(local);
        }
    }

    [HarmonyPatch]
    private class DeathUsePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetTypesFromAssembly(typeof(TownOfUsPlugin).Assembly)
                .Where(type =>
                    type.IsSubclassOf(typeof(TownOfUsButton)) || type.IsSubclassOf(typeof(TownOfUsTargetButton<>)))
                .Select(type => AccessTools.PropertyGetter(type, nameof(TownOfUsButton.UsableInDeath)));
        }

        private static void Postfix(ref bool __result)
        {
            if (DontFearTheReaperTweak.AbilitiesActive)
            {
                __result = true;
            }
        }
    }
}