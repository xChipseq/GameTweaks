using System.Collections.Generic;
using System.Linq;
using GameTweaks.Modifiers;
using GameTweaks.Options;
using GameTweaks.Utilities;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class ImpostoromiconTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakImpostoromicon");
    public override Color Color => TweakPalette.ImpostoromiconColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.ImpostoromiconTweak;

    private static Dictionary<PlayerControl, PlayerControl> votes { get; } = new();
    private static MeetingMenu? impostoromiconMenu { get; set; }

    public override void Start()
    {
        impostoromiconMenu = null;
        votes.Clear();
    }

    [RegisterEvent]
    public static void SetRoleEvent(SetRoleEvent @event)
    {
        if (GameTweaksManager.Instance == null)
        {
            return;
        }
        if (!GameTweaksManager.Instance.IsActive<ImpostoromiconTweak>())
        {
            return;
        }
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        if (!TutorialManager.InstanceExists)
        {
            return;
        }

        AssignImpostoromiconIfNone();
    }

    [RegisterEvent]
    public static void ChangeRoleEvent(ChangeRoleEvent @event)
    {
        if (GameTweaksManager.Instance == null)
        {
            return;
        }
        if (!GameTweaksManager.Instance.IsActive<ImpostoromiconTweak>())
        {
            return;
        }
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        if (!TutorialManager.InstanceExists)
        {
            return;
        }

        AssignImpostoromiconIfNone();
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<ImpostoromiconTweak>())
        {
            return;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (!localPlayer.IsImpostor())
        {
            return;
        }

        impostoromiconMenu ??= new MeetingMenu(
            localPlayer.Data.Role,
            OnClick,
            "0",
            MeetingAbilityType.Toggle,
            TweakAssets.ImpostoromiconButtonActive,
            TweakAssets.ImpostoromiconButton,
            IsExempt,
            position: new Vector3(1.1f, -0.15f, -3)
        );

        votes.Clear();
        impostoromiconMenu.GenButtons(@event.MeetingHud, true);

        var current = GetCurrentHolder();
        if (current != null)
        {
            impostoromiconMenu.Actives[current.PlayerId] = true;
        }
    }

    public override void OnRoundStart(bool gameStart)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        if (gameStart)
        {
            AssignImpostoromiconIfNone();
            return;
        }
        if (votes.Count == 0)
        {
            return;
        }

        var voteCounts = new Dictionary<PlayerControl, int>();
        foreach (var (_, target) in votes)
        {
            voteCounts.TryAdd(target, 0);
            voteCounts[target]++;
        }

        voteCounts.Shuffle();
        var winner = voteCounts.MaxBy(x => x.Value).Key;
        var current = GetCurrentHolder();
        if (current == winner)
        {
            return;
        }
        if (winner.HasDied())
        {
            return;
        }

        RpcSetImpostoromiconHolder(winner);
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent @event)
    {
        impostoromiconMenu?.HideButtons();
    }

    [MethodRpc((uint)TweakRpcCalls.ImpostoromiconVotePlayer)]
    public static void RpcImpostoromiconVote(PlayerControl voter, PlayerControl target)
    {
        if (!voter.IsImpostor() || !target.IsImpostor())
        {
            Error($"RpcImpostoromiconVote: Invalid impostor voter/target");
            return;
        }

        votes[voter] = target;
        if (PlayerControl.LocalPlayer.IsImpostorAligned())
        {
            TweakHelpers.Notify(
                voter == target
                    ? TouLocale.GetParsed("TweakImpostoromiconVoteThemself").Replace("<player>", voter.Data.PlayerName)
                    : TouLocale.Get("TweakImpostoromiconVoteMate")
                        .Replace("<voter>", voter.Data.PlayerName)
                        .Replace("<player>", target.Data.PlayerName),
                TweakPalette.ImpostoromiconColor);
        }

        if (impostoromiconMenu != null)
        {
            impostoromiconMenu.Buttons.Do(x =>
            {
                if (x.Value == null) return; // for some reason, empty buttons are present in the list???
                var text = x.Value.GetComponentInChildren<TextMeshPro>();
                text.text = votes.Count(v => v.Value.PlayerId == x.Key).ToString();
            });
        }
    }

    [MethodRpc((uint)TweakRpcCalls.ImpostoromiconSetHolder)]
    public static void RpcSetImpostoromiconHolder(PlayerControl holder)
    {
        ModifierUtils.GetPlayersWithModifier<ImpostoromiconModifier>()
            .Do(x => x.RemoveModifier<ImpostoromiconModifier>());
        holder.AddModifier<ImpostoromiconModifier>();

        if (PlayerControl.LocalPlayer.IsImpostorAligned())
        {
            TweakHelpers.Notify(
                TouLocale.GetParsed("TweakImpostoromiconGain").Replace("<player>", holder.Data.PlayerName),
                TweakPalette.ImpostoromiconColor);
        }
    }

    private static void OnClick(PlayerVoteArea voteArea, MeetingHud hud)
    {
        var target = voteArea.GetPlayer()!;
        if (votes.GetValueOrDefault(PlayerControl.LocalPlayer) == target)
        {
            return;
        }

        RpcImpostoromiconVote(PlayerControl.LocalPlayer, target);
    }

    private static bool IsExempt(PlayerVoteArea voteArea)
    {
        return !voteArea.GetPlayer()!.IsImpostor();
    }

    private static void AssignImpostoromiconIfNone()
    {
        if (GetCurrentHolder() != null)
        {
            return;
        }

        var impostors = TweakHelpers.GetImpostors(false);
        if (impostors.Count == 0)
        {
            return;
        }

        var chosen = impostors.Random()!;
        RpcSetImpostoromiconHolder(chosen);
    }

    private static PlayerControl? GetCurrentHolder()
    {
        var current = ModifierUtils.GetPlayersWithModifier<ImpostoromiconModifier>().ToList();
        return current.Count == 0 ? null : current[0];
    }
}