using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace GameTweaks;

public static class TweakAssets
{
    public static LoadableAsset<Sprite> ImpostoromiconButton { get; } =
        new LoadableResourceAsset("GameTweaks.Resources.Impostoromicon.png");
    public static LoadableAsset<Sprite> ImpostoromiconButtonActive { get; } =
        new LoadableResourceAsset("GameTweaks.Resources.ImpostoromiconActive.png");
}