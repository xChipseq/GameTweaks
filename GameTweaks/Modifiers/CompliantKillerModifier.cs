using System.Linq;
using GameTweaks.Options;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace GameTweaks.Modifiers;

public sealed class CompliantKillerModifier : AllianceGameModifier
{
    public override string ModifierName => "Compliant Killer";
    public override int GetAssignmentChance() => OptionGroupSingleton<TweaksOptions>.Instance.CompliantKillersTweak ? 100 : 0;
    public override int GetAmountPerGame() => int.MaxValue;
    public override bool IsModifierValidOn(RoleBehaviour role) =>
        role.GetRoleAlignment() == RoleAlignment.NeutralKilling;

    public override string GetDescription() => "Work together with other neutral killers!";
    public override bool HideOnUi => false; // TODO: hide this later
    public override bool CrewContinuesGame => false;
    public override AlliedFaction TrueFactionType => AlliedFaction.NeutralKiller;

    public override void OnActivate()
    {
        base.OnActivate();
        if (Player.AmOwner)
        {
            var others = ModifierUtils.GetPlayersWithModifier<CompliantKillerModifier>().Where(x => !x.AmOwner);
            foreach (var other in others)
            {
                other.AddModifier<CompliantRevealModifier>((ushort)other.Data.RoleType);
            }
        }
        //Warning($"{Player.Data.PlayerName} is added to the compliant killers team");
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        if (Player.AmOwner)
        {
            var others = ModifierUtils.GetActiveModifiers<CompliantRevealModifier>();
            foreach (var other in others)
            {
                other.Player.RemoveModifier(other);
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Player.Data.Role.GetRoleAlignment() != RoleAlignment.NeutralKilling)
        {
            Player.RemoveModifier(this);
        }
    }

    public override bool? DidWin(GameOverReason reason)
    {
        var killerAlliance = ModifierUtils.GetPlayersWithModifier<CompliantKillerModifier>().Count(x => !x.HasDied());
        if (MiscUtils.KillersAliveCount > killerAlliance)
        {
            return false;
        }

        return killerAlliance >= Helpers.GetAlivePlayers().Count - killerAlliance;
    }
}