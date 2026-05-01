using HarmonyLib;
using TownOfUs.Assets;
using TownOfUs.Modules.Wiki;

namespace GameTweaks.Patches;

[HarmonyPatch]
public static class WikiPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(IngameWikiMinigame), nameof(IngameWikiMinigame.AddNewTerms))]
    public static void AddNewTermsPostfix(IngameWikiMinigame instance)
    {
        instance._activeTerms.Add(new TermWikiInfo("TweakWikiTitle", "TweakWikiTerms", TouAssets.TerminologySprite));
    }
}