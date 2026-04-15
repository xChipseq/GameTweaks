using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using UnityEngine;

namespace GameTweaks.Options;

public sealed class TweaksOptions : AbstractOptionGroup
{
    public override string GroupName => "<b>Tweaks</b>";
    public override Color GroupColor => TweakPalette.ModColor;

    [ModdedToggleOption("TweakAnomalyOption")]
    public bool AnomalyTweak { get; set; } = false;

    [ModdedToggleOption("TweakBrutalCrewOption")]
    public bool BrutalCrewTweak { get; set; } = false;

    [ModdedToggleOption("TweakChatWhispersOption")]
    public bool ChatWhispersTweak { get; set; } = false;

    [ModdedToggleOption("TweakCompliantKillersOption")]
    public bool CompliantKillersTweak { get; set; } = false;

    [ModdedToggleOption("TweakDivineInterventionOption")]
    public bool DivineInterventionTweak { get; set; } = false;

    [ModdedToggleOption("TweakDontFearTheReaperOption")]
    public bool DontFearTheReaperTweak { get; set; } = false;

    [ModdedToggleOption("TweakElectionOption")]
    public bool ElectionTweak { get; set; } = false;

    [ModdedToggleOption("TweakGraveyardOption")]
    public bool GraveyardTweak { get; set; } = false;

    [ModdedToggleOption("TweakHauntedHouseOption")]
    public bool HauntedHouseTweak { get; set; } = false;

    [ModdedToggleOption("TweakImpostoromiconOption")]
    public bool ImpostoromiconTweak { get; set; } = false;

    // [ModdedToggleOption("TweakKeepingThemBusyOption")]
    // public bool KeepingThemBusyTweak { get; set; } = false;

    [ModdedToggleOption("TweakLiveVotingOption")]
    public bool LiveVotingTweak { get; set; } = false;

    [ModdedToggleOption("TweakMerryGoRoundOption")]
    public bool MerryGoRoundTweak { get; set; } = false;

    [ModdedToggleOption("TweakNoSkipOption")]
    public bool NoSkipTweak { get; set; } = false;

    [ModdedToggleOption("TweakPerfectCrewOption")]
    public bool PerfectCrewTweak { get; set; } = false;

    [ModdedToggleOption("TweakTacticalDeploymentOption")]
    public bool TacticalDeploymentTweak { get; set; } = false;

    [ModdedToggleOption("TweakVipOption")]
    public bool VipTweak { get; set; } = false;
}