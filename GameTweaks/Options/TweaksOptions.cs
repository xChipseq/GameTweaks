using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using UnityEngine;

namespace GameTweaks.Options;

public sealed class TweaksOptions : AbstractOptionGroup
{
    public override string GroupName => "<b>Tweaks</b>";
    public override Color GroupColor => TweakPalette.ModColor;

    [ModdedToggleOption("Anomaly")]
    public bool AnomalyTweak { get; set; } = false;

    [ModdedToggleOption("Brutal Crew")]
    public bool BrutalCrewTweak { get; set; } = false;

    [ModdedToggleOption("Chat Whispers")]
    public bool ChatWhispersTweak { get; set; } = false;

    [ModdedToggleOption("Compliant Killers")]
    public bool CompliantKillersTweak { get; set; } = false;

    [ModdedToggleOption("Divine Intervention")]
    public bool DivineInterventionTweak { get; set; } = false;

    [ModdedToggleOption("Don't Fear The Reaper")]
    public bool DontFearTheReaperTweak { get; set; } = false;

    [ModdedToggleOption("Election")]
    public bool ElectionTweak { get; set; } = false;

    [ModdedToggleOption("Graveyard")]
    public bool GraveyardTweak { get; set; } = false;

    [ModdedToggleOption("Haunted House")]
    public bool HauntedHouseTweak { get; set; } = false;

    [ModdedToggleOption("Impostoromicon")]
    public bool ImpostoromiconTweak { get; set; } = false;

    // [ModdedToggleOption("Keeping Them Busy")]
    // public bool KeepingThemBusyTweak { get; set; } = false;

    [ModdedToggleOption("Live Voting")]
    public bool LiveVotingTweak { get; set; } = false;

    [ModdedToggleOption("Merry Go Round")]
    public bool MerryGoRoundTweak { get; set; } = false;

    [ModdedToggleOption("No Skip")]
    public bool NoSkipTweak { get; set; } = false;

    [ModdedToggleOption("Perfect Crew")]
    public bool PerfectCrewTweak { get; set; } = false;

    [ModdedToggleOption("Tactical Deployment")]
    public bool TacticalDeploymentTweak { get; set; } = false;

    [ModdedToggleOption("VIP")]
    public bool VipTweak { get; set; } = false;
}