using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cpp2IL.Core.Extensions;
using GameTweaks.Options;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modules.Localization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameTweaks.Tweaks;

public sealed class AnomalyTweak : AbstractGameTweak
{
    public override string Name => TouLocale.Get("TweakAnomaly");
    public override Color Color => TweakPalette.AnomalyColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.AnomalyTweak;

    private static readonly Dictionary<Type, Type[]> exclusions = new()
    {
        { typeof(VipTweak), [typeof(PerfectCrewTweak), typeof(ChatWhispersTweak), typeof(CompliantKillersTweak)] },
        { typeof(PerfectCrewTweak), [typeof(VipTweak)] },
        { typeof(ChatWhispersTweak), [typeof(VipTweak)] },
        { typeof(CompliantKillersTweak), [typeof(VipTweak)] },
    };

    public override void Start()
    {
        if (!AmongUsClient.Instance.AmHost) // host assigns anomaly tweaks
        {
            return;
        }

        Coroutines.Start(CoStart());
    }

    [MethodRpc((uint)TweakRpcCalls.AnomalySelectTweak)]
    public static void RpcAnomalySelectTweak(PlayerControl player, ushort tweakId)
    {
        Coroutines.Start(CoSelectTweak(tweakId));
    }

    private static IEnumerator CoStart()
    {
        var instance = GameTweaksManager.Instance!;
        var tweaks = GameTweaksManager.AllTweaks
            .Where(x => x is not AnomalyTweak && !instance.IsActive(x.Id))
            .ToList();
        if (tweaks.Count == 0)
        {
            Warning("No valid tweaks found for Anomaly");
            yield break;
        }

        tweaks = tweaks.Shuffle().ToList();
        var chance = 100f;
        var selected = new List<AbstractGameTweak>();
        while (Random.Range(0f, 101f) <= chance)
        {
            if (tweaks.Count == 0)
                break;
            var tweak = tweaks.RemoveAndReturn(0);
            var conflicts = exclusions.GetValueOrDefault(tweak.GetType()) ?? [];
            if (selected.Any(s => conflicts.Contains(s.GetType())))
                continue;

            selected.Add(tweak);
            chance /= 2; // half the chance every time
        }

        while (!PlayerControl.LocalPlayer) // sometimes we're faster than the player for some reason
        {
            yield return null;
        }
        selected.Do(x => RpcAnomalySelectTweak(PlayerControl.LocalPlayer, x.Id));
    }

    private static IEnumerator CoSelectTweak(ushort tweakId)
    {
        while (GameTweaksManager.Instance == null)
        {
            yield return null;
        }

        var instance = GameTweaksManager.Instance;
        if (!instance.IsActive<AnomalyTweak>())
        {
            Error("RpcAnomalySelectTweak: Anomaly is not active");
            yield break;
        }

        var tweak = instance.GetTweakType(tweakId);
        if (tweak == null)
        {
            Error($"RpcAnomalySelectTweak: ID {tweakId} not found");
            yield break;
        }

        instance.AddTweak(tweak);
        Warning($"RpcAnomalySelectTweak: Selected {tweakId}");
    }
}