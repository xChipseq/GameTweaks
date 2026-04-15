using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Buttons;

public sealed class AfterlifeButton : TownOfUsTargetButton<MonoBehaviour>
{
    public override string Name => "You are dead";
    public override bool UsableInDeath => true;
    public override float Cooldown => 10;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ExeTormentSprite;

    public enum ButtonTargetType
    {
        None,
        Player,
        Body,
    }

    public bool Used;
    public CustomActionButton? ClonedButton;
    public ButtonTargetType TargetType;

    protected override void OnClick()
    {
        if (ClonedButton == null)
        {
            return;
        }

        if (TargetType != ButtonTargetType.None)
        {
            var targetProperty = AccessTools.Property(ClonedButton.GetType(), nameof(Target));
            targetProperty.SetValue(ClonedButton, GetTarget());
        }

        var clickMethod = AccessTools.Method(ClonedButton.GetType(), nameof(OnClick));
        clickMethod.Invoke(ClonedButton, []);
        Used = true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return !Used && ClonedButton != null && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.HasDied();
    }

    public override MonoBehaviour? GetTarget()
    {
        return TargetType switch
        {
            ButtonTargetType.Player => PlayerControl.LocalPlayer?.GetClosestLivingPlayer(
                !PlayerControl.LocalPlayer.IsImpostorAligned(), Distance, true),
            ButtonTargetType.Body => PlayerControl.LocalPlayer?.GetNearestDeadBody(Distance),
            _ => null
        };
    }

    public override void SetOutline(bool active)
    {
        switch (TargetType)
        {
            case ButtonTargetType.Player:
                Target?.Cast<PlayerControl>().cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(TextOutlineColor));
                break;
            case ButtonTargetType.Body:
                Target?.Cast<DeadBody>().bodyRenderers.Do(x => x.SetOutline(active ? TextOutlineColor : null));
                break;
        }
    }

    public void MimicButton(CustomActionButton button)
    {
        OverrideName(button.Name);
        OverrideSprite(button.Sprite.LoadAsset());
        SetTextOutline(button.TextOutlineColor);
        ClonedButton = button;

        var target = AccessTools.Method(button.GetType(), nameof(GetTarget));
        TargetType = ButtonTargetType.None;
        if (target == null)
        {
            return;
        }

        if (target.ReturnType == typeof(PlayerControl))
        {
            TargetType = ButtonTargetType.Player;
        }
        else if (target.ReturnType == typeof(DeadBody))
        {
            TargetType = ButtonTargetType.Body;
        }
    }

    public override bool CanUse()
    {
        return TargetType switch
        {
            ButtonTargetType.None => PlayerControl.LocalPlayer.moveable &&
                                     ((EffectActive && IsEffectCancellable()) ||
                                      (!EffectActive && (!LimitedUses || UsesLeft > 0))),
            _ => base.CanUse()
        };
    }

    public override bool CanClick()
    {
        return TargetType switch
        {
            ButtonTargetType.None => (EffectActive ? IsEffectCancellable() : Timer <= 0) && CanUse(),
            _ => base.CanUse()
        };
    }
}