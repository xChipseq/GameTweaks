using GameTweaks.Tweaks;
using HarmonyLib;
using MiraAPI.Utilities;
using UnityEngine;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public static class DivineInterventionPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
    private static void RoomTrackerPatch(RoomTracker __instance)
    {
        try
        {
            if (LobbyBehaviour.Instance)
            {
                return;
            }
            if (MeetingHud.Instance)
            {
                return;
            }
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null)
            {
                return;
            }

            var room = Helpers.GetRoom(localPlayer.GetTruePosition());
            if (room == null)
            {
                return;
            }

            __instance.text.color = room == DivineInterventionTweak.ProtectedRoom ? TweakPalette.DivineInterventionColor : Color.white;
        }
        catch
        {
            // uhmmmm
        }
    }
}