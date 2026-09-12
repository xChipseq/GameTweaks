using System.Linq;
using GameTweaks.Options;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace GameTweaks.Tweaks;

public sealed class DivineInterventionTweak : AbstractGameTweak
{
    public override string Name => MiraLocaleManager.Get("TweakDivineIntervention");
    public override Color Color => TweakPalette.DivineInterventionColor;
    public override bool IsEnabled() => OptionGroupSingleton<TweaksOptions>.Instance.DivineInterventionTweak;

    public static PlainShipRoom? ProtectedRoom;

    public override void OnRoundStart(bool gameStart)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var ship = ShipStatus.Instance;
        var rooms = ship.AllRooms.ToList();
        PlainShipRoom selected;
        do
        {
            selected = rooms.Random()!;
        } while (!ship.FastRooms.ContainsValue(selected));
        RpcPickProtectedRoom(PlayerControl.LocalPlayer, selected.RoomId);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (!GameTweaksManager.Instance!.IsActive<DivineInterventionTweak>())
        {
            return;
        }
        if (MeetingHud.Instance)
        {
            return;
        }

        var killer = @event.Source;
        var victim = @event.Target;
        if (killer == victim)
        {
            return;
        }

        var room = Helpers.GetRoom(victim.GetTruePosition());
        if (ProtectedRoom == room)
        {
            @event.Cancel();

            if (killer.AmOwner)
            {
                var anim = Object.Instantiate(RoleManager.Instance.protectAnim, victim.transform);
                anim.SetMaskLayerBasedOnWhoShouldSee(true);
                anim.Renderer.color = TweakPalette.DivineInterventionColor;
                anim.Play(victim, null, !killer.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
            }
        }
    }

    [MethodRpc((uint)TweakRpcCalls.DivineInterventionPickRoom)]
    public static void RpcPickProtectedRoom(PlayerControl player, SystemTypes room)
    {
        ProtectedRoom = ShipStatus.Instance.FastRooms[room];
    }
}