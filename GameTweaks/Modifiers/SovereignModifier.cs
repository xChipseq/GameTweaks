using MiraAPI.Modifiers;
using TownOfUs.Modules.Localization;

namespace GameTweaks.Modifiers;

public sealed class SovereignModifier(int votes) : BaseModifier
{
    public override string ModifierName => "Sovereign";
    public override bool HideOnUi => false;
    public override string GetDescription() =>
        TouLocale.GetParsed("TweakElectionSovereignDesc").Replace("<votes>", VoteCount.ToString());

    public readonly int VoteCount = votes;

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }
}