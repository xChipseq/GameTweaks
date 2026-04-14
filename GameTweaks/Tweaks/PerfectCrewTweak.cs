using GameTweaks.Options;
using GameTweaks.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class PerfectCrewTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakPerfectCrew");
    public override Color Color => TweakPalette.PerfectCrewColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.PerfectCrewTweak;

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<PerfectCrewTweak>())
        {
            return;
        }

        if (@event.DeathReason == DeathReason.Exile && TweakHelpers.QualifiesAsCrew(@event.Player))
        {
            TweakHelpers.MurderAllCrew();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<PerfectCrewTweak>())
        {
            return;
        }

        var killer = @event.Source;
        var victim = @event.Target;
        if (killer != victim && TweakHelpers.QualifiesAsCrew(killer) && TweakHelpers.QualifiesAsCrew(victim))
        {
            TweakHelpers.MurderAllCrew();
        }
    }
}