using GameTweaks.Modifiers;
using GameTweaks.Options;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public class BrutalCrewTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakBrutalCrew");
    public override Color Color => TweakPalette.BrutalCrewColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.BrutalCrewTweak;

    [RegisterEvent]
    public static void SetRoleEvent(SetRoleEvent @event)
    {
        if (GameTweaksManager.Instance == null)
        {
            return;
        }
        if (!GameTweaksManager.Instance.IsActive<BrutalCrewTweak>())
        {
            return;
        }
        if (TutorialManager.InstanceExists)
        {
            return;
        }

        MakeBrutalIfNot(@event.Player);
    }

    [RegisterEvent]
    public static void ChangeRoleEvent(ChangeRoleEvent @event)
    {
        if (GameTweaksManager.Instance == null)
        {
            return;
        }
        if (!GameTweaksManager.Instance.IsActive<BrutalCrewTweak>())
        {
            return;
        }

        MakeBrutalIfNot(@event.Player);
    }

    private static void MakeBrutalIfNot(PlayerControl player)
    {
        var role = player.Data.Role;
        if (!role.IsCrewmate() || player.HasModifier<BrutalCrewModifier>())
        {
            return;
        }

        player.AddModifier<BrutalCrewModifier>();
    }
}