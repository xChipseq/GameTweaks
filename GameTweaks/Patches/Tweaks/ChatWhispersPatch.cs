using System;
using System.Linq;
using GameTweaks.Tweaks;
using HarmonyLib;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using TownOfUs.Patches.Misc;
using TownOfUs.Utilities;

namespace GameTweaks.Patches.Tweaks;

[HarmonyPatch]
public static class ChatWhispersPatch
{
    private static readonly string[] WHISPER_COMMANDS = ["/whisper", "/w"];
    private static readonly string[] HELP_COMMANDS = ["/whelp"];

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ChatPatches), nameof(ChatPatches.FirstPrefix))]
    public static void CommandPatch([HarmonyArgument(0)] ChatController instance, ref bool __result)
    {
        if (!__result)
        {
            return;
        }
        if (!MeetingHud.Instance)
        {
            return;
        }
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }
        if (GameTweaksManager.Instance == null || !GameTweaksManager.Instance.IsActive<ChatWhispersTweak>())
        {
            return;
        }

        void Finalize()
        {
            instance.freeChatField.Clear();
            instance.quickChatMenu.Clear();
            instance.quickChatField.Clear();
            instance.UpdateChatMode();
        }

        var text = instance.freeChatField.Text.WithoutRichText();
        if (text.Length < 2)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer.Data;
        var split = text.Split(' ', 3);
        if (HELP_COMMANDS.Any(x => split[0].Equals(x, StringComparison.InvariantCultureIgnoreCase)))
        {
            var helpMsg = MiraLocaleManager.Get("TweakChatWhispersHelpCmd");
            MiscUtils.AddSystemChat(local, MiraLocaleManager.Get("TweakChatWhispersCmdTitle"), helpMsg, altColors: true);
            Finalize();
            __result = false;
            return;
        }
        if (!WHISPER_COMMANDS.Any(x => split[0].Equals(x, StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        __result = false;
        if (split.Length != 3)
        {
            var errorMsg = MiraLocaleManager.Get("TweakChatWhispersCmdError");
            MiscUtils.AddSystemChat(local, MiraLocaleManager.Get("TweakChatWhispersCmdTitle"), errorMsg, altColors: true);
            Finalize();
            return;
        }

        PlayerControl target;
        var playerArg = split[1];
        if (int.TryParse(playerArg, out var number))
        {
            var states = MeetingHud.Instance.playerStates.ToArray();
            var sorted = states // snippet from au's source that sorts buttons (the states array isnt actually sorted)
                .OrderBy(p => !p.AmDead ? 0 : 50)
                .ThenBy(p => p.PlayerId).ToArray<PlayerVoteArea>();

            if (number > states.Length)
            {
                var numErrorMsg = MiraLocaleManager.Get("TweakChatWhispersCmdNumError").Replace("<num>", number.ToString());
                MiscUtils.AddSystemChat(local, MiraLocaleManager.Get("TweakChatWhispersCmdTitle"), numErrorMsg, altColors: true);
                Finalize();
                return;
            }

            target = sorted[number - 1].GetPlayer()!;
        }
        else
        {
            var player = PlayerControl.AllPlayerControls
                .ToArray()
                .FirstOrDefault(x => x.Data.PlayerName.Equals(playerArg, StringComparison.InvariantCultureIgnoreCase) ||
                                     x.Data.PlayerName.StartsWith(playerArg, StringComparison.InvariantCultureIgnoreCase));
            if (player == null)
            {
                var notFoundErrorMsg = MiraLocaleManager.Get("TweakChatWhispersCmdNotFoundError").Replace("<player>", playerArg);
                MiscUtils.AddSystemChat(local, MiraLocaleManager.Get("TweakChatWhispersCmdTitle"), notFoundErrorMsg, altColors: true);
                Finalize();
                return;
            }

            target = player;
        }

        Finalize();
        if (target.AmOwner)
        {
            MiscUtils.AddSystemChat(local, MiraLocaleManager.Get("TweakChatWhispersCmdTitle"), MiraLocaleManager.Get("TweakChatWhispersCmdWhisperYourself"), altColors: true);
            return;
        }

        if (target.HasDied())
        {
            var deadErrorMsg = MiraLocaleManager.Get("TweakChatWhispersCmdWhisperDead").Replace("<player>", target.Data.PlayerName);
            MiscUtils.AddSystemChat(local, MiraLocaleManager.Get("TweakChatWhispersCmdTitle"), deadErrorMsg, altColors: true);
            return;
        }

        var content = split[2];
        ChatWhispersTweak.RpcWhisperPlayer(local.Object, target, content);
    }
}