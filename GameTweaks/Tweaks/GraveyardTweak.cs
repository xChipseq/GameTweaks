using System.Collections.Generic;
using AmongUs.GameOptions;
using GameTweaks.Modifiers;
using GameTweaks.Options;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class GraveyardTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakGraveyard");
    public override Color Color => TweakPalette.GraveyardColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.GraveyardTweak;

    private static Queue<(PlayerControl, RoleTypes)> graveyardQueue { get; } = new();

    public override void Start()
    {
        graveyardQueue.Clear();
    }

    [RegisterEvent(100)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<GraveyardTweak>())
        {
            return;
        }

        // the player's role is not revealed instantly, only during the next meeting
        var player = @event.Player;
        var role = player.GetRoleWhenAlive().Role;
        graveyardQueue.Enqueue((player, role));
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<GraveyardTweak>())
        {
            return;
        }

        while (graveyardQueue.Count > 0)
        {
            var (player, role) = graveyardQueue.Dequeue();
            player.AddModifier<GraveyardRevealModifier>((ushort)role);
        }
    }
}