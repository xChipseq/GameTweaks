using GameTweaks.GameOvers;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers.Types;

namespace GameTweaks.Modifiers;

public sealed class VipKillerModifier : GameModifier
{
    public override string ModifierName => "you murderer!!!";
    public override bool HideOnUi => true;
    public override int GetAssignmentChance() => 0;
    public override int GetAmountPerGame() => 0;

    public override bool? DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<VipDeadGameOver>();
    }
}