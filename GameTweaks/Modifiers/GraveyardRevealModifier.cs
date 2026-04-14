using AmongUs.GameOptions;
using TownOfUs.Modifiers;

namespace GameTweaks.Modifiers;

public sealed class GraveyardRevealModifier(ushort role) : RevealModifier((int)ChangeRoleResult.Nothing, true, RoleManager.Instance.GetRole((RoleTypes)role))
{
    public override string ModifierName => "Graveyard Reveal";
    public override bool HideOnUi => true;
}