using GameTweaks.Buttons;
using GameTweaks.Options;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using UnityEngine;

namespace GameTweaks.Tweaks;

// This has so many edge cases and some buttons simply refuse to work with this system but hey, it's funny
public sealed class DontFearTheReaperTweak : AbstractGameTweak
{
    public override string Name => MiraLocaleManager.Get("TweakDontFearTheReaper");
    public override Color Color => TweakPalette.DontFearTheReaperColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.DontFearTheReaperTweak;

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

        var role = PlayerControl.LocalPlayer.GetRoleWhenAlive();
        foreach (var button in CustomButtonManager.Buttons.Shuffle())
        {
            if (button is IKillButton)
            {
                continue;
            }
            if (!button.Enabled(role))
            {
                continue;
            }

            var deathUsable = AccessTools.Property(button.GetType(), nameof(TownOfUsButton.UsableInDeath));
            if (deathUsable != null && (bool)deathUsable.GetValue(button)!)
            {
                continue;
            }

            var afterlifeButton = CustomButtonSingleton<AfterlifeButton>.Instance;
            afterlifeButton.MimicButton(button);
            afterlifeButton.Used = false;
            break;
        }
    }
}