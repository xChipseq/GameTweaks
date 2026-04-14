using System.Collections;
using System.Linq;
using GameTweaks.GameOvers;
using GameTweaks.Modifiers;
using GameTweaks.Options;
using GameTweaks.Utilities;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class VipTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakVip");
    public override Color Color => TweakPalette.VipColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.VipTweak;

    public override void OnRoundStart(bool gameStart)
    {
        if (!AmongUsClient.Instance.AmHost) // host assigns the vip
        {
            return;
        }
        if (ModifierUtils.GetPlayersWithModifier<VipModifier>().Any())
        {
            return;
        }

        PickVip();
    }

    public static void PickVip()
    {
        var possiblePlayers = TweakHelpers
            .GetCrewmates(false)
            .Where(x => x.Data.Role is not AltruistRole or SheriffRole or ProsecutorRole or ImitatorRole or VigilanteRole or SnitchRole &&
                        TweakHelpers.QualifiesAsCrew(x) &&
                        !x.HasModifier<ExecutionerTargetModifier>() &&
                        !x.HasModifier<LoverModifier>()) // (was wondering about this one but vip lover seems unfair)
            .ToList();
        if (possiblePlayers.Count == 0)
        {
            Warning("No valid players found for VIP");
            return;
        }

        var selectedPlayer = possiblePlayers.Random()!;
        var role = selectedPlayer.GetRoleWhenAlive();
        selectedPlayer.RpcAddModifier<VipModifier>((ushort)role.Role);
    }

    [RegisterEvent(100)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<VipTweak>())
        {
            return;
        }
        if (!@event.Player.HasModifier<VipModifier>())
        {
            return;
        }

        if (@event.DeathReason == DeathReason.Exile) // brutally murder all crewmates
        {
            TweakHelpers.MurderAllCrew();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<VipTweak>())
        {
            return;
        }

        var killer = @event.Source;
        var victim = @event.Target;
        if (!victim.HasModifier<VipModifier>())
        {
            return;
        }

        if (killer.IsImpostorAligned()) // impostors killed the vip, so they win
        {
            if (AmongUsClient.Instance.AmHost)
            {
                var impostors = TweakHelpers
                    .GetImpostors()
                    .Select(x => x.Data)
                    .ToArray();
                Coroutines.Start(CoVipDeadWin(impostors));
            }
            return;
        }

        if (killer.Data.Role is VampireRole) // special case for vamps cuz they're in a team too
        {
            if (AmongUsClient.Instance.AmHost)
            {
                var vamps = CustomRoleUtils
                    .GetActiveRolesOfType<VampireRole>()
                    .Select(x => x.Player.Data)
                    .ToArray();
                Coroutines.Start(CoVipDeadWin(vamps));
            }
            return;
        }

        if (killer.IsNeutral()) // a neutral role killed the vip, they win on their own
        {
            if (AmongUsClient.Instance.AmHost)
            {
                Coroutines.Start(CoVipDeadWin([killer.Data]));
            }
            return;
        }

        // otherwise, we once again brutally murder the crew
        TweakHelpers.MurderAllCrew();
        TweakHelpers.Notify(TouLocale.Get("TweakVipCrewMurdered"), Palette.ImpostorRed);
    }

    private static IEnumerator CoVipDeadWin(NetworkedPlayerInfo[] winners)
    {
        winners.Do(x => x.Object.RpcAddModifier<VipKillerModifier>());
        yield return new WaitForSeconds(1);
        CustomGameOver.Trigger<VipDeadGameOver>(winners);
    }
}