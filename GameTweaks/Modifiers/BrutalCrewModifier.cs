using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities;

namespace GameTweaks.Modifiers;

public sealed class BrutalCrewModifier : AllianceGameModifier
{
    public override string ModifierName => "Brutal";
    public override int GetAssignmentChance() => 0;
    public override bool HideOnUi => true;
    public override bool GetsPunished => false;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Player.IsCrewmate())
        {
            Player.RemoveModifier(this);
        }
    }
}