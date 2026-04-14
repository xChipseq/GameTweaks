using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using GameTweaks.Tweaks;
using HarmonyLib;
using Reactor.Utilities.Extensions;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public class TacticalDeploymentPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    private static bool WrapUpPrefix(ExileController __instance)
    {
        if (!TacticalDeploymentTweak.IsMapValid() || !GameTweaksManager.Instance!.IsActive<TacticalDeploymentTweak>())
        {
            return true;
        }

        if (__instance.initData.networkedPlayer != null)
        {
            var player = __instance.initData.networkedPlayer.Object;
            if (player)
            {
                player.Exiled();
            }
            __instance.initData.networkedPlayer.IsDead = true;
        }
        if (DestroyableSingleton<TutorialManager>.InstanceExists || (GameManager.Instance != null && !GameManager.Instance.LogicFlow.IsGameOverDueToDeath()))
        {
            __instance.StartCoroutine(CoCustomWrapUp(__instance).WrapToIl2Cpp());
            return false;
        }
        __instance.gameObject.Destroy();
        return false;
    }

    private static IEnumerator CoCustomWrapUp(ExileController __instance)
    {
        yield return ShipStatus.Instance.PrespawnStep();
        __instance.ReEnableGameplay();
        __instance.gameObject.Destroy();
    }

    [HarmonyPatch]
    private class PrespawnStepPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return new[] { typeof(SkeldShipStatus), typeof(PolusShipStatus), typeof(MiraShipStatus) }
                .Select(x => AccessTools.Method(x, nameof(ShipStatus.PrespawnStep)));
        }

        private static bool Prefix(ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!TacticalDeploymentTweak.IsMapValid() || !GameTweaksManager.Instance!.IsActive<TacticalDeploymentTweak>())
            {
                return true;
            }

            __result = TacticalDeploymentTweak.PickDeployment().WrapToIl2Cpp();
            return false;
        }
    }
}