using System.Linq;
using GameTweaks.Modifiers;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace GameTweaks.GameOvers;

public sealed class VipDeadGameOver : CustomGameOver
{
    private string winningTeam = string.Empty;
    private bool soloWinner;
    private Color winnerColor;

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        var vips = ModifierUtils.GetPlayersWithModifier<VipModifier>().ToList();
        if (winners.Length == 0 || vips.Count != 1 || !vips[0].HasDied())
        {
            Error("Invalid VipDead game over!");
            return false;
        }

        if (winners.Any(x => x.Role is VampireRole vamp))
        {
            winningTeam = winners[0].Role.NiceName;
            winnerColor = winners[0].Role.TeamColor;
            return true;
        }

        if (winners.Length == 1 && !winners[0].Role.IsImpostor)
        {
            var winner = winners[0].Role;
            soloWinner = true;
            winningTeam = winner.NiceName;
            winnerColor = winner.TeamColor;
            return true;
        }

        winningTeam = "Impostors";
        winnerColor = Palette.ImpostorRed;
        return true;
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, winnerColor);

        var text = Object.Instantiate(endGameManager.WinText);
        var winText = soloWinner ? MiraLocaleManager.Get("SoloWin") : MiraLocaleManager.Get("TeamWin");
        var vipGameOverText = MiraLocaleManager.Get("TweakVipGameOver");
        winText = winText.Replace("<role>", winningTeam);
        text.text = $"{TweakPalette.VipColor.ToTextColor()}{vipGameOverText}</color>\n{winText}!";
        text.color = winnerColor;
        GameHistory.WinningFaction = $"{TweakPalette.VipColor.ToTextColor()}{vipGameOverText}</color> - <color=#{winnerColor.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TeamWin").Replace("<role>", winningTeam)}</color>";

        endGameManager.WinText.transform.localPosition = new Vector3(0, 2f, -14);
        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.25f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = Vector3.one;

        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}