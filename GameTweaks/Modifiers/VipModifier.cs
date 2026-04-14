using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace GameTweaks.Modifiers;

public sealed class VipModifier(ushort roleType) : RevealModifier((int)ChangeRoleResult.UpdateInfo, false, RoleManager.Instance.GetRole((RoleTypes)roleType))
{
    public override string ModifierName => "VIP";
    public override bool HideOnUi => false;
    public override bool RevealRole => Player.AmOwner || PlayerControl.LocalPlayer.IsCrewmate()
                                                      || (PlayerControl.LocalPlayer.HasDied() &&
                                                          OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow);
    public override string GetDescription() => TouLocale.Get("TweakVipModifierDesc");

    public override void Update()
    {
        base.Update();
        ExtraRoleText = RevealRole ? $" {TweakPalette.VipColor.ToTextColor()}{TouLocale.Get("TweakVipModifier")}</color>" : string.Empty;
    }
}