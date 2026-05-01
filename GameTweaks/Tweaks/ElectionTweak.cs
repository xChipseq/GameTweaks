using System.Linq;
using GameTweaks.Modifiers;
using GameTweaks.Options;
using GameTweaks.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class ElectionTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakElection");
    public override Color Color => TweakPalette.ElectionColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.ElectionTweak;

    public const float ElectionTime = 20f; // 20 second erection
    public static bool currentlyElection;

    public override void Start()
    {
        currentlyElection = false;
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        if (!GameTweaksManager.Instance!.IsActive<ElectionTweak>())
        {
            return;
        }

        var modifiers = ModifierUtils.GetPlayersWithModifier<SovereignModifier>().ToList();
        if (modifiers.Count > 0)
        {
            return;
        }

        currentlyElection = true;
    }

    [RegisterEvent]
    public static void CheckForEndVotingEventHandler(CheckForEndVotingEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<ElectionTweak>())
        {
            return;
        }

        if (currentlyElection)
        {
            @event.Cancel();
        }
    }

    [RegisterEvent(-100)]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<ElectionTweak>())
        {
            return;
        }

        // no self votes or skips in election (you can't if not cheating but just in case)
        if (currentlyElection && (@event.Player.PlayerId == @event.TargetId || @event.TargetId == TweakHelpers.SkipVoteId))
        {
            @event.Cancel();
            return;
        }

        if (!@event.Player.TryGetModifier<SovereignModifier>(out var sovereign))
        {
            return;
        }

        // vote count + 1 base vote
        // note: mayor votes & knights should be fine, too bad if not
        @event.VoteData.SetRemainingVotes(0);
        for (var i = 0; i <= sovereign.VoteCount; i++)
        {
            @event.VoteData.VoteForPlayer(@event.TargetId);
        }
        @event.Cancel();
    }

    [RegisterEvent(100)]
    public static void MeetingSelectEventHandler(MeetingSelectEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<ElectionTweak>())
        {
            return;
        }
        if (!currentlyElection)
        {
            return;
        }

        @event.AllowSelect = @event.TargetId != PlayerControl.LocalPlayer.PlayerId;
    }

    [RegisterEvent]
    public static void DummyVoteEventHandler(DummyVoteEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<ElectionTweak>())
        {
            return;
        }

        if (currentlyElection)
            @event.CanSkip = false;
    }
}