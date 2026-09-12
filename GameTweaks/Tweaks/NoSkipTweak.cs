using GameTweaks.Options;
using GameTweaks.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using TMPro;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class NoSkipTweak : AbstractGameTweak
{
    public override string Name => MiraLocaleManager.Get("TweakNoSkip");
    public override Color Color => TweakPalette.NoSkipColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.NoSkipTweak;

    private TextMeshPro? skipText;

    public override void Update()
    {
        if (!MeetingHud.Instance)
        {
            return;
        }

        var hud = MeetingHud.Instance;
        hud.SkipVoteButton?.gameObject.SetActive(false);
        skipText ??= hud.SkippedVoting.GetComponentInChildren<TextMeshPro>();
        skipText.enabled = false;
    }

    // TODO: Make these (and any other events) private when my commit gets into a mira release
    [RegisterEvent]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<NoSkipTweak>())
        {
            return;
        }

        if (@event.TargetId == TweakHelpers.SkipVoteId)
        {
            @event.Cancel();
        }
    }

    [RegisterEvent(100)]
    public static void DummyVoteEventHandler(DummyVoteEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<NoSkipTweak>())
        {
            return;
        }

        @event.CanSkip = false;
    }
}