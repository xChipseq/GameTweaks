using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs;

namespace GameTweaks;

[BepInAutoPlugin("chipseq.gametweaks", "Game Tweaks")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(TownOfUsPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class GameTweaksPlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Game\nTweaks";
    public ConfigFile GetConfigFile() => Config;

    public static bool IsMobile => Constants.GetPlatformType() is Platforms.Android or Platforms.IPhone;
    public static bool DevMode => false;

    public override void Load()
    {
        Harmony.PatchAll();
        MiraLocaleManager.Register(Id);
        GameTweaksManager.RegisterTweaks(GetType().Assembly);
        ReactorCredits.Register<GameTweaksPlugin>(ReactorCredits.AlwaysShow);
    }
}