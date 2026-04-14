using GameTweaks.Modifiers;
using GameTweaks.Tweaks;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
[HarmonyPriority(Priority.Last)]
public static class GraveyardReportPatch
{
    public static void Postfix(MeetingIntroAnimation __instance, Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
    {
        if (!GameTweaksManager.Instance!.IsActive<GraveyardTweak>())
        {
            return;
        }

        // Why: the states in intro animations have no player assigned to them, only cosmetics are applied ...
        for (var i = 0; i < __instance.deadCards.Count; i++)
        {
            var data = deadBodies[i];
            if (data == null)
            {
                continue;
            }
            var card = __instance.deadCards[i];
            var player = data.Object;
            var reveal = player.GetModifier<GraveyardRevealModifier>();
            if (reveal == null)
            {
                continue;
            }

            var role = reveal.ShownRole!;
            var color = role.TeamColor;
            var roleName = $"<size=80%>{color.ToTextColor()}{role.GetRoleName()}</color></size>\n{card.NameText.text}";
            card.NameText.text = roleName;
        }
    }
}