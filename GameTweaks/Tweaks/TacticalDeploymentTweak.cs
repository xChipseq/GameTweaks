using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameTweaks.Components;
using GameTweaks.Options;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class TacticalDeploymentTweak : AbstractGameTweak
{
    public override string Name => MiraLocaleManager.Get("TweakTacticalDeployment");
    public override Color Color => TweakPalette.TacticalDeploymentColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.TacticalDeploymentTweak;

    public readonly struct CustomSpawnLocation(SystemTypes system, Vector3 pos)
    {
        public SystemTypes System => system;
        public Vector3 Position => pos;
    }

    public static Dictionary<ExpandedMapNames, CustomSpawnLocation[]> Positions { get; } = new()
    {
        [ExpandedMapNames.Skeld] = [
            new CustomSpawnLocation(SystemTypes.Nav, new Vector3(16.5899f, -4.6398f)),
            new CustomSpawnLocation(SystemTypes.Reactor, new Vector3(-20.276f, -5.3997f)),
            new CustomSpawnLocation(SystemTypes.MedBay, new Vector3(-9.0679f, -3.0626f)),
            new CustomSpawnLocation(SystemTypes.Admin, new Vector3(4.6242f, -7.4291f)),
            new CustomSpawnLocation(SystemTypes.Storage, new Vector3(-1.7465f, -15.7614f))
        ],
        [ExpandedMapNames.Dleks] = [
            new CustomSpawnLocation(SystemTypes.Nav, new Vector3(-16.5899f, -4.6398f)),
            new CustomSpawnLocation(SystemTypes.Reactor, new Vector3(20.276f, -5.3997f)),
            new CustomSpawnLocation(SystemTypes.MedBay, new Vector3(9.0679f, -3.0626f)),
            new CustomSpawnLocation(SystemTypes.Admin, new Vector3(-4.6242f, -7.4291f)),
            new CustomSpawnLocation(SystemTypes.Storage, new Vector3(1.7465f, -15.7614f))
        ],
        [ExpandedMapNames.MiraHq] = [
            new CustomSpawnLocation(SystemTypes.Launchpad, new Vector3(-4.4231f, 2.2586f)),
            new CustomSpawnLocation(SystemTypes.Laboratory, new Vector3(6.1398f, 12.5369f)),
            new CustomSpawnLocation(SystemTypes.Greenhouse, new Vector3(17.8617f, 19.0059f)),
            new CustomSpawnLocation(SystemTypes.Cafeteria, new Vector3(22.1015f, 0.3318f)),
        ],
        [ExpandedMapNames.Polus] = [
            new CustomSpawnLocation(SystemTypes.Electrical, new Vector3(5.4841f, -9.9841f)),
            new CustomSpawnLocation(SystemTypes.LifeSupp, new Vector3(3.2421f, -21.7374f)),
            new CustomSpawnLocation(SystemTypes.Office, new Vector3(20.9678f, -19.4227f)),
            new CustomSpawnLocation(SystemTypes.Specimens, new Vector3(36.4897f, -21.6416f)),
            new CustomSpawnLocation(SystemTypes.Laboratory, new Vector3(31.4381f, -7.6176f)),
        ],
        [ExpandedMapNames.Fungle] = [
            new CustomSpawnLocation(SystemTypes.Beach, new Vector3(-22.6943f, -0.7906f)),
            new CustomSpawnLocation(SystemTypes.Jungle, new Vector3(-0.4936f, -10.8778f)),
            new CustomSpawnLocation(SystemTypes.Reactor, new Vector3(21.37f, -7.3811f)),
            new CustomSpawnLocation(SystemTypes.MiningPit, new Vector3(12.786f, 4.6588f)),
            new CustomSpawnLocation(SystemTypes.Comms, new Vector3(17.7045f, 13.2126f)),
        ],
    };

    public static bool IsMapValid()
    {
        var map = MiscUtils.GetCurrentMap;
        return Positions.ContainsKey(map);
    }

    public static IEnumerator PickDeployment()
    {
        var map = MiscUtils.GetCurrentMap;
        if (!Positions.TryGetValue(map, out var list) || list.Length == 0)
        {
            Error($"{map} is not valid Tactical Deployment");
            yield break;
        }

        var minigame = CustomSpawnInMinigame.Create();
        minigame.Locations = list;
        minigame.BeginCustom();
        yield return minigame.WaitForFinish();
    }
}