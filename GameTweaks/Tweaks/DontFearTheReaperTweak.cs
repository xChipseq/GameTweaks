using GameTweaks.Options;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using TownOfUs.Modules.Localization;
using UnityEngine;

namespace GameTweaks.Tweaks;

// TODO: Make dead players behave like haunter/phantom.
// TODO: you could also make it actually work
public sealed class DontFearTheReaperTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakDontFearTheReaper");
    public override Color Color => TweakPalette.DontFearTheReaperColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.DontFearTheReaperTweak;

    public static bool AbilitiesActive { get; set; }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!@event.Player.AmOwner)
        {
            return;
        }
        if (!GameTweaksManager.Instance!.IsActive<DontFearTheReaperTweak>())
        {
            return;
        }
        if (@event.DeathReason != DeathReason.Kill)
        {
            return;
        }

        AbilitiesActive = true;
    }
}