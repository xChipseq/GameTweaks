using MiraAPI.Modifiers;

namespace GameTweaks.Modifiers;

public sealed class ImpostoromiconModifier : BaseModifier
{
    public override string ModifierName => "Impostoromicon";
    public override bool HideOnUi => false; // TODO: hide this later
    public override string GetDescription() => "You are able to kill.";
}