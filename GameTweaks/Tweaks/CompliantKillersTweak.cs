using GameTweaks.Modifiers;
using GameTweaks.Options;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class CompliantKillersTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakCompliantKillers");
    public override Color Color => TweakPalette.CompliantKillersColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.CompliantKillersTweak;

    [RegisterEvent]
    public static void ChangeRoleEvent(ChangeRoleEvent @event)
    {
        if (GameTweaksManager.Instance == null)
        {
            return;
        }
        if (!GameTweaksManager.Instance.IsActive<CompliantKillersTweak>())
        {
            return;
        }

        var player = @event.Player;
        if (player.HasModifier<CompliantKillerModifier>())
        {
            return;
        }

        if (@event.NewRole.GetRoleAlignment() != RoleAlignment.NeutralKilling)
        {
            return;
        }

        player.AddModifier<CompliantKillerModifier>();
    }
}