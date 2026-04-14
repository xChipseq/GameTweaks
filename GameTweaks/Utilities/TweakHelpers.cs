using System.Collections.Generic;
using System.Linq;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.Utilities;

public static class TweakHelpers
{
    public const byte SkipVoteId = 253;

    public static PlayerControl? PlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.PlayerId == id);
    }

    public static LobbyNotificationMessage Notify(string text, Color? color, Sprite? icon = null)
    {
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", color ?? Color.white, spr: icon);
        notif.Text.SetOutlineThickness(0.35f);
        notif.transform.localPosition = new Vector3(0f, 1f, -20f);
        return notif;
    }

    public static PlayerControl[] GetDeadPlayers()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(x => x.HasDied())
            .ToArray();
    }

    public static List<PlayerControl> GetCrewmates(bool includeDead = true)
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(x => x.IsCrewmate())
            .Where(x => !x.HasDied() || includeDead)
            .ToList();
    }

    public static List<PlayerControl> GetImpostors(bool includeDead = true)
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(x => x.IsImpostor())
            .Where(x => !x.HasDied() || includeDead)
            .ToList();
    }

    public static void MurderAllCrew()
    {
        var crew = Helpers.GetAlivePlayers().Where(QualifiesAsCrew);
        foreach (var crewmate in crew)
        {
            crewmate.CustomMurder(crewmate, MurderResultFlags.Succeeded, createDeadBody: false, showKillAnim: false, playKillSound: false, teleportMurderer: false);
        }
    }

    public static bool QualifiesAsCrew(PlayerControl player)
    {
        return player.IsCrewmate() && // is crewmate aligned
               !player.HasModifier<EgotistModifier>() && // not an egotist
               !player.HasModifier<CrewpostorModifier>() && // not a crewpostor
               (!player.TryGetModifier<LoverModifier>(out var lovers) || (lovers.OtherLover != null && lovers.OtherLover.IsCrewmate())); // crew-lovers
    }

    public static void ToggleKillButtons(bool active, bool includeCrew = true, bool includeNeutral = true)
    {
        var hud = HudManager.Instance;
        if (hud == null)
        {
            return;
        }

        var role = PlayerControl.LocalPlayer.Data.Role;
        if (role.IsCrewmate() && !includeCrew)
        {
            return;
        }

        if (role is not ICustomRole customRole)
        {
            hud.KillButton.gameObject.SetActive(active);
            return;
        }

        if (customRole.Team == ModdedRoleTeams.Custom && !includeNeutral)
        {
            return;
        }

        hud.KillButton.gameObject.SetActive(active && customRole.Configuration.UseVanillaKillButton);
        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button is not IKillButton)
            {
                return;
            }

            button.Button?.ToggleVisible(active);
        }
    }

    public static void SendCustomChatNote(NetworkedPlayerInfo source, string text, bool altColors = false)
    {
        var chat = HudManager.Instance?.Chat;
        if (chat == null)
        {
            return;
        }
        if (source == null)
        {
            return;
        }

        var pooledBubble = chat.GetPooledBubble();
        pooledBubble.SetCosmetics(source);
        pooledBubble.transform.SetParent(chat.scroller.Inner);
        pooledBubble.transform.localScale = Vector3.one;
        pooledBubble.SetNotification();
        pooledBubble.SetName(text, false, false, TweakPalette.ChatWhispersColor);
        pooledBubble.SetText(string.Empty);
        pooledBubble.AlignChildren();
        chat.AlignAllBubbles();
        if (altColors)
        {
            pooledBubble.Background.color = Color.black;
            pooledBubble.TextArea.color = Color.white;
        }
        if (!chat.IsOpenOrOpening && chat.notificationRoutine == null)
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }
        if (source.Object != PlayerControl.LocalPlayer)
        {
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch = 0.5f + source.PlayerId / 15f;
        }
    }
}