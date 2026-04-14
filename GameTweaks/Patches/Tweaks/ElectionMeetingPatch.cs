using System.Globalization;
using System.Linq;
using GameTweaks.Modifiers;
using GameTweaks.Tweaks;
using GameTweaks.Utilities;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch(typeof(MeetingHud))]
public static class ElectionMeetingPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static void UpdatePostfix(MeetingHud __instance)
    {
        if (!GameTweaksManager.Instance!.IsActive<ElectionTweak>())
        {
            return;
        }

        if (!ElectionTweak.currentlyElection)
        {
            return;
        }

        __instance.SkipVoteButton.gameObject.SetActive(false);
        CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>().Do(x => x.HideProsButton = true);
        var options = GameManager.Instance.LogicOptions.TryCast<LogicOptionsNormal>()!;
        if (__instance.state is MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Voted)
        {
            var num2 = __instance.discussionTimer - options.GetDiscussionTime();
            var num3 = Mathf.Max(0f, ElectionTweak.ElectionTime - num2);
            __instance.TimerText.text = TouLocale.GetParsed("TweakElectionTimer").Replace("<timer>", Mathf.CeilToInt(num3).ToString());
            if (num2 >= ElectionTweak.ElectionTime)
            {
                ElectionTweak.currentlyElection = false;
                __instance.discussionTimer -= ElectionTweak.ElectionTime;
                __instance.SkipVoteButton.gameObject.SetActive(true);

                var votes = VotingUtils.CalculateVotes();
                var forPlayer = VotingUtils.CalculateNumVotes(votes);
                if (!forPlayer.Any())
                {
                    return;
                }

                forPlayer.Shuffle(); // this makes ties randomly pick instead of selecting the first player
                var top = forPlayer.MaxBy(x => x.Value);
                var player = TweakHelpers.PlayerById(top.Key);
                player!.AddModifier<SovereignModifier>((int)top.Value);
                __instance.playerStates.Do(x =>
                {
                    x.ThumbsDown.enabled = false;
                    x.UnsetVote();
                    var spreader = x.GetComponent<VoteSpreader>();
                    spreader.Votes.ToArray().Do(s => s.gameObject.Destroy());
                    spreader.Votes.Clear();
                });
                PlayerControl.AllPlayerControls.ToArray().Do(x =>
                {
                    var data = x.GetVoteData();
                    data.Votes.ToArray().Do(data.RemovePlayerVote);
                    data.SetRemainingVotes(1);

                    var dummy = x.GetComponent<DummyBehaviour>();
                    if (dummy) dummy.voted = false;
                });
                __instance.ClearVote();
                CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>().Do(x =>
                {
                    if (!x.Player.HasModifier<JailedModifier>())
                        x.HideProsButton = false;
                });

                var winMsg = TouLocale.GetParsed("TweakElectionWin")
                    .Replace("<player>", player!.Data.PlayerName)
                    .Replace("<votes>", top.Value.ToString(CultureInfo.InvariantCulture));
                TweakHelpers.Notify(winMsg,
                    TweakPalette.ElectionColor);
            }
        }
    }
}