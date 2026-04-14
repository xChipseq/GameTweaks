using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameTweaks.Tweaks;
using HarmonyLib;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public static class CompliantKillersPatches
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.Last)]
    private class NeutralKillButtonsPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetTypesFromAssembly(typeof(TownOfUsPlugin).Assembly)
                .Where(type => type.IsAssignableTo(typeof(IKillButton)) && !type.IsAbstract)
                .Select(type =>
                    AccessTools.FindIncludingBaseTypes(type, t => AccessTools.DeclaredMethod(t, "GetTarget")))
                .Where(m => m != null);
        }

        private static void Postfix(object __instance, ref PlayerControl? __result)
        {
            if (!__result) // so basically: if OG code found no targets, we won't find ours either (im so smart)
            {
                return;
            }
            if (!GameTweaksManager.Instance || !GameTweaksManager.Instance!.IsActive<CompliantKillersTweak>())
            {
                return;
            }

            var local = PlayerControl.LocalPlayer;
            if (local.Data.Role.GetRoleAlignment() != RoleAlignment.NeutralKilling)
            {
                return;
            }

            if (__result!.Data.Role.GetRoleAlignment() == RoleAlignment.NeutralKilling)
            {
                __result = null;
            }
        }
    }
}