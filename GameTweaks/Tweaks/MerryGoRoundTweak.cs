using System.Collections.Generic;
using AmongUs.GameOptions;
using GameTweaks.Options;
using GameTweaks.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class MerryGoRoundTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakMerryGoRound");
    public override Color Color => TweakPalette.MerryGoRoundColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.MerryGoRoundTweak;

    public override void OnRoundStart(bool gameStart)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        if (gameStart)
        {
            return;
        }

        var crewmatePlayers = TweakHelpers.GetCrewmates(false);
        var impostorPlayers = TweakHelpers.GetImpostors(false);
        ShufflePlayerRoles(crewmatePlayers);
        ShufflePlayerRoles(impostorPlayers);
        // neutral players have our mercy
    }

    private static void ShufflePlayerRoles(List<PlayerControl> team)
    {
        var players = new List<PlayerControl>();
        var roles = new List<RoleTypes>();

        foreach (var player in team)
        {
            players.Add(player);
            roles.Add(player.Data.RoleType);
        }

        players.Shuffle();
        roles.Shuffle();

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var role = roles[i];
            if (player.Data.RoleType == role)
                continue;
            player.RpcChangeRole((ushort)role, false);
        }
    }
}