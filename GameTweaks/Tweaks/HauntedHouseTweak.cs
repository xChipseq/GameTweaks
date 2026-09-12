using GameTweaks.Modifiers;
using GameTweaks.Options;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class HauntedHouseTweak : AbstractGameTweak
{
    public override string Name => MiraLocaleManager.Get("TweakHauntedHouse");
    public override Color Color => TweakPalette.HauntedHouseColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.HauntedHouseTweak;

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<HauntedHouseTweak>())
        {
            return;
        }

        @event.Player.AddModifier<VisibleGhostModifier>();
    }
}