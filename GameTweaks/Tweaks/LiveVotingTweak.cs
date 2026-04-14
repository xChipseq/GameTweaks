using System.Collections.Generic;
using System.Linq;
using GameTweaks.Modifiers;
using GameTweaks.Options;
using GameTweaks.Utilities;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class LiveVotingTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakLiveVoting");
    public override Color Color => TweakPalette.LiveVotingColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.LiveVotingTweak;

    private static Dictionary<byte, List<(byte, GameObject)>> votes { get; } = new(); // target = [(voter, bloop)]

    public override void OnRoundStart(bool gameStart)
    {
        votes.Clear();
    }

    [RegisterEvent(-100)]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<LiveVotingTweak>())
        {
            return;
        }
        if (@event.IsCancelled)
        {
            return;
        }

        if (@event.TargetId == 252) // Blackmailed
        {
            return;
        }

        DoTheBloop(@event.Player, @event.TargetId);

        // kill me
        var hud = MeetingHud.Instance;
        hud.SkippedVoting.SetActive(true);
        hud.SkippedVoting.GetComponentInChildren<TextMeshPro>().enabled = false;
    }

    private static void RegisterBloopAVoteIcon(MeetingHud hud, Transform state, byte target, NetworkedPlayerInfo voter)
    {
        hud.BloopAVoteIcon(voter, 0, state);
        var spreader = state.GetComponent<VoteSpreader>();
        if (spreader == null)
            return;
        var last = spreader.Votes[^1];
        votes.TryAdd(target, new());
        votes[target].Add((voter.PlayerId, last.gameObject));
    }

    private static void DoTheBloop(PlayerControl voter, byte target)
    {
        var hud = MeetingHud.Instance;
        var state = target == TweakHelpers.SkipVoteId
            ? hud.SkippedVoting.transform
            : hud.playerStates.FirstOrDefault(x => x.TargetPlayerId == target)!.transform;
        if (state == null)
        {
            return;
        }

        // I haven't really found a way to reliably track how many votes a player has due to
        // how differently they are implemented across roles & mods, so we just check for toum roles.
        if (voter.Data.Role is MayorRole mayor && mayor.Revealed)
        {
            for (var i = 0; i < 3; i++)
                RegisterBloopAVoteIcon(hud, state, target, voter.Data);
        }
        else
        {
            RegisterBloopAVoteIcon(hud, state, target, voter.Data);
        }

        var knights = voter.GetModifiers<KnightedModifier>().ToList();
        if (knights.Count > 0)
        {
            for (var i = 0; i <= knights.Count; i++)
                RegisterBloopAVoteIcon(hud, state, target, voter.Data);
        }
        if (voter.TryGetModifier<SovereignModifier>(out var sovereign))
        {
            for (var i = 0; i < sovereign.VoteCount; i++)
                RegisterBloopAVoteIcon(hud, state, target, voter.Data);
        }
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<LiveVotingTweak>())
        {
            return;
        }
        if (!MeetingHud.Instance)
        {
            return;
        }

        var id = @event.Player.PlayerId;
        var playerVotes = votes
            .Where(x => x.Value.Any(z => z.Item1 == id))
            .ToList();
        foreach (var (target, _) in playerVotes)
        {
            var pva = target == TweakHelpers.SkipVoteId
                ? MeetingHud.Instance.SkipVoteButton
                : MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == target);
            if (pva == null)
                continue;
            var spreader = target == TweakHelpers.SkipVoteId
                ? MeetingHud.Instance.SkippedVoting.GetComponent<VoteSpreader>()
                : pva.GetComponent<VoteSpreader>();
            spreader.Votes.ToArray().Do(x => x?.gameObject.Destroy());
            spreader.Votes.Clear();

            var vootes = VotingUtils.CalculateVotes().Where(x => x.Suspect == target).ToList();
            if (vootes.Count == 0)
                continue;

            foreach (var vote in vootes)
            {
                var voter = MiscUtils.PlayerById(vote.Voter);
                DoTheBloop(voter!, target);
            }
        }

        if (!votes.TryGetValue(id, out var votesForPlayer))
        {
            return;
        }
        votesForPlayer.Do(x => x.Item2?.Destroy());
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<LiveVotingTweak>())
        {
            return;
        }

        @event.MeetingHud.SkippedVoting.GetComponentInChildren<TextMeshPro>().enabled = true;
    }

    [RegisterEvent]
    public static void PopulateResultsEventHandler(PopulateResultsEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<LiveVotingTweak>())
        {
            return;
        }

        // we rebloop the votes if someone was prosecuted
        if (CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>().Any(x => x.HasProsecuted))
        {
            MeetingHud.Instance.playerStates.Do(x =>
            {
                var spreader = x.GetComponent<VoteSpreader>();
                spreader.Votes.ToArray().Do(s => s.gameObject.Destroy());
                spreader.Votes.Clear();
            });
            return;
        }

        @event.Cancel();
        MeetingHud.Instance.TitleText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults);
    }
}