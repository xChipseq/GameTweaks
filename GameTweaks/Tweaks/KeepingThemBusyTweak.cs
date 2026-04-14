/*
using System.Linq;
using GameTweaks.Options;
using GameTweaks.Utilities;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class KeepingThemBusyTweak : AbstractGameTweak
{
    public override string Name => "Keeping Them Busy";
    public override Color Color => TweakPalette.KeepingThemBusyColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.KeepingThemBusyTweak;

    public override void OnRoundStart(bool gameStart)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        if (gameStart)
        {
            return;
        }

        var ship = ShipStatus.Instance;
        foreach (var player in TweakHelpers.GetCrewmates(false))
        {
            var tasks = player.myTasks.ToArray().ToList();
            foreach (var task in tasks)
            {
                if (task.IsComplete)
                {
                    continue;
                }
                if (ship.CommonTasks.Any(x => task.TaskType == x.TaskType))
                {
                    continue;
                }

                var replacementList = (ship.LongTasks.Any(x => task.TaskType == x.TaskType)
                    ? ship.LongTasks : ship.ShortTasks).ToList();
                NormalPlayerTask replacement;
                do
                {
                    replacement = replacementList.Random()!;
                } while (replacement.TaskType == task.TaskType || !(task.TryCast<DivertPowerTask>() && replacement.TryCast<DivertPowerTask>()));

                RpcReplaceTask(player, task.TaskType, (byte)replacement.Index);
            }
        }
    }

    [MethodRpc((uint)TweakRpcCalls.KeepingThemBusyReplaceTask)]
    public static void RpcReplaceTask(PlayerControl player, TaskTypes target, byte replacementIdx)
    {
        var task = player.myTasks.ToArray().FirstOrDefault(x => x.TaskType == target);
        if (task == null)
        {
            Error("RpcReplaceTask: Target not found");
            return;
        }

        var replacement = ShipStatus.Instance.GetTaskById(replacementIdx);
        Warning($"Replacing {target} with {replacement.TaskType}");
        task.TaskType = replacement.TaskType;
        task.Index = replacement.Index;
        task.Initialize();
    }
}
*/