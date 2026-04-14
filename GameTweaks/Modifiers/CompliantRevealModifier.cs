using AmongUs.GameOptions;
using TownOfUs.Modifiers;
using UnityEngine;

namespace GameTweaks.Modifiers;

public class CompliantRevealModifier(ushort role) : RevealModifier((int)ChangeRoleResult.RemoveModifier, true, RoleManager.Instance.GetRole((RoleTypes)role))
{
    public override string ModifierName => "Team Reveal";
    public override bool HideOnUi => true;
    public override Color? NameColor => TweakPalette.ElectionColor; // intentional for the golden color :>
}