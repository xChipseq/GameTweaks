using GameTweaks.Options;
using GameTweaks.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class ChatWhispersTweak : AbstractGameTweak
{
    public override string Name => MiraLocaleManager.Get("TweakChatWhispers");
    public override Color Color => TweakPalette.ChatWhispersColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.ChatWhispersTweak;

    private static bool shownOnce;

    [MethodRpc((uint)TweakRpcCalls.ChatWhispersWhisper)]
    public static void RpcWhisperPlayer(PlayerControl source, PlayerControl target, string message)
    {
        var msg = MiraLocaleManager.Get("TweakChatWhispersNotification")
            .Replace("<source>", source.Data.PlayerName)
            .Replace("<target>", target.Data.PlayerName);
        TweakHelpers.SendCustomChatNote(source.Data, msg, true);
        if (!source.AmOwner && !target.AmOwner)
        {
            return;
        }

        var nameText = source.AmOwner
            ? MiraLocaleManager.Get("TweakChatWhispersTo").Replace("<name>", target.Data.PlayerName)
            : MiraLocaleManager.Get("TweakChatWhispersFrom").Replace("<name>", source.Data.PlayerName);
        MiscUtils.AddFakeChat(source.Data, nameText, message, altColors: true, onLeft: target.AmOwner);
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        shownOnce = false;
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (shownOnce)
        {
            return;
        }
        if (!GameTweaksManager.Instance!.IsActive<ChatWhispersTweak>())
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local.HasDied())
        {
            return;
        }

        MiscUtils.AddSystemChat(local.Data,
            "Whispers",
            MiraLocaleManager.Get("TweakChatWhispersHelp"),
            altColors: true);
        shownOnce = true;
    }
}