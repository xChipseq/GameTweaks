using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace GameTweaks.Events;

public static class GeneralEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var tweak in GameTweaksManager.Instance!.ActiveTweaks)
        {
            tweak.OnRoundStart(@event.TriggeredByIntro);
        }
    }
}